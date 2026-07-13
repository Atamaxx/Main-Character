using UnityEngine;

public class ImitateXY : MonoBehaviour
{
    [Header("Imitation Settings")]
    [SerializeField] private bool _pause = false;
    [SerializeField] private Transform _imitationTarget;
    [SerializeField] private float _speed = 10f;
    
    [Header("Axis Control")]
    [SerializeField] private bool _imitateX = true;
    [SerializeField] private bool _imitateY = true;
    
    [Header("Range Settings")]
    [SerializeField] private bool _useImitationRange = false;
    [SerializeField] private float _imitationRange = 10f;
    
    [Header("Position Offset")]
    [SerializeField] private Vector2 _offset; // Offset added to the target's position
    
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
        if (_pause || _imitationTarget == null) return;
        
        _imitationPosition = _imitationTarget.position;
        _objectPosition = transform.position;
        float distance = Vector3.Distance(_objectPosition, _imitationPosition);
        
        if (_useImitationRange)
        {
            if (distance <= _imitationRange)
            {
                _isWithinRange = true;
                // Move towards the target's position, respecting axis settings
                Vector3 targetPos = CalculateTargetPosition(_imitationPosition, _objectPosition);
                transform.position = Vector3.Lerp(_objectPosition, targetPos, Time.deltaTime * _speed);
            }
            else
            {
                _isWithinRange = false;
                // Move back to the initial position (ignoring the offset)
                Vector3 targetPos = new Vector3(
                    _imitateX ? _initialPosition.x : _objectPosition.x,
                    _imitateY ? _initialPosition.y : _objectPosition.y,
                    _objectPosition.z);
                transform.position = Vector3.Lerp(_objectPosition, targetPos, Time.deltaTime * _speed);
            }
        }
        else
        {
            // Move towards the target's position, respecting axis settings
            Vector3 targetPos = CalculateTargetPosition(_imitationPosition, _objectPosition);
            transform.position = Vector3.Lerp(_objectPosition, targetPos, Time.deltaTime * _speed);
        }
    }
    
    private Vector3 CalculateTargetPosition(Vector3 targetPosition, Vector3 currentPosition)
    {
        // Calculate the target position based on which axes to imitate
        return new Vector3(
            _imitateX ? targetPosition.x + _offset.x : currentPosition.x,
            _imitateY ? targetPosition.y + _offset.y : currentPosition.y,
            currentPosition.z);
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
    
    public void SetImitationRange(float range)
    {
        _imitationRange = range;
    }
    
    public void SetImitateX(bool imitateX)
    {
        _imitateX = imitateX;
    }
    
    public void SetImitateY(bool imitateY)
    {
        _imitateY = imitateY;
    }
    
    private void OnDrawGizmos()
    {
        if (_imitationTarget == null)
            return;
            
        // Update positions for drawing
        _imitationPosition = _imitationTarget.position;
        _objectPosition = transform.position;
        
        // Draw a line from this object to the target's position plus offset
        Vector3 targetPos = CalculateTargetPosition(_imitationPosition, _objectPosition);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_objectPosition, targetPos);
        
        // Visualize the imitation range if enabled
        if (_useImitationRange)
        {
            Gizmos.color = (Vector3.Distance(_objectPosition, _imitationPosition) <= _imitationRange) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_objectPosition, _imitationRange);
        }
    }
}