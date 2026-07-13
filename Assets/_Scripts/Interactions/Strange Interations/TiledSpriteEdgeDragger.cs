using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Makes one edge of a tiled Sprite Renderer follow a transform as if being dragged,
/// adjusting both position and size dynamically. Works correctly with scaled objects.
/// </summary>
public class TiledSpriteEdgeDragger : MonoBehaviour
{
    public enum DragEdge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [Foldout("Components")]
    [Required("Sprite Renderer is required")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Foldout("Drag Settings")]
    [SerializeField] private DragEdge _dragEdge = DragEdge.Right;

    [Foldout("Target Settings")]
    [Required("Target Transform is required")]
    [SerializeField] private Transform _targetTransform;

    [Foldout("Imitation Settings")]
    [SerializeField] private bool _imitateX = true;

    [Foldout("Imitation Settings")]
    [SerializeField] private bool _imitateY = true;

    [Foldout("Imitation Settings")]
    [SerializeField] private Vector2 _positionOffset = Vector2.zero;

    [Foldout("Constraints")]
    [SerializeField] private bool _useMinSize = true;

    [Foldout("Constraints")]
    [ShowIf("_useMinSize")]
    [SerializeField] private float _minWidth = 0.1f;

    [Foldout("Constraints")]
    [ShowIf("_useMinSize")]
    [SerializeField] private float _minHeight = 0.1f;

    [Foldout("Debug")]
    [SerializeField] private bool _showDebugGizmos = false;

    // Cached values to optimize performance
    private Vector2 _initialSize;
    private Vector3 _initialPosition;
    private Vector3 _initialScale;
    private Vector2 _anchorPoint;
    private Vector2 _lastTargetPosition;

    private void Awake()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // Cache the initial values
        _initialSize = _spriteRenderer.size;
        _initialPosition = transform.position;
        _initialScale = transform.localScale;
        
        // Calculate the anchor point based on the drag edge
        CalculateAnchorPoint();
        
        if (_targetTransform != null)
        {
            _lastTargetPosition = _targetTransform.position;
        }
    }

    private void CalculateAnchorPoint()
    {
        if (_spriteRenderer == null) return;

        // Get the sprite's current world-space bounds
        Bounds bounds = _spriteRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        // Calculate the anchor point in world space based on the drag edge
        switch (_dragEdge)
        {
            case DragEdge.Left:
                _anchorPoint = new Vector2(center.x + extents.x, center.y);
                break;
            case DragEdge.Right:
                _anchorPoint = new Vector2(center.x - extents.x, center.y);
                break;
            case DragEdge.Top:
                _anchorPoint = new Vector2(center.x, center.y - extents.y);
                break;
            case DragEdge.Bottom:
                _anchorPoint = new Vector2(center.x, center.y + extents.y);
                break;
        }
    }

    private void LateUpdate()
    {
        if (_targetTransform == null || _spriteRenderer == null)
            return;

        Vector2 targetPosition = _targetTransform.position;
        Vector3 currentScale = transform.localScale;
        Bounds bounds = _spriteRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        Vector2 currentSize = _spriteRenderer.size;
        Vector2 newSize = currentSize;
        Vector3 newPosition = transform.position;

        // Apply the target position based on the imitation settings and drag edge
        switch (_dragEdge)
        {
            case DragEdge.Left:
                if (_imitateX)
                {
                    float targetX = targetPosition.x + _positionOffset.x;
                    float rightEdgeX = _anchorPoint.x;
                    float worldSpaceWidth = rightEdgeX - targetX;
                    
                    // Convert world space width to size (accounting for scale)
                    float newWidth = worldSpaceWidth / Mathf.Abs(currentScale.x);
                    
                    if (_useMinSize && newWidth < _minWidth)
                    {
                        newWidth = _minWidth;
                        worldSpaceWidth = newWidth * Mathf.Abs(currentScale.x);
                        targetX = rightEdgeX - worldSpaceWidth;
                    }
                    
                    newSize.x = newWidth;
                    
                    // Calculate new center position
                    float centerX = (targetX + rightEdgeX) * 0.5f;
                    newPosition = new Vector3(centerX, newPosition.y, newPosition.z);
                }
                if (_imitateY)
                {
                    newPosition.y = targetPosition.y + _positionOffset.y;
                }
                break;

            case DragEdge.Right:
                if (_imitateX)
                {
                    float targetX = targetPosition.x + _positionOffset.x;
                    float leftEdgeX = _anchorPoint.x;
                    float worldSpaceWidth = targetX - leftEdgeX;
                    
                    // Convert world space width to size (accounting for scale)
                    float newWidth = worldSpaceWidth / Mathf.Abs(currentScale.x);
                    
                    if (_useMinSize && newWidth < _minWidth)
                    {
                        newWidth = _minWidth;
                        worldSpaceWidth = newWidth * Mathf.Abs(currentScale.x);
                        targetX = leftEdgeX + worldSpaceWidth;
                    }
                    
                    newSize.x = newWidth;
                    
                    // Calculate new center position
                    float centerX = (leftEdgeX + targetX) * 0.5f;
                    newPosition = new Vector3(centerX, newPosition.y, newPosition.z);
                }
                if (_imitateY)
                {
                    newPosition.y = targetPosition.y + _positionOffset.y;
                }
                break;

            case DragEdge.Top:
                if (_imitateY)
                {
                    float targetY = targetPosition.y + _positionOffset.y;
                    float bottomEdgeY = _anchorPoint.y;
                    float worldSpaceHeight = targetY - bottomEdgeY;
                    
                    // Convert world space height to size (accounting for scale)
                    float newHeight = worldSpaceHeight / Mathf.Abs(currentScale.y);
                    
                    if (_useMinSize && newHeight < _minHeight)
                    {
                        newHeight = _minHeight;
                        worldSpaceHeight = newHeight * Mathf.Abs(currentScale.y);
                        targetY = bottomEdgeY + worldSpaceHeight;
                    }
                    
                    newSize.y = newHeight;
                    
                    // Calculate new center position
                    float centerY = (bottomEdgeY + targetY) * 0.5f;
                    newPosition = new Vector3(newPosition.x, centerY, newPosition.z);
                }
                if (_imitateX)
                {
                    newPosition.x = targetPosition.x + _positionOffset.x;
                }
                break;

            case DragEdge.Bottom:
                if (_imitateY)
                {
                    float targetY = targetPosition.y + _positionOffset.y;
                    float topEdgeY = _anchorPoint.y;
                    float worldSpaceHeight = topEdgeY - targetY;
                    
                    // Convert world space height to size (accounting for scale)
                    float newHeight = worldSpaceHeight / Mathf.Abs(currentScale.y);
                    
                    if (_useMinSize && newHeight < _minHeight)
                    {
                        newHeight = _minHeight;
                        worldSpaceHeight = newHeight * Mathf.Abs(currentScale.y);
                        targetY = topEdgeY - worldSpaceHeight;
                    }
                    
                    newSize.y = newHeight;
                    
                    // Calculate new center position
                    float centerY = (targetY + topEdgeY) * 0.5f;
                    newPosition = new Vector3(newPosition.x, centerY, newPosition.z);
                }
                if (_imitateX)
                {
                    newPosition.x = targetPosition.x + _positionOffset.x;
                }
                break;
        }

        // Apply the changes
        _spriteRenderer.size = newSize;
        transform.position = newPosition;

        // Recalculate anchor point after size change
        CalculateAnchorPoint();

        // Update last target position
        _lastTargetPosition = targetPosition;
    }

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || !enabled)
            return;

        if (_spriteRenderer != null)
        {
            // Draw the sprite bounds
            Gizmos.color = Color.yellow;
            Bounds bounds = _spriteRenderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Draw the anchor point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_anchorPoint, 0.1f);

            // Draw a line to the target if available
            if (_targetTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_anchorPoint, _targetTransform.position);
            }
        }
    }

    #region Public Methods

    /// <summary>
    /// Set which edge to drag
    /// </summary>
    public void SetDragEdge(DragEdge edge)
    {
        _dragEdge = edge;
        CalculateAnchorPoint();
    }

    /// <summary>
    /// Set the target transform to follow
    /// </summary>
    public void SetTargetTransform(Transform target)
    {
        _targetTransform = target;
        if (_targetTransform != null)
        {
            _lastTargetPosition = _targetTransform.position;
        }
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
    /// Reset the sprite to its initial state
    /// </summary>
    [Button("Reset Sprite")]
    public void ResetSprite()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.size = _initialSize;
            transform.position = _initialPosition;
            transform.localScale = _initialScale;
            CalculateAnchorPoint();
        }
    }

    #endregion
}