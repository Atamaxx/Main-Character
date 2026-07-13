using UnityEngine;

public class ImitateX : MonoBehaviour
{
    [SerializeField] private bool _pause = false;
    [SerializeField] private Transform _imitationTarget;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private bool _useImitationRange = false;
    [SerializeField] private float _imitationRange = 10f;

    private Vector3 _initialPosition;
    private Vector3 _imitationPosition;
    private Vector3 _objectPosition;
    private bool _isWithinRange = false;

    private void Start()
    {
        // Store the initial position
        _initialPosition = transform.position;
    }

    private void Update()
    {
        if (_pause) return;

        _imitationPosition = _imitationTarget.position;
        _objectPosition = transform.position;

        float distance = Vector3.Distance(_objectPosition, _imitationPosition);

        if (_useImitationRange)
        {
            if (distance <= _imitationRange)
            {
                _isWithinRange = true;
                // Move towards the target's X position
                Vector3 newPosition = new Vector3(_imitationPosition.x, _objectPosition.y, _objectPosition.z);
                transform.position = Vector3.Lerp(_objectPosition, newPosition, Time.deltaTime * _speed);
            }
            else
            {
                _isWithinRange = false;
                // Move back to the initial X position
                Vector3 newInitialPosition = new Vector3(_initialPosition.x, _objectPosition.y, _objectPosition.z);
                transform.position = Vector3.Lerp(_objectPosition, newInitialPosition, Time.deltaTime * _speed);
            }
        }
        else
        {
            Vector3 newPosition = new Vector3(_imitationPosition.x, _objectPosition.y, _objectPosition.z);
            transform.position = Vector3.Lerp(_objectPosition, newPosition, Time.deltaTime * _speed);
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
    public void SetImitationRange(float range)
    {
        _imitationRange = range;
    }
    private void OnDrawGizmos()
    {
        if (_imitationTarget == null)
            return;

        // Update positions for drawing
        _imitationPosition = _imitationTarget.position;
        _objectPosition = transform.position;

        // Draw a line from this object to the target's X position (keeping Y and Z unchanged)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_objectPosition, new Vector3(_imitationPosition.x, _objectPosition.y, _objectPosition.z));

        // Visualize the imitation range if enabled
        if (_useImitationRange)
        {
            // Change color based on the distance
            if (Vector3.Distance(_objectPosition, _imitationPosition) <= _imitationRange)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(_objectPosition, _imitationRange);
        }
    }
}
