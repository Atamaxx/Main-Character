using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class TreeFallOnTilt : MonoBehaviour
{
    [Header("Imitation Settings")]
    [SerializeField] private bool _pause = false;
    [SerializeField] private Transform _imitationTarget;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private bool _useImitationRange = false;
    [SerializeField] private float _imitationRange = 30f; // in degrees
    [SerializeField] private Vector3 _offset; // Offset added to the target's rotation (in Euler angles)

    private Quaternion _initialRotation;
    private bool _isWithinRange = false;

    [Header("Tilt Fall Settings")]
    [Tooltip("Cumulative tilt threshold (in degrees) required to trigger the fall")]
    [SerializeField] private float tiltThreshold = 100f;
    private float _tiltAccumulated = 0f;
    private float _lastTilt = 0f;

    [Header("Falling Settings")]
    [SerializeField] private UnityEvent _onFall;
    [SerializeField] private float fallSpeed = 2f;    // Speed at which the tree falls
    [SerializeField] private float fallAngle = 90f;     // Additional angle (relative to initial) to represent the fall
    private Quaternion _fallRotation;

    [Header("FMOD Settings")]
    [SerializeField] private string fmodGlobalParamName = "TreeCreakVolume"; // Replace with your FMOD global param name
    [SerializeField] private float fmodParamInitial = 0f;  // initial FMOD parameter value
    [SerializeField] private float fmodParamTarget = 1f;   // target FMOD parameter value when tiltThreshold is reached

    private void Start()
    {
        // Store the initial rotation.
        _initialRotation = transform.rotation;

        // If the target is set, initialize the last recorded tilt (using the z-axis).
        if (_imitationTarget != null)
        {
            _lastTilt = _imitationTarget.eulerAngles.z;
        }

        // Define the target fall rotation relative to the initial rotation.
        _fallRotation = _initialRotation * Quaternion.Euler(fallAngle, 0f, 0f);
    }

    private void Update()
    {
        if (_pause)
            return;

        // --- Imitation Rotation ---
        // Calculate the target rotation (with the optional offset).
        Quaternion targetRotation = _imitationTarget.rotation * Quaternion.Euler(_offset);
        float angleDifference = Quaternion.Angle(transform.rotation, _imitationTarget.rotation);

        // Calculate imitation effectiveness based on tilt accumulation.
        // When _tiltAccumulated is 0, effectiveness is 0; when it reaches tiltThreshold, effectiveness is 1.
        float imitationEffectiveness = Mathf.Clamp01(_tiltAccumulated / tiltThreshold);

        if (_useImitationRange)
        {
            if (angleDifference <= _imitationRange)
            {
                _isWithinRange = true;
                // Blend between the initial and target rotations based on imitation effectiveness.
                Quaternion desiredRotation = Quaternion.Slerp(_initialRotation, targetRotation, imitationEffectiveness);
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * _speed);
            }
            else
            {
                _isWithinRange = false;
                transform.rotation = Quaternion.Lerp(transform.rotation, _initialRotation, Time.deltaTime * _speed);
            }
        }
        else
        {
            // Always imitate the target's rotation by blending between the initial and target rotations.
            Quaternion desiredRotation = Quaternion.Slerp(_initialRotation, targetRotation, imitationEffectiveness);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * _speed);
        }

        // --- Tilt Accumulation ---
        // Measure the cumulative change in the target's z-axis rotation.
        float currentTilt = _imitationTarget.eulerAngles.z;
        float deltaTilt = Mathf.Abs(Mathf.DeltaAngle(_lastTilt, currentTilt));
        _tiltAccumulated += deltaTilt;
        _lastTilt = currentTilt;

        // --- FMOD Global Parameter Update ---
        // Update FMOD parameter value based on the current tilt accumulation.
        float fmodEffectiveness = Mathf.Clamp01(_tiltAccumulated / tiltThreshold);
        float fmodParamValue = Mathf.Lerp(fmodParamInitial, fmodParamTarget, fmodEffectiveness);
        RuntimeManager.StudioSystem.setParameterByName(fmodGlobalParamName, fmodParamValue);

        // Uncomment for debugging:
        // Debug.Log("Tilt Accumulated: " + _tiltAccumulated);
        // Debug.Log("FMOD Parameter Value: " + fmodParamValue);

        // If the cumulative tilt exceeds the threshold, trigger the fall.
        if (_tiltAccumulated >= tiltThreshold)
        {
            TriggerFall();
        }
    }

    private void TriggerFall()
    {
        _pause = true; // Stop further imitation once the fall is triggered.
        _onFall?.Invoke();
    }

    // --- Public Methods ---
    public void OnStopped() => _pause = true;
    public void OnResumed() => _pause = false;

    public void SetImitationTarget(Transform target)
    {
        _imitationTarget = target;
        if (_imitationTarget != null)
            _lastTilt = _imitationTarget.eulerAngles.z;
    }

    public void SetImitationSpeed(float speed) => _speed = speed;
    public void SetImitationOffsetX(float offsetX) => _offset.x = offsetX;
    public void SetImitationOffsetY(float offsetY) => _offset.y = offsetY;
    public void SetImitationOffsetZ(float offsetZ) => _offset.z = offsetZ;
    public void SetImitationRange(float range) => _imitationRange = range;

    private void OnDrawGizmos()
    {
        if (_imitationTarget == null)
            return;

        // Draw the current forward direction.
        Gizmos.color = Color.cyan;
        Vector3 currentForward = transform.position + transform.forward * 2f;
        Gizmos.DrawLine(transform.position, currentForward);

        // Draw the target's forward direction (with offset).
        Quaternion targetRotation = _imitationTarget.rotation * Quaternion.Euler(_offset);
        Vector3 targetForward = transform.position + (targetRotation * Vector3.forward) * 2f;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, targetForward);

        // If using imitation range, visualize it.
        if (_useImitationRange)
        {
            float angleDifference = Quaternion.Angle(transform.rotation, _imitationTarget.rotation);
            Gizmos.color = (angleDifference <= _imitationRange) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _imitationRange * 0.1f);
        }
    }
}
