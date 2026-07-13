using UnityEngine;
using UnityEngine.Events;

public class CheckIfUnderCollider : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Vertical offset for the ray's origin point.")]
    [SerializeField] private float _yOffset = 0f;

    [SerializeField] private Transform _objectToCheck;
    [SerializeField] private LayerMask _objectLayer;

    [Tooltip("Layers considered as obstacles.")]
    [SerializeField] private LayerMask _obstacleLayers;

    [Tooltip("Enable to visualize the ray in the Scene view.")]
    [SerializeField] private bool _debug = true;
    
    // Current state flag: true if under an obstacle.
    public bool IsUnder = false;

    // UnityEvents for enter and exit state.
    public UnityEvent OnUnderColliderEnter;
    public UnityEvent OnUnderColliderExit;

    // Private state to track changes.
    private bool _prevIsUnder = false;

    private void Update()
    {
        // Determine whether the object is under an obstacle.
        IsUnder = !IsPlayerVisible();

        // Check for state change.
        if (IsUnder != _prevIsUnder)
        {
            if (IsUnder)
            {
                // Fire event when the object goes under an obstacle.
                OnUnderColliderEnter?.Invoke();
            }
            else
            {
                // Fire event when the object leaves the under state.
                OnUnderColliderExit?.Invoke();
            }
            _prevIsUnder = IsUnder;
        }
    }

    public bool IsPlayerVisible()
    {
        if (_objectToCheck == null)
            return false;

        Vector2 origin = new Vector2(_objectToCheck.position.x, _objectToCheck.position.y + _yOffset);
        float direction = _yOffset > 0 ? -1f : 1f;
        float distance = Mathf.Abs(_yOffset);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * direction, distance, _obstacleLayers | _objectLayer);

        if (_debug)
        {
            Debug.DrawRay(origin, direction * distance * Vector2.up, Color.blue);
        }

        if (hit)
        {
            if (_debug)
            {
                Debug.DrawRay(origin, direction * distance * Vector2.up, Color.green);
            }
            return hit.transform == _objectToCheck;
        }

        return false;
    }
}
