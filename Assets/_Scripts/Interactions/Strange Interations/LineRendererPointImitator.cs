using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Makes a specific point in a LineRenderer imitate the position of a target Transform.
/// Can optionally update an EdgeCollider2D to match the LineRenderer points.
/// </summary>
public class LineRendererPointImitator : MonoBehaviour
{
    [Foldout("Components")]
    [Required("LineRenderer is required")]
    [SerializeField] private LineRenderer _lineRenderer;

    [Foldout("Components")]
    [SerializeField] private EdgeCollider2D _edgeCollider2D;

    [Foldout("Point Settings")]
    [MinValue(0)]
    [SerializeField] private int _pointIndex = 0;

    [Foldout("Point Settings")]
    [Required("Target Transform is required")]
    [SerializeField] private Transform _targetTransform;

    [Foldout("Imitation Settings")]
    [SerializeField] private bool _imitateX = true;

    [Foldout("Imitation Settings")]
    [SerializeField] private bool _imitateY = true;

    [Foldout("Imitation Settings")]
    [SerializeField] private Vector2 _positionOffset = Vector2.zero;

    [Foldout("Collider Settings")]
    [SerializeField] private bool _updateEdgeCollider = false;

    [Foldout("Collider Settings")]
    [ShowIf("_updateEdgeCollider")]
    [SerializeField] private bool _updateColliderOnlyOnLateUpdate = true;

    private bool _pointPositionChanged = false;

    private void Awake()
    {
        // If LineRenderer not assigned, try to get it from this GameObject
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                Debug.LogError("LineRenderer component not found on " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // If EdgeCollider2D was not assigned but update is enabled, try to get it
        if (_updateEdgeCollider && _edgeCollider2D == null)
        {
            _edgeCollider2D = GetComponent<EdgeCollider2D>();
        }
    }

    private void Update()
    {
        UpdateLineRendererPoint();
        
        // If we're not delaying the collider update to LateUpdate, update it now
        if (_pointPositionChanged && _updateEdgeCollider && !_updateColliderOnlyOnLateUpdate)
        {
            UpdateEdgeCollider();
            _pointPositionChanged = false;
        }
    }

    private void LateUpdate()
    {
        // If we've deferred the collider update to LateUpdate, do it now
        if (_pointPositionChanged && _updateEdgeCollider && _updateColliderOnlyOnLateUpdate)
        {
            UpdateEdgeCollider();
            _pointPositionChanged = false;
        }
    }

    private void UpdateLineRendererPoint()
    {
        if (_targetTransform == null || _lineRenderer == null)
            return;

        // Ensure the pointIndex is within valid range
        if (_pointIndex < 0 || _pointIndex >= _lineRenderer.positionCount)
        {
            Debug.LogWarning("Point index out of range in LineRenderer");
            return;
        }

        // Get the current position of the specified point
        Vector3 pointPosition = _lineRenderer.GetPosition(_pointIndex);
        Vector3 newPosition = pointPosition;

        // Apply the target position based on the imitation settings
        if (_imitateX)
            newPosition.x = _targetTransform.position.x + _positionOffset.x;

        if (_imitateY)
            newPosition.y = _targetTransform.position.y + _positionOffset.y;

        // Check if position has changed
        if (newPosition != pointPosition)
        {
            // Update the position of the point
            _lineRenderer.SetPosition(_pointIndex, newPosition);
            _pointPositionChanged = true;
        }
    }

    private void UpdateEdgeCollider()
    {
        if (_edgeCollider2D == null || _lineRenderer == null)
            return;

        // Convert LineRenderer points to Vector2 array for EdgeCollider2D
        Vector2[] colliderPoints = new Vector2[_lineRenderer.positionCount];
        
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            // Convert from world to local space if needed
            Vector3 point = _lineRenderer.useWorldSpace 
                ? transform.InverseTransformPoint(_lineRenderer.GetPosition(i)) 
                : _lineRenderer.GetPosition(i);
                
            colliderPoints[i] = new Vector2(point.x, point.y);
        }

        // Update the edge collider points
        _edgeCollider2D.points = colliderPoints;
    }

    #region Public Methods

    /// <summary>
    /// Set the index of the point to control in the LineRenderer
    /// </summary>
    [Button("Validate Point Index")]
    public void ValidatePointIndex()
    {
        if (_lineRenderer != null && (_pointIndex < 0 || _pointIndex >= _lineRenderer.positionCount))
        {
            Debug.LogWarning($"Point index {_pointIndex} is out of range. LineRenderer has {_lineRenderer.positionCount} points.");
        }
        else
        {
            Debug.Log($"Point index {_pointIndex} is valid.");
        }
    }

    /// <summary>
    /// Set the index of the point to control in the LineRenderer
    /// </summary>
    public void SetPointIndex(int index)
    {
        if (_lineRenderer != null && index >= 0 && index < _lineRenderer.positionCount)
        {
            _pointIndex = index;
        }
        else
        {
            Debug.LogError("Invalid point index: " + index);
        }
    }

    /// <summary>
    /// Set the target transform to imitate
    /// </summary>
    public void SetTargetTransform(Transform target)
    {
        _targetTransform = target;
    }

    /// <summary>
    /// Configure which axes to imitate
    /// </summary>
    public void SetImitationAxes(bool imitateXAxis, bool imitateYAxis)
    {
        _imitateX = imitateXAxis;
        _imitateY = imitateYAxis;
    }

    /// <summary>
    /// Set the position offset
    /// </summary>
    public void SetPositionOffset(Vector2 offset)
    {
        _positionOffset = offset;
    }

    /// <summary>
    /// Enable or disable EdgeCollider2D updates
    /// </summary>
    public void SetEdgeColliderUpdating(bool updateCollider)
    {
        _updateEdgeCollider = updateCollider;
    }

    /// <summary>
    /// Force an immediate update of the EdgeCollider2D based on current LineRenderer points
    /// </summary>
    [Button("Update Edge Collider Now")]
    public void ForceEdgeColliderUpdate()
    {
        if (_edgeCollider2D == null)
        {
            Debug.LogWarning("No EdgeCollider2D assigned or found on this GameObject");
            return;
        }
        UpdateEdgeCollider();
    }

    #endregion
}