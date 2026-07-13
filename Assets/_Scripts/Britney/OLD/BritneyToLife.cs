using UnityEngine;
using System.Collections;
using NaughtyAttributes;

namespace MainController
{
    public class BritneyToLife : MonoBehaviour
    {
        [SerializeField, BoxGroup("SFX")] private float _volumeMultiplier = 1f;
        [SerializeField, BoxGroup("SFX")] private float _volumeWhenStatic = 0f;
        [SerializeField, BoxGroup("SFX")] private float _walkingVolumeBase = 0.15f;
        [SerializeField, BoxGroup("SFX")] private float _sprintMultiplier = 1.5f;
        [SerializeField, BoxGroup("SFX")] private float _ctrlMultiplier = 0.5f;
        [SerializeField, BoxGroup("SFX")] private Vector2 _walkingVolumePlusRange = new(0.2f, 0.5f);
   
        [SerializeField, BoxGroup("ANIMATIONS")] private float _tiltAmount = 4f;
        [SerializeField, BoxGroup("ANIMATIONS")] private Transform _tiltObject;
        private IPlayerController _IPlayer;


        private float _walkingVolume = 0.15f;
        private float _jumpReleaseVolumeMax = 1f;
        public float _currentVolume = 0f;
        private Coroutine _volumeChangeCoroutine;
        private bool _grounded;

       // [SerializeField, BoxGroup("ANIMATIONS")]

        // Store the initial rotation so you can preserve it.
        private Vector3 _initialTiltEuler;

        [SerializeField] private Vector2 _speedGap = new(0.01f, 0.2f);
        [SerializeField] private Vector2 _volumeSpeedGap = new(0, 1);
        //[SerializeField] private float _volumeSpeedAdd = 0.2f;

        private void Awake()
        {
            _IPlayer = GetComponent<IPlayerController>();
        }

        private void Start()
        {
            _walkingVolume = _walkingVolumeBase;
            OnStatic();

            // Store the initial rotation of the tilt object (in Euler angles).
            if (_tiltObject != null)
            {
                _initialTiltEuler = _tiltObject.localEulerAngles;
            }
        }

        private void Update()
        {
            if (_IPlayer == null) return;

            // Volume control logic
            if (!_IPlayer.Grounded)
            {
                float speed = 0;// ProceduralMovement.PlayerInstance.DistancePerFrame;
                float t = Mathf.Clamp((speed - _speedGap.x) / (_speedGap.y - _speedGap.x), 0.0f, 1.0f);
                _currentVolume = Mathf.Lerp(_volumeSpeedGap.x, _volumeSpeedGap.y, t);
            }
            else if (_IPlayer.Input.X != 0 && _IPlayer.Grounded)
            {
                OnWalking();
            }
            else if (_IPlayer.Grounded && _volumeChangeCoroutine != null)
            {
                OnStatic();
            }
            //AudioSystem.Instance.UpdateFootstepsVolume(_currentVolume * _volumeMultiplier);

            UpdateTilt();
        }


        private void OnEnable()
        {
            _IPlayer.Jumped += OnJumped;
            _IPlayer.Stepped += OnStepped;
        }

        private void OnDisable()
        {
            _IPlayer.Jumped -= OnJumped;
            _IPlayer.Stepped -= OnStepped;
        }



        private void OnStatic()
        {
            _currentVolume = _volumeWhenStatic;
            if (_volumeChangeCoroutine != null)
                StopCoroutine(_volumeChangeCoroutine);
            _volumeChangeCoroutine = StartCoroutine(ChangeVolumeGradually(_volumeWhenStatic, 0.1f));
        }

        private void OnWalking()
        {
            if (_IPlayer.Input.Sprint)
            {
                _currentVolume = _sprintMultiplier * _walkingVolume;
            }
            else if (_IPlayer.Input.Deceleration)
            {
                _currentVolume = _ctrlMultiplier * _walkingVolume;
            }
            else
            {
                _currentVolume = _walkingVolume;
            }
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

        private void OnJumped()
        {
            // Handle jump event if needed
        }

        private void OnStepped()
        {
            if (_volumeChangeCoroutine != null)
                StopCoroutine(_volumeChangeCoroutine);

            float steppedVolume = _walkingVolume + Random.Range(_walkingVolumePlusRange.x, _walkingVolumePlusRange.y);
            _volumeChangeCoroutine = StartCoroutine(
                TempIncreaseVolume(steppedVolume, _walkingVolumeBase, 0.1f)
            );
        }

        private void UpdateTilt()
        {
            if (_tiltObject == null) return;

            float tiltOffset = 0f;
            if (_IPlayer.Input.X != 0f)
            {
                float tiltMultiplier = 1f;
                if (_IPlayer.Input.Sprint) tiltMultiplier = 1.5f;
                else if (_IPlayer.Input.Deceleration) tiltMultiplier = 0.6f;

                tiltOffset = -_tiltAmount * _IPlayer.Input.X * tiltMultiplier;
            }

            float baseZ = _initialTiltEuler.z;
            if (baseZ > 180f) baseZ -= 360f;

            float targetZ = baseZ + tiltOffset;

            _tiltObject.localRotation = Quaternion.Euler(
                _initialTiltEuler.x,
                _initialTiltEuler.y,
                targetZ
            );
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
    }
}
