using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using My;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.VFX;

namespace Britney
{
    public class BritneyToLife : MonoBehaviour
    {
        [BoxGroup("PARTICLES"), SerializeField]
        private ParticleSystem _jumpParticles;

        [BoxGroup("PARTICLES"), SerializeField]
        private ParticleSystem _launchParticles;

        [BoxGroup("PARTICLES"), SerializeField]
        private ParticleSystem _moveParticles;

        [BoxGroup("PARTICLES"), SerializeField]
        private ParticleSystem _landParticles;

        private bool _rightLegMove = true;
        private List<Vector3> _armBasePoints = new();
        private IBritneyController _iBritney;
        private bool _grounded;
        private float _speedMultiplier;
        private Rigidbody2D _rb;
        private float _velocity;

        #region UNITY
        private void Awake()
        {
            _iBritney = GetComponentInParent<IBritneyController>();
        }

        private void Start()
        {
            _armBasePoints = Geometry.CalculateBezierLocalPoints(
                _leftArmTarget.position,
                _root.position,
                _rightArmTarget.position,
                AnimationSO.NumberOfArmPoints,
                _root
            );

            _walkingVolume = SoundSO.WalkingVolumeBase;
            _initialTiltEuler = _tiltObject.localEulerAngles;
            _rb = GetComponent<Rigidbody2D>();

            VFXSetUp();
        }

        private void OnEnable()
        {
            _iBritney.Jumped += OnJumped;
            _iBritney.GroundedChanged += OnGroundedChanged;

            //            AudioSystem.Instance.PlaySFXLoop(_walkingSFXKey, FMODEvents.Instance.WalkingSFXLoop);
        }

        private void OnDisable()
        {
            _iBritney.Jumped -= OnJumped;
            _iBritney.GroundedChanged -= OnGroundedChanged;

            //            AudioSystem.Instance.StopSFXLoop(_walkingSFXKey);

            //!_moveParticles.Stop();
        }

        private void Update()
        {
            CalculateVelocity();

            _speedMultiplier = AnimationSO.SpeedBaseMultiplier;

            if (_grounded)
            {
                if (_isTransitioningLanding)
                {
                    // Compute running (grounded) leg positions and hand blend value.
                    ComputeRunningLegData(
                        out Vector3 runningLeftPos,
                        out Vector3 runningRightPos,
                        out float handPercent
                    );

                    // Blend from the stored air positions (captured at landing) to the running positions.
                    _leftLegTarget.position = Vector3.Lerp(
                        _airLeftLegAtLanding,
                        runningLeftPos,
                        _landingBlend
                    );
                    _rightLegTarget.position = Vector3.Lerp(
                        _airRightLegAtLanding,
                        runningRightPos,
                        _landingBlend
                    );

                    // Update arms so they blend seamlessly too.
                    Hands(handPercent);

                    // Increment blend factor over the transition duration.
                    _landingBlend += Time.deltaTime / AnimationSO.LandingTransitionDuration;
                    if (_landingBlend >= 1f)
                    {
                        _landingBlend = 1f;
                        _isTransitioningLanding = false;
                    }
                }

                if (_velocity != 0)
                {
                    OnWalkingSFX();

                    if (!_isTransitioningLanding)
                    {
                        // Use your regular running (walking) animation.
                        LegsAnimation();
                    }
                }
                else
                {
                    //if (_volumeChangeCoroutine != null)
                    OnStaticSFX();
                }
            }
            else // if player not grounded
            {
                OnFlyingSFX();
                AirLegsAnimation();
            }

            UpdateTilt();
            AudioSystem.Instance.SetSFXParameter(
                _walkingSFXKey,
                FMODEvents.Instance.WalkingIntensityParam,
                _currentVolume * SoundSO.VolumeMultiplier
            );
        }
        #endregion

        #region ANIMATION
        [BoxGroup("ANIMATION")]
        public AnimationSettings AnimationSO;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _leftLegTarget;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _rightLegTarget;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _leftArmTarget;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _rightArmTarget;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _root;

        [BoxGroup("ANIMATION"), SerializeField]
        private Transform _tiltObject;

        [BoxGroup("ANIMATION"), SerializeField]
        private float tiltSmoothSpeed = 5f; // Smoothing factor

        [BoxGroup("ANIMATION")]
        public bool MoveLeftHand = true;

        [BoxGroup("ANIMATION")]
        public bool MoveRightHand = true;

        private void LegsAnimation()
        {
            Vector3 G = _root.position;
            Vector3 L = _rightLegTarget.position;
            Vector3 T = _leftLegTarget.position;
            float halfStepLength = AnimationSO.HalfStepLength * _speedMultiplier;

            float baseX = _rightLegMove ? T.x : L.x;
            float dx = Mathf.Clamp(G.x - baseX, -halfStepLength, halfStepLength);

            float curveTime = Mathf.Clamp(dx / halfStepLength, -1, 1);

            Hands((curveTime + 1) / 2);

            float x = G.x + dx;
            float y =
                G.y
                - AnimationSO.MaxLegsHeight
                + AnimationSO.StepCurve.Evaluate(curveTime) * AnimationSO.MaxStepHeight;
            Vector2 newPos = new(x, y);

            if (_rightLegMove)
                _rightLegTarget.position = newPos;
            else
                _leftLegTarget.position = newPos;

            if (Mathf.Abs(dx) == halfStepLength)
            {
                _rightLegMove = !_rightLegMove;
                OnStepped();
            }
        }

        private void Hands(float percent)
        {
            int maxArmPoint = AnimationSO.NumberOfArmPoints - 1;
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
            if (MoveLeftHand)
                _leftArmTarget.localPosition = _armBasePoints[maxArmPoint - rightHandNum];

            if (MoveRightHand)
                _rightArmTarget.localPosition = _armBasePoints[rightHandNum];
        }

        private void AirLegsAnimation()
        {
            // Use the root's position as a reference.
            Vector3 rootPos = _root.position;

            // Define desired positions relative to the root.
            // Here we assume you want the legs to be positioned below the root,
            // with a lateral separation.
            float offsetY = AnimationSO.AirLegYOffset;
            float lateralOffset = AnimationSO.AirLegLateralOffset;

            // Calculate desired positions:
            Vector2 desiredLeftLegPos = new Vector2(rootPos.x - lateralOffset, rootPos.y - offsetY);
            Vector2 desiredRightLegPos = new Vector2(
                rootPos.x + lateralOffset,
                rootPos.y - offsetY
            );

            // Lerp from current leg positions to the desired air positions.
            _leftLegTarget.position = Vector3.Lerp(
                _leftLegTarget.position,
                desiredLeftLegPos,
                Time.deltaTime * AnimationSO.AirLegLerpSpeed
            );
            _rightLegTarget.position = Vector3.Lerp(
                _rightLegTarget.position,
                desiredRightLegPos,
                Time.deltaTime * AnimationSO.AirLegLerpSpeed
            );
        }

        /// <summary>
        /// Computes the running target positions for both legs and the hand percent based on the current walking logic.
        /// </summary>
        /// <param name="leftLegTarget">Computed target position for the left leg.</param>
        /// <param name="rightLegTarget">Computed target position for the right leg.</param>
        /// <param name="handPercent">A normalized value for calling Hands (derived from curveTime).</param>
        private void ComputeRunningLegData(
            out Vector3 leftLegTarget,
            out Vector3 rightLegTarget,
            out float handPercent
        )
        {
            Vector3 G = _root.position;
            float halfStepLength = AnimationSO.HalfStepLength * _speedMultiplier;

            // We'll use a lateral offset when a leg is stationary.
            float lateralOffset = AnimationSO.GroundedLateralOffset;

            float maxLegsHeight = AnimationSO.MaxLegsHeight;
            // Depending on which leg is moving, compute the moving leg’s target using your walking logic.
            if (_rightLegMove)
            {
                // Use the left leg target's x as the base.
                float baseX = _leftLegTarget.position.x;
                float dx = Mathf.Clamp(G.x - baseX, -halfStepLength, halfStepLength);
                float curveTime = Mathf.Clamp(dx / halfStepLength, -1f, 1f);
                handPercent = (curveTime + 1f) / 2f; // same as in your original Hands call

                // Compute moving (right) leg target.
                float xRight = G.x + dx;
                float yRight =
                    G.y
                    - maxLegsHeight
                    + AnimationSO.StepCurve.Evaluate(curveTime) * AnimationSO.MaxStepHeight;
                rightLegTarget = new Vector3(xRight, yRight, _rightLegTarget.position.z);

                // The left leg remains at a fixed offset.
                leftLegTarget = new Vector3(
                    G.x - lateralOffset,
                    G.y - maxLegsHeight,
                    _leftLegTarget.position.z
                );
            }
            else
            {
                // When the left leg is moving, use the right leg target's x as the base.
                float baseX = _rightLegTarget.position.x;
                float dx = Mathf.Clamp(G.x - baseX, -halfStepLength, halfStepLength);
                float curveTime = Mathf.Clamp(dx / halfStepLength, -1f, 1f);
                handPercent = (curveTime + 1f) / 2f;

                // Compute moving (left) leg target.
                float xLeft = G.x + dx;
                float yLeft =
                    G.y
                    - maxLegsHeight
                    + AnimationSO.StepCurve.Evaluate(curveTime) * AnimationSO.MaxStepHeight;
                leftLegTarget = new Vector3(xLeft, yLeft, _leftLegTarget.position.z);

                // The right leg stays at a fixed offset.
                rightLegTarget = new Vector3(
                    G.x + lateralOffset,
                    G.y - maxLegsHeight,
                    _rightLegTarget.position.z
                );
            }
            Hands(handPercent);
        }

        private Vector3 _initialTiltEuler;
        private Quaternion _targetRotation;

        private void UpdateTilt()
        {
            if (_tiltObject == null)
                return;

            // Calculate tilt offset based on input
            float tiltOffset =
                -AnimationSO.TiltAmount * _iBritney.F_Input.Move.x * _speedMultiplier;

            // Normalize the base Z angle to the range (-180, 180]
            float baseZ = _initialTiltEuler.z;
            if (baseZ > 180f)
                baseZ -= 360f;

            // Calculate target Z rotation
            float targetZ = baseZ + tiltOffset;

            // Define the target rotation with the new Z angle
            Quaternion desiredRotation = Quaternion.Euler(
                _initialTiltEuler.x,
                _initialTiltEuler.y,
                targetZ
            );

            // Smoothly interpolate towards the desired rotation
            _targetRotation = Quaternion.Lerp(
                _tiltObject.localRotation,
                desiredRotation,
                tiltSmoothSpeed * Time.deltaTime
            );

            // Apply the interpolated rotation
            _tiltObject.localRotation = _targetRotation;
        }
        #endregion

        #region SOUND
        [BoxGroup("SOUNDS")]
        public SoundSettings SoundSO;
        private float _currentVolume = 0f;
        private float _walkingVolume = 0.15f;
        private string _walkingSFXKey = "Walking";

        private Coroutine _volumeChangeCoroutine;

        private void OnStaticSFX()
        {
            _currentVolume = SoundSO.VolumeWhenStatic;
            if (_volumeChangeCoroutine != null)
                StopCoroutine(_volumeChangeCoroutine);
            _volumeChangeCoroutine = StartCoroutine(
                ChangeVolumeGradually(SoundSO.VolumeWhenStatic, SoundSO.EaseOffDuration)
            );
        }

        private void OnWalkingSFX()
        {
            _currentVolume = _walkingVolume * _speedMultiplier;
        }

        private void OnFlyingSFX()
        {
            Vector2 airSpeedInterval = SoundSO.AirSpeedInterval;
            Vector2 airVolInterval = SoundSO.AirVolumeInterval;

            float t = Mathf.Clamp(
                (_velocity - airSpeedInterval.x) / (airSpeedInterval.y - airSpeedInterval.x),
                0.0f,
                1.0f
            );
            _currentVolume = Mathf.Lerp(airVolInterval.x, airVolInterval.y, t);
        }

        private void OnSteppedSFX()
        {
            // if (_volumeChangeCoroutine != null)
            //     StopCoroutine(_volumeChangeCoroutine);

            // float steppedVolume =
            //     _walkingVolume
            //     + UnityEngine.Random.Range(
            //         SoundSO.WalkingVolumePlusRange.x,
            //         SoundSO.WalkingVolumePlusRange.y
            //     );
            // _volumeChangeCoroutine = StartCoroutine(
            //     TempIncreaseVolume(
            //         steppedVolume,
            //         SoundSO.WalkingVolumeBase,
            //         SoundSO.StepSoundDuration
            //     )
            // );
        }

        private IEnumerator ChangeVolumeGradually(float targetVolume, float duration)
        {
            float startVolume = _currentVolume;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                _currentVolume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _currentVolume = targetVolume;
        }

        private IEnumerator TempIncreaseVolume(float targetVolume, float baseVolume, float duration)
        {
            float startTime = Time.time;
            float elapsed = 0f;

            // Set the current volume to the stepped volume immediately
            _walkingVolume = targetVolume;

            // Gradually decrease the volume back to the base walking volume
            while (elapsed < duration)
            {
                elapsed = Time.time - startTime;
                _walkingVolume = Mathf.Lerp(targetVolume, baseVolume, elapsed / duration);
                yield return null;
            }

            // Ensure volume is set back to normal walking volume after the effect
            _walkingVolume = baseVolume;
        }

        #endregion

        #region VFX
        [BoxGroup("VFX"), SerializeField]
        private List<VisualEffect> _vfx = new();

        [BoxGroup("VFX")]
        public VFXSettings VfxSO;
        private int _bodyPartsNum;
        private int _scaleParameterIdHEAD;
        private int _colorParameterID;
        private int _spawnRateAuraParameterId;
        private Color[] _currentBodyColors;
        private Color[] _targetBodyColors;
        private Color[] _initialBodyColors;
        private Coroutine _colorTransitionCoroutine;

        private void VFXSetUp()
        {
            _bodyPartsNum = _vfx.Count;

            _scaleParameterIdHEAD = Shader.PropertyToID(VfxSO.ScaleParam);
            _colorParameterID = Shader.PropertyToID(VfxSO.ColorParam);
            _spawnRateAuraParameterId = Shader.PropertyToID(VfxSO.SpawnRateParam);

            _currentBodyColors = new Color[_bodyPartsNum];
            _targetBodyColors = new Color[_bodyPartsNum];
            _initialBodyColors = new Color[_bodyPartsNum];
        }

        public void RedrawBody(int inkAmountUsed, int maxInkAmount)
        {
            // Avoid division by zero
            if (maxInkAmount <= 0)
            {
                Debug.LogError("maxInkAmount must be greater than zero.");
                return;
            }

            // Calculate how many body parts should appear "unfilled"
            // (Using RoundToInt so the value rounds appropriately)
            int inkUsedToBodyCount = Mathf.RoundToInt(
                Mathf.Lerp(0, _bodyPartsNum, (float)inkAmountUsed / maxInkAmount)
            );

            // Cache the palette reference for cleaner code
            var palette = ColorController.Instance.Palette;

            // Set target colors for each body part.
            // (Assuming parts 1..n follow one rule while part 0—the head—follows a different rule)
            for (int i = 1; i < _bodyPartsNum; i++)
            {
                _targetBodyColors[i] =
                    (i <= inkUsedToBodyCount) ? palette.BasicColorHDR : palette.FillColorHDR;
            }
            // Special handling for the first element (e.g., head)
            _targetBodyColors[0] =
                (inkUsedToBodyCount == _bodyPartsNum)
                    ? palette.BasicColorHDR
                    : palette.FillColorHDR;

            // Check whether any changes are needed by comparing current vs. target colors.
            bool changeNeeded = false;
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                if (_targetBodyColors[i] != _currentBodyColors[i])
                {
                    changeNeeded = true;
                    break;
                }
            }
            if (!changeNeeded)
            {
                return;
            }

            // Capture the current visual state as the starting point for the transition.
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                _initialBodyColors[i] = _currentBodyColors[i];
            }

            // If a previous transition is still running, cancel it.
            if (_colorTransitionCoroutine != null)
            {
                StopCoroutine(_colorTransitionCoroutine);
            }

            _colorTransitionCoroutine = StartCoroutine(ColorTransitionCoroutine());
        }

        private IEnumerator ColorTransitionCoroutine()
        {
            float duration = VfxSO.ColorTransitionDuration; // Transition duration from your settings.
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Interpolate colors from the starting (_initialBodyColors) to the target values.
                for (int i = 0; i < _bodyPartsNum; i++)
                {
                    Color blended = Color.Lerp(_initialBodyColors[i], _targetBodyColors[i], t);
                    _vfx[i].SetVector4(_colorParameterID, blended);

                    // Update _currentBodyColors continuously so that a new transition will start
                    // from the last displayed color.
                    _currentBodyColors[i] = blended;
                }
                yield return null; // Wait for next frame.
            }

            // Finalize: ensure that all colors are exactly set to their target values.
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                _currentBodyColors[i] = _targetBodyColors[i];
                _vfx[i].SetVector4(_colorParameterID, _targetBodyColors[i]);
            }

            _colorTransitionCoroutine = null;
        }

        [BoxGroup("VFX"), SerializeField]
        private int _defaultSpawnRate = 100;

        [BoxGroup("VFX"), SerializeField]
        private int _freezeSpawnRate = 50;

        [BoxGroup("VFX"), SerializeField]
        private float _resetDelay = 0.1f;

        //private Coroutine _freezeVFXCoroutine;

        public void StopVFX()
        {
            foreach (VisualEffect vfx in _vfx)
            {
                vfx.SetInt("SpawnRate", _freezeSpawnRate);
            }
        }

        public void PlayVFX()
        {
            foreach (VisualEffect vfx in _vfx)
            {
                vfx.SetInt("SpawnRate", _defaultSpawnRate);
            }
        }

        #endregion


        #region ACTIONS
        // Flag and blend factor for landing transition
        private bool _isTransitioningLanding = false;
        private float _landingBlend = 0f;

        // Store the air leg positions at the moment of landing
        private Vector3 _airLeftLegAtLanding;
        private Vector3 _airRightLegAtLanding;

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;

            if (grounded)
            {
                // Capture the current air positions for both legs.
                _airLeftLegAtLanding = _leftLegTarget.position;
                _airRightLegAtLanding = _rightLegTarget.position;

                // Reset the landing blend.
                _landingBlend = 0f;
                _isTransitioningLanding = true;

                //!_moveParticles.Play();
                //!_landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                //!_landParticles.Play();
            }
            else
            {
                //!_moveParticles.Stop();
            }
        }

        private void OnJumped()
        {
            if (_grounded) // Avoid coyote
            {
                //!_jumpParticles.Play();
            }
        }

        private void OnStepped()
        {
            OnSteppedSFX();
        }

        private void CalculateVelocity()
        {
            _velocity = _rb.linearVelocity.magnitude;
        }
        #endregion
    }
}
