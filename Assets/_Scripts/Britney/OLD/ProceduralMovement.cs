using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace MainController
{
    public class ProceduralMovementOld : MonoBehaviour, IPlayerController
    {
        public static ProceduralMovementOld PlayerInstance { get; private set; }

        // --- IPlayerController interface ---
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector2 RawMovement { get; private set; }
        public bool Grounded => _colDown; // Combined slope or ground check

        // Optional events
        public event Action<bool> GroundedChanged;
        public event Action Jumped;
        public event Action Sprinting;
        public event Action Stepped;
        public event Action Decelerating;

        // --------------------------------------------------
        // Internal Movement Variables
        // --------------------------------------------------
        private Vector3 _lastPosition;
        public float _currentHorizontalSpeed,
            _currentVerticalSpeed;
        private float _moveMultiplier = 1f;
        private bool _hasJumped = false; // Did we actually jump this airtime?

        [BoxGroup("MAIN"), SerializeField]
        private float _sprintMultiplier = 2f;

        [BoxGroup("MAIN"), SerializeField]
        private float _decelerationMultiplier = 0.5f;

        [BoxGroup("MAIN"), SerializeField]
        private Rigidbody2D _rigidbody;

        [BoxGroup("MAIN"), SerializeField]
        private GatherMovementInput _movementInput;

        public float DistancePerFrame;

        // --------------------------------------------------
        // GRAVITY & JUMP
        // --------------------------------------------------
        [
            BoxGroup("JUMP/Gravity"),
            SerializeField,
            Tooltip("Base gravity scale (positive = downward).")
        ]
        private float _gravityScale = 5f;

        [BoxGroup("JUMP/Gravity"), SerializeField, Tooltip("Extra gravity ONLY on jump release.")]
        private float _releaseGravityMultiplier = 2f;

        [
            BoxGroup("JUMP/Heights"),
            SerializeField,
            Tooltip("Full jump height in world units if jump is held entire time.")
        ]
        private float _jumpHeight = 5f;

        [
            BoxGroup("JUMP/Heights"),
            SerializeField,
            Tooltip("If true, holding jump => higher jump, releasing => short hop.")
        ]
        private bool _variableJumpHeight = true;

        [
            BoxGroup("JUMP/Timing"),
            SerializeField,
            Tooltip("Coyote time (jump still possible after stepping off).")
        ]
        private float _coyoteTime = 0.2f;
        private float _coyoteCounter;

        [
            BoxGroup("JUMP/Timing"),
            SerializeField,
            Tooltip("Remember a jump input before landing (jump buffering).")
        ]
        private float _jumpBufferTime = 0.15f;
        private float _jumpBufferCounter;

        [BoxGroup("JUMP/Feel"), SerializeField, Tooltip("Allow horizontal movement in mid-air.")]
        private bool _allowMidAirMovement = true;

        [
            BoxGroup("JUMP/Feel"),
            SerializeField,
            Tooltip("If true, horizontal speed goes to 0 instantly with no input.")
        ]
        private bool _immediateHorizontalStop = false;

        // --------------------------------------------------
        // RUNNING / Horizontal Movement
        // --------------------------------------------------
        [Header("RUNNING")]
        [SerializeField]
        private float _acceleration = 90f;

        [SerializeField]
        private float _moveClamp = 13f;

        [SerializeField]
        private float _deAcceleration = 60f;

        // --------------------------------------------------
        // TARGETS / IK
        // --------------------------------------------------
        [BoxGroup("TARGETS"), SerializeField]
        private Transform LeftLegTarget;

        [BoxGroup("TARGETS"), SerializeField]
        private Transform RightLegTarget;

        [BoxGroup("TARGETS"), SerializeField]
        private Transform LeftArmTarget;

        [BoxGroup("TARGETS"), SerializeField]
        private Transform RightArmTarget;

        [BoxGroup("TARGETS"), SerializeField]
        private Transform Root;

        // Step animation
        [BoxGroup("STEP"), SerializeField]
        private float _halfStepLength = 1f;

        [BoxGroup("STEP"), SerializeField]
        private float _maxLegsHeight = 2f;

        [BoxGroup("STEP"), SerializeField]
        private float _maxStepHeight = 0.5f;

        [BoxGroup("STEP"), SerializeField]
        private AnimationCurve _stepCurve;
        private bool _rightLegMove = true;

        // Arm animation
        [BoxGroup("ELSE"), SerializeField]
        private int _numberOfArmPoints = 100;
        private List<Vector3> _armBasePoints = new();

        // --------------------------------------------------
        // COLLISION / Slope
        // --------------------------------------------------
        [BoxGroup("COLLISION"), SerializeField]
        private LayerMask _groundLayer;

        [BoxGroup("COLLISION"), SerializeField]
        private Transform _groundCheck;

        [BoxGroup("COLLISION"), SerializeField]
        private float _groundedRadius = .25f;
        private bool _colDown; // final "grounded or slope" check
        private float _timeLeftGrounded;

        [BoxGroup("SLOPE"), SerializeField]
        private float _slopeCheckDistance = 0.5f;

        [BoxGroup("SLOPE"), SerializeField]
        private float _maxSlopeAngle = 70f;

        [BoxGroup("SLOPE"), SerializeField]
        private bool _isOnSlope;

        [BoxGroup("SLOPE"), SerializeField]
        private float _slopeDownAngle;

        [BoxGroup("SLOPE"), SerializeField]
        private float _slopeSideAngle;

        [BoxGroup("SLOPE"), SerializeField]
        private float _lastSlopeAngle;
        private bool _canWalkOnSlope;
        private Vector2 slopeNormalPerp;

        private void Awake()
        {
            if (PlayerInstance != null)
            {
                Debug.LogError("Found more than one Player instance in the scene!");
            }
            PlayerInstance = this;
        }

        private void Start()
        {
            _lastPosition = Root.position;
            _armBasePoints = Geometry.CalculateBezierLocalPoints(
                LeftArmTarget.position,
                Root.position,
                RightArmTarget.position,
                _numberOfArmPoints,
                Root
            );
        }

        private void Update()
        {
            SetBaseValues();
            GatherInput();
            RunCollisionChecks(); // checks ground + slope

            CalculateRun(); // horizontal
            HandleJump(); // vertical

            MoveCharacter(); // apply velocity
        }

        private void SetBaseValues()
        {
            Vector3 G = Root.position;
            DistancePerFrame = Vector2.Distance(G, _lastPosition);
            Velocity = (G - _lastPosition) / Time.deltaTime;
            _lastPosition = G;
        }

        #region Gather Input
        private void GatherInput()
        {
            Input = new FrameInput
            {
                JumpLeft = _movementInput.JumpLeftPressing,
                JumpUpPressed = _movementInput.JumpUpPressed,
                JumpUpReleased = _movementInput.JumpUpReleased,
                JumpUpPressing = _movementInput.JumpUpPressing,
                JumpRight = _movementInput.JumpRightPressing,
                X = _movementInput.XInput,
                Sprint = _movementInput.SpeedUpPressing,
                Deceleration = _movementInput.SpeedDownPressing,
            };

            // Sprint/Decel multipliers
            if (Input.Sprint)
            {
                _moveMultiplier = _sprintMultiplier;
            }
            else if (Input.Deceleration)
            {
                _moveMultiplier = _decelerationMultiplier;
            }
            else
            {
                _moveMultiplier = 1f;
            }

            // Jump buffering
            if (Input.JumpUpPressed)
            {
                _jumpBufferCounter = _jumpBufferTime;
            }
            else
            {
                if (_jumpBufferCounter > 0f)
                    _jumpBufferCounter -= Time.deltaTime;
            }
        }
        #endregion

        #region Horizontal Movement
        private void CalculateRun()
        {
            bool hasHorizontalInput = !Mathf.Approximately(Input.X, 0f);

            // if grounded or we allow mid-air movement
            if (Grounded || _allowMidAirMovement)
            {
                if (hasHorizontalInput)
                {
                    // accelerate
                    _currentHorizontalSpeed +=
                        _moveMultiplier * Input.X * _acceleration * Time.deltaTime;
                    // clamp
                    _currentHorizontalSpeed = Mathf.Clamp(
                        _currentHorizontalSpeed,
                        -_moveClamp * _moveMultiplier,
                        _moveClamp * _moveMultiplier
                    );
                }
                else
                {
                    // immediate stop if desired
                    if (_immediateHorizontalStop)
                    {
                        _currentHorizontalSpeed = 0f;
                    }
                    else
                    {
                        // normal decelerate
                        _currentHorizontalSpeed = Mathf.MoveTowards(
                            _currentHorizontalSpeed,
                            0f,
                            _deAcceleration * Time.deltaTime
                        );
                    }
                }
            }
            else
            {
                // no mid-air movement
                _currentHorizontalSpeed = Mathf.MoveTowards(
                    _currentHorizontalSpeed,
                    0f,
                    _deAcceleration * 0.1f * Time.deltaTime
                );
            }
        }
        #endregion

        #region Vertical Movement (Jump)
        private void HandleJump()
        {
            // 1) Coyote Time
            if (Grounded)
            {
                _coyoteCounter = _coyoteTime;

                // (Optional) Zero out downward speed once on ground
                if (_currentVerticalSpeed < 0f)
                    _currentVerticalSpeed = 0f;
            }
            else
            {
                // Decrease coyote if off ground
                if (_coyoteCounter > 0f)
                    _coyoteCounter -= Time.deltaTime;
            }

            // 2) Jump Buffer
            if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
            {
                // Perform jump
                float jumpVelocity = Mathf.Sqrt(
                    2f * _jumpHeight * Mathf.Abs(Physics2D.gravity.y * _gravityScale)
                );
                _currentVerticalSpeed = jumpVelocity;
                _hasJumped = true;

                // Reset counters
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
            }

            // 3) Calculate Gravity This Frame
            float gravity = Physics2D.gravity.y * _gravityScale;

            // If we actually jumped and are still going up, but the player has released jump => short hop
            if (_hasJumped && _variableJumpHeight && !Input.JumpUpPressing)
            {
                gravity *= _releaseGravityMultiplier;
            }

            // Add to vertical speed
            _currentVerticalSpeed += gravity * Time.deltaTime;

            // 4) Clamp Fall Speed so it won't "constantly rise" in negativity
            //    e.g. -40f is a typical max fall speed for many 2D games
            float maxFallSpeed = -40f;
            if (_currentVerticalSpeed < maxFallSpeed)
            {
                _currentVerticalSpeed = maxFallSpeed;
            }
        }

        #endregion

        #region Collision + Slope
        private void RunCollisionChecks()
        {
            LandingThisFrame = false;

            // Basic ground overlap
            bool basicGroundCheck = Physics2D.OverlapCircle(
                _groundCheck.position,
                _groundedRadius,
                _groundLayer
            );
            // Slope check
            //SlopeCheck();

            // We treat ourselves as "grounded" if either the basic circle says so
            // OR if we're on a slope that we can walk on
            //bool slopeGroundCheck = _isOnSlope && _canWalkOnSlope;

            bool finalGroundedCheck = basicGroundCheck; //|| slopeGroundCheck);

            if (_colDown && !finalGroundedCheck)
            {
                // We left ground
                _timeLeftGrounded = Time.time;
                GroundedChanged?.Invoke(false);
            }
            else if (!_colDown && finalGroundedCheck)
            {
                // We just landed
                LandingThisFrame = true;
                GroundedChanged?.Invoke(true);
                ResetLegs();
                _hasJumped = false; // reset jump state on landing
            }
            _colDown = finalGroundedCheck;
        }

        private void SlopeCheck()
        {
            Vector2 checkPos = _groundCheck.position;
            SlopeCheckHorizontal(checkPos);
            SlopeCheckVertical(checkPos);
        }

        private void SlopeCheckHorizontal(Vector2 checkPos)
        {
            RaycastHit2D slopeHitFront = Physics2D.Raycast(
                checkPos,
                transform.right,
                _slopeCheckDistance,
                _groundLayer
            );
            RaycastHit2D slopeHitBack = Physics2D.Raycast(
                checkPos,
                -transform.right,
                _slopeCheckDistance,
                _groundLayer
            );

            if (slopeHitFront)
            {
                _isOnSlope = true;
                _slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            }
            else if (slopeHitBack)
            {
                _isOnSlope = true;
                _slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
            }
            else
            {
                _slopeSideAngle = 0f;
                _isOnSlope = false;
            }
        }

        private void SlopeCheckVertical(Vector2 checkPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                checkPos,
                Vector2.down,
                _slopeCheckDistance,
                _groundLayer
            );
            if (hit)
            {
                slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
                _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (_slopeDownAngle != _lastSlopeAngle)
                {
                    _isOnSlope = true;
                }
                _lastSlopeAngle = _slopeDownAngle;

                // If slope is too steep
                if (_slopeDownAngle > _maxSlopeAngle || _slopeSideAngle > _maxSlopeAngle)
                {
                    _canWalkOnSlope = false;
                }
                else
                {
                    _canWalkOnSlope = true;
                }
            }
        }
        #endregion

        #region Apply Movement
        private void MoveCharacter()
        {
            // If on slope and can walk
            if (_isOnSlope && _canWalkOnSlope && !Input.JumpUpPressing)
            {
                // move along slope normal
                RawMovement = new Vector2(
                    -_currentHorizontalSpeed * slopeNormalPerp.x,
                    -_currentHorizontalSpeed * slopeNormalPerp.y
                );
            }
            else
            {
                // normal movement
                RawMovement = new Vector2(_currentHorizontalSpeed, _currentVerticalSpeed);
            }

            _rigidbody.linearVelocity = RawMovement;

            if (Grounded)
            {
                LegsAnimation();
            }
        }
        #endregion

        #region Animation
        private void LegsAnimation()
        {
            Vector3 G = Root.position;
            Vector3 L = RightLegTarget.position;
            Vector3 T = LeftLegTarget.position;

            float baseX = _rightLegMove ? T.x : L.x;
            float dx = Mathf.Clamp(G.x - baseX, -_halfStepLength, _halfStepLength);

            float curveTime = Mathf.Clamp(dx / _halfStepLength, -1, 1);

            Hands((curveTime + 1) / 2);

            float x = G.x + dx;
            float y = G.y - _maxLegsHeight + _stepCurve.Evaluate(curveTime) * _maxStepHeight;
            Vector2 newPos = new(x, y);

            if (_rightLegMove)
                RightLegTarget.position = newPos;
            else
                LeftLegTarget.position = newPos;

            if (Mathf.Abs(dx) == _halfStepLength)
            {
                _rightLegMove = !_rightLegMove;
                Stepped?.Invoke();
            }
        }

        private void ResetLegs()
        {
            Vector3 G = Root.position;
            Vector3 L = RightLegTarget.position;
            Vector3 T = LeftLegTarget.position;

            float baseX = _rightLegMove ? T.x : L.x;
            float dx = Mathf.Clamp(G.x - baseX, -_halfStepLength, _halfStepLength);

            float curveTime = Mathf.Clamp(dx / _halfStepLength, -1, 1);

            Hands((curveTime + 1) / 2);

            float x = G.x + dx;
            float y = G.y - _maxLegsHeight + _stepCurve.Evaluate(curveTime) * _maxStepHeight;
            Vector2 newPos = new(x, y);

            RightLegTarget.position = newPos;
            LeftLegTarget.position = newPos;
        }

        private void Hands(float percent)
        {
            int maxArmPoint = _numberOfArmPoints - 1;
            int rightHandNum;

            // arms go in opposite directions to each other
            if (_rightLegMove)
            {
                rightHandNum = (int)Mathf.Round(maxArmPoint * percent);
            }
            else
            {
                rightHandNum = maxArmPoint - (int)Mathf.Round(maxArmPoint * percent);
            }

            rightHandNum = Mathf.Clamp(rightHandNum, 0, maxArmPoint);

            RightArmTarget.localPosition = _armBasePoints[rightHandNum];
            LeftArmTarget.localPosition = _armBasePoints[maxArmPoint - rightHandNum];
        }
        #endregion

        private void OnDrawGizmos()
        {
            // Ground check sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundedRadius);
        }
    }
}
