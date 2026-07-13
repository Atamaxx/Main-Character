using UnityEngine;

public class ImitateRotation : MonoBehaviour
{
    [SerializeField]
    private bool _pause = false;

    [SerializeField]
    private Transform _imitationTarget;

    [SerializeField]
    private float _speed = 10f;

    [SerializeField]
    private bool _useImitationRange = false;

    [SerializeField]
    private float _imitationRange = 30f; // in degrees

    [SerializeField]
    private Vector3 _offset; // Offset added to the target's rotation (in Euler angles)

    private Quaternion _initialRotation;
    private bool _isWithinRange = false;

    private void Start()
    {
        // Store the initial rotation
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (_pause)
            return;
        if (_imitationTarget == null)
            return;

        // Calculate the target rotation with offset
        Quaternion targetRotation = _imitationTarget.rotation * Quaternion.Euler(_offset);
        // Determine the angle difference between the current rotation and the target's rotation
        float angleDifference = Quaternion.Angle(transform.rotation, _imitationTarget.rotation);

        if (_useImitationRange)
        {
            if (angleDifference <= _imitationRange)
            {
                _isWithinRange = true;
                // Rotate towards the target's rotation (with offset)
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * _speed
                );
            }
            else
            {
                _isWithinRange = false;
                // Rotate back to the initial rotation if out of range
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    _initialRotation,
                    Time.deltaTime * _speed
                );
            }
        }
        else
        {
            // Always rotate towards the target's rotation (with offset)
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * _speed
            );
        }
    }

    public void OnStopped()
    {
        _pause = true;
    }

    public void OnResumed()
    {
        _pause = false;
    }

    public void SetImitationTarget(Transform target)
    {
        _imitationTarget = target;
    }

    public void SetImitationSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetImitationOffsetX(float offsetX)
    {
        _offset.x = offsetX;
    }

    public void SetImitationOffsetY(float offsetY)
    {
        _offset.y = offsetY;
    }

    public void SetImitationOffsetZ(float offsetZ)
    {
        _offset.z = offsetZ;
    }

    public void SetImitationRange(float range)
    {
        _imitationRange = range;
    }

    private void OnDrawGizmos()
    {
        if (_imitationTarget == null)
            return;

        // Draw a line representing the object's current forward direction
        Gizmos.color = Color.cyan;
        Vector3 currentForward = transform.position + transform.forward * 2f;
        Gizmos.DrawLine(transform.position, currentForward);

        // Calculate the desired target rotation with offset and draw its forward direction
        Quaternion targetRotation = _imitationTarget.rotation * Quaternion.Euler(_offset);
        Vector3 targetForward = transform.position + (targetRotation * Vector3.forward) * 2f;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, targetForward);

        // Visualize the imitation range if enabled by drawing a small wire sphere (scaled arbitrarily)
        if (_useImitationRange)
        {
            float angleDifference = Quaternion.Angle(transform.rotation, _imitationTarget.rotation);
            Gizmos.color = (angleDifference <= _imitationRange) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _imitationRange * 0.1f);
        }
    }
}
