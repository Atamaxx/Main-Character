using System;
using UnityEngine;

namespace Britney
{
    public class BritneyMovement : MonoBehaviour, IBritneyController, ITeleportable
    {
        public MovementSettings Stats;

        [SerializeField]
        private BritneyInputs _movementInput;
        private float _moveMultiplier;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private bool _cachedQueryStartInColliders;
        private Vector2 _frameVelocity;
        private float _time;

        // Control state management
        private bool _inputEnabled = true;

        #region Interface
        public FrameInput F_Input { get; set; }
        public bool IsFrozen { get; set; }
        public bool IsGrounded => _grounded; // Added property to access grounded state
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action Frozen;
        public event Action Unfrozen;
        #endregion

        #region Unity
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

            HandleNoInput();

            ApplyMovement();
        }
        #endregion

        #region Game State Handling

        /// <summary>
        /// Enables player input controls and unfreezes the rigidbody
        /// </summary>
        public void EnableControls()
        {
            print("Enabling Controls");
            _inputEnabled = true;
            if (IsFrozen)
            {
                UnfreezeRigidBody();
            }

            // Enable input component if applicable
            if (_movementInput != null)
            {
                _movementInput.enabled = true;
            }
        }

        /// <summary>
        /// Disables player input controls and freezes the rigidbody
        /// </summary>
        public void DisableControls()
        {
            print("Disabling Controls");
            _inputEnabled = false;
            FreezeRigidBody();

            // Clear input to prevent lingering movement effects
            F_Input = new FrameInput();

            // Disable input component if applicable
            if (_movementInput != null)
            {
                _movementInput.enabled = false;
            }
        }

        #endregion

        #region Gather Input
        private void GatherInput()
        {
            // Only gather input if input is enabled
            if (!_inputEnabled)
            {
                F_Input = new FrameInput();
                return;
            }

            F_Input = new FrameInput
            {
                JumpDown = _movementInput.JumpUpPressedThisFrame,
                JumpHeld = _movementInput.JumpUpPressing,
                Move = new Vector2(_movementInput.MoveInput.x, _movementInput.MoveInput.y),
                SpeedUp = _movementInput.SpeedUpPressing,
                SpeedDown = _movementInput.SpeedDownPressing,
            };

            _moveMultiplier = Stats.MoveMultiplier;
            // if (_stats.SnapInput)
            // {
            //     F_Input.Move.x = Mathf.Abs(F_Input.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(F_Input.Move.x);
            //     F_Input.Move.y = Mathf.Abs(F_Input.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(F_Input.Move.y);
            // }

            if (F_Input.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void HandleNoInput()
        {
            // If input is not enabled, we shouldn't toggle freeze state based on input
            if (!_inputEnabled)
                return;

            if (F_Input.Move.x == 0 && F_Input.Move.y == 0 && !F_Input.JumpHeld)
            {
                if (!IsFrozen)
                {
                    FreezeRigidBody();
                    Frozen?.Invoke();
                }

                return;
            }

            if (IsFrozen)
            {
                Unfrozen?.Invoke();
                UnfreezeRigidBody();
            }
        }
        #endregion

        #region Collisions
        [SerializeField]
        private Transform _groundCheck;

        [SerializeField]
        private Transform _ceilingCheck;

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.OverlapCircle(
                _groundCheck.position,
                Stats.GrounderDistance,
                Stats.PlayerLayer
            );
            bool ceilingHit = Physics2D.OverlapCircle(
                _ceilingCheck.position,
                Stats.GrounderDistance,
                Stats.PlayerLayer
            );

            // Hit a Ceiling
            if (ceilingHit)
                _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }
        #endregion

        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump =>
            _bufferedJumpUsable && _time < _timeJumpWasPressed + Stats.JumpBuffer;
        private bool CanUseCoyote =>
            _coyoteUsable && !_grounded && _time < _frameLeftGrounded + Stats.CoyoteTime;

        private void HandleJump()
        {
            if (_time < 0.1f)
                return;

            if (!_endedJumpEarly && !_grounded && !F_Input.JumpHeld && _rb.linearVelocity.y > 0)
                _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump)
                return;

            if (_grounded || CanUseCoyote)
                ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = Stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (F_Input.Move.x == 0)
            {
                var deceleration = _grounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(
                    _frameVelocity.x,
                    0,
                    deceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(
                    _frameVelocity.x,
                    F_Input.Move.x * Stats.MaxSpeed,
                    Stats.Acceleration * Time.fixedDeltaTime
                );
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = Stats.GroundingForce;
            }
            else
            {
                var inAirGravity = Stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0)
                    inAirGravity *= Stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(
                    _frameVelocity.y,
                    -Stats.MaxFallSpeed,
                    inAirGravity * Time.fixedDeltaTime
                );
            }
        }

        /// <summary>
        /// Freezes the rigidbody movement and sets IsFrozen flag
        /// </summary>
        public void FreezeRigidBody()
        {
            IsFrozen = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.linearDamping = 100f;
            _rb.angularDamping = 100f;
        }

        /// <summary>
        /// Unfreezes the rigidbody movement and clears IsFrozen flag
        /// </summary>
        public void UnfreezeRigidBody()
        {
            IsFrozen = false;
            _rb.linearDamping = 0;
            _rb.angularDamping = 0;
        }

        #endregion

        private void ApplyMovement()
        {
            if (IsFrozen)
                return;

            _rb.linearVelocity = _frameVelocity * _moveMultiplier;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Stats == null)
                Debug.LogWarning(
                    "Please assign a ScriptableStats asset to the Player Controller's Stats slot",
                    this
                );
        }
#endif

        private void OnDrawGizmos()
        {
            if (_groundCheck != null)
            {
                // Check for ground hit
                bool groundHit = Physics2D.OverlapCircle(
                    _groundCheck.position,
                    Stats.GrounderDistance,
                    Stats.PlayerLayer
                );

                // Draw ground check circle
                Gizmos.color = groundHit ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_groundCheck.position, Stats.GrounderDistance);
            }

            if (_ceilingCheck != null)
            {
                // Check for ceiling hit
                bool ceilingHit = Physics2D.OverlapCircle(
                    _ceilingCheck.position,
                    Stats.GrounderDistance,
                    Stats.PlayerLayer
                );

                // Draw ceiling check circle
                Gizmos.color = ceilingHit ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_ceilingCheck.position, Stats.GrounderDistance);
            }
        }

        /// <summary>
        /// Teleports the player to the specified destination
        /// </summary>
        public void TeleportTo(Vector3 destination)
        {
            transform.position = destination;
        }
    }
}
