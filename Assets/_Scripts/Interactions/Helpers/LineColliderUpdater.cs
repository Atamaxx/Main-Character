using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

public class LineColliderUpdater : MonoBehaviour
{
    [Header("Collider Update Settings")]
    [Tooltip("Enable to update box colliders in play mode")]
    public bool useBoxColliders = false;
    
    [Tooltip("Enable to update edge colliders in play mode")]
    public bool useEdgeColliders = false;
    
    [Tooltip("How often to check for changes (in seconds). Lower values are more responsive but less efficient.")]
    [Range(0.05f, 1.0f)]
    public float updateInterval = 0.2f;

    [Header("Box Collider Settings")]
    [Tooltip("Thickness of each box collider segment. If left at 0, uses the line's startWidth.")]
    public float boxThickness = 0.0f;

    [Tooltip("Padding to expand or contract the generated box colliders along x/y.")]
    public Vector2 boxPadding = Vector2.zero;

    [Tooltip("If true, any child objects named 'SegmentCollider_x' will be destroyed before creating new ones.")]
    public bool clearOldSegmentColliders = true;
    [Layer] public int segmentsLayer;

    [Header("Edge Collider Settings")]
    [Tooltip("Extra offset for EdgeCollider if desired.")]
    public Vector2 edgeOffset = Vector2.zero;

    [Header("Other Settings")]
    [Tooltip("Check this if the line has a uniform width and you want the colliders to use 'startWidth'.")]
    public bool useLineWidthForThickness = true;

    [Tooltip("Set the z-position of the colliders (to avoid collisions or visual overlaps).")]
    public float colliderZPosition = 0f;

    // --- Track changes so we only rebuild if needed ---
    private int _cachedPointCount = -1;
    private Vector3[] _cachedPositions;
    private float _cachedBoxThickness;
    private bool _lastUseLineWidthForThickness;
    private bool _collidersDirty = true;
    
    // Optimization variables
    private LineRenderer _lineRenderer;
    private EdgeCollider2D _edgeCollider;
    private float _timeSinceLastCheck;
    private List<GameObject> _segmentColliders = new List<GameObject>();

    private void Awake()
    {
        // This script only works in play mode
        if (!Application.isPlaying)
        {
            enabled = false;
            return;
        }

        // Cache components for better performance
        _lineRenderer = GetComponent<LineRenderer>();
        _edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private void Start()
    {
        // Initialize colliders at the start
        UpdateColliders();
    }

    private void OnEnable()
    {
        // Only enable in play mode
        if (!Application.isPlaying)
        {
            enabled = false;
            return;
        }
        
        MarkCollidersDirty();
    }

    private void Update()
    {
        // Skip if neither collider type is enabled
        if (!useBoxColliders && !useEdgeColliders)
            return;
            
        // Use time-based interval checking for better performance
        _timeSinceLastCheck += Time.deltaTime;
        if (_timeSinceLastCheck >= updateInterval)
        {
            if (CheckLineRendererStateChanged())
            {
                UpdateColliders();
            }
            _timeSinceLastCheck = 0f;
        }
    }

    private void UpdateColliders()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null) return;
        }

        if (useEdgeColliders)
            SetUpEdgeCollider();

        if (useBoxColliders)
            SetUpSegmentedBoxColliders();

        _collidersDirty = false;
    }

    /// <summary>
    /// Sets a flag to indicate the collider setup should be rebuilt on the next update.
    /// </summary>
    private void MarkCollidersDirty()
    {
        _collidersDirty = true;
    }

    /// <summary>
    /// Checks the current LineRenderer state and compares to cached data to see if it's changed.
    /// </summary>
    /// <returns>true if changed; false otherwise.</returns>
    private bool CheckLineRendererStateChanged()
    {
        if (_lineRenderer == null) return false;

        int currentPointCount = _lineRenderer.positionCount;
        if (currentPointCount != _cachedPointCount) return true;

        // Quick exit if no points to check
        if (currentPointCount == 0) return false;

        // Compare positions (only if we have points)
        if (_cachedPositions == null || _cachedPositions.Length != currentPointCount)
            return true;

        Vector3[] currentPositions = new Vector3[currentPointCount];
        _lineRenderer.GetPositions(currentPositions);

        // Use a threshold for floating point comparison to avoid rebuilding due to tiny changes
        const float positionThreshold = 0.001f;
        for (int i = 0; i < currentPointCount; i++)
        {
            if (Vector3.SqrMagnitude(currentPositions[i] - _cachedPositions[i]) > positionThreshold * positionThreshold)
                return true;
        }

        // Compare thickness & toggles
        float effectiveThickness = GetEffectiveThickness();
        if (!Mathf.Approximately(effectiveThickness, _cachedBoxThickness))
            return true;

        if (_lastUseLineWidthForThickness != useLineWidthForThickness)
            return true;

        return false;
    }

    private float GetEffectiveThickness()
    {
        if (_lineRenderer == null) return boxThickness;
        
        return (useLineWidthForThickness && boxThickness <= 0f)
            ? _lineRenderer.startWidth
            : boxThickness;
    }

    /// <summary>
    /// After rebuilding colliders, cache the line state so we know if anything changes later.
    /// </summary>
    private void CacheLineState()
    {
        if (_lineRenderer == null) return;

        _cachedPointCount = _lineRenderer.positionCount;
        if (_cachedPointCount > 0)
        {
            // Resize array only if needed
            if (_cachedPositions == null || _cachedPositions.Length != _cachedPointCount)
                _cachedPositions = new Vector3[_cachedPointCount];
                
            _lineRenderer.GetPositions(_cachedPositions);
        }
        else
        {
            _cachedPositions = null;
        }

        _cachedBoxThickness = GetEffectiveThickness();
        _lastUseLineWidthForThickness = useLineWidthForThickness;
    }

    private void SetUpEdgeCollider()
    {
        if (_lineRenderer == null) return;

        // Ensure we have an EdgeCollider2D
        if (_edgeCollider == null)
        {
            _edgeCollider = GetComponent<EdgeCollider2D>();
            if (_edgeCollider == null)
            {
                _edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            }
        }

        int pointCount = _lineRenderer.positionCount;
        if (pointCount < 2)
        {
            if (_edgeCollider.pointCount > 0)
                _edgeCollider.points = new Vector2[0]; // Clear points
            return;
        }

        // Reuse arrays when possible to reduce garbage collection
        Vector3[] worldPositions = new Vector3[pointCount];
        Vector2[] edgeColliderPoints = new Vector2[pointCount];

        // Gather line positions
        if (_lineRenderer.useWorldSpace)
        {
            _lineRenderer.GetPositions(worldPositions);
        }
        else
        {
            Vector3[] localPositions = new Vector3[pointCount];
            _lineRenderer.GetPositions(localPositions);
            for (int i = 0; i < pointCount; i++)
            {
                worldPositions[i] = transform.TransformPoint(localPositions[i]);
            }
        }

        // Convert to local space for the EdgeCollider2D
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 offsetPos = worldPositions[i] + (Vector3)edgeOffset;
            edgeColliderPoints[i] = _edgeCollider.transform.InverseTransformPoint(offsetPos);
        }

        // Assign points
        _edgeCollider.points = edgeColliderPoints;

        // Cache state
        CacheLineState();
    }

    private void SetUpSegmentedBoxColliders()
    {
        if (_lineRenderer == null) return;

        int pointCount = _lineRenderer.positionCount;
        if (pointCount < 2)
        {
            ClearSegmentColliders();
            return;
        }

        // Remove old segment colliders if desired
        if (clearOldSegmentColliders)
        {
            ClearSegmentColliders();
        }

        // Determine actual thickness
        float effectiveThickness = GetEffectiveThickness();

        // Reuse arrays to reduce garbage collection
        Vector3[] worldPositions = new Vector3[pointCount];
        
        // Get line positions in world space
        if (_lineRenderer.useWorldSpace)
        {
            _lineRenderer.GetPositions(worldPositions);
        }
        else
        {
            Vector3[] localPositions = new Vector3[pointCount];
            _lineRenderer.GetPositions(localPositions);
            for (int i = 0; i < pointCount; i++)
            {
                worldPositions[i] = transform.TransformPoint(localPositions[i]);
            }
        }

        // Track how many colliders we need to create or reuse
        int segmentCount = pointCount - 1;
        int existingCount = _segmentColliders.Count;
        
        // Create box colliders for each segment
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 p1 = worldPositions[i];
            Vector3 p2 = worldPositions[i + 1];

            // Compute midpoint and convert to local space
            Vector3 midpointWorld = p1 + (p2 - p1) * 0.5f;
            Vector3 midpointLocal = transform.InverseTransformPoint(midpointWorld);

            // Calculate angle in local space
            Vector3 localP1 = transform.InverseTransformPoint(p1);
            Vector3 localP2 = transform.InverseTransformPoint(p2);
            Vector3 localSegment = localP2 - localP1;
            float localAngle = Mathf.Atan2(localSegment.y, localSegment.x) * Mathf.Rad2Deg;
            float segmentLength = localSegment.magnitude;

            // Reuse existing collider or create new one
            GameObject segmentObj;
            BoxCollider2D boxCollider;
            
            if (i < existingCount)
            {
                // Reuse existing collider
                segmentObj = _segmentColliders[i];
                boxCollider = segmentObj.GetComponent<BoxCollider2D>();
            }
            else
            {
                // Create new collider
                segmentObj = new GameObject($"SegmentCollider_{i}");
                segmentObj.transform.SetParent(transform, false);
                boxCollider = segmentObj.AddComponent<BoxCollider2D>();
                _segmentColliders.Add(segmentObj);
            }

            // Position and configure collider
            segmentObj.transform.localPosition = new Vector3(midpointLocal.x, midpointLocal.y, colliderZPosition);
            segmentObj.transform.localRotation = Quaternion.Euler(0f, 0f, localAngle);
            segmentObj.layer = segmentsLayer;

            float widthWithPadding = effectiveThickness + boxPadding.y;
            float lengthWithPadding = segmentLength + boxPadding.x;

            boxCollider.size = new Vector2(lengthWithPadding, widthWithPadding);
            boxCollider.offset = Vector2.zero;
        }

        // Remove any excess colliders
        if (existingCount > segmentCount)
        {
            for (int i = existingCount - 1; i >= segmentCount; i--)
            {
                Destroy(_segmentColliders[i]);
                _segmentColliders.RemoveAt(i);
            }
        }

        // Cache state
        CacheLineState();
    }

    /// <summary>
    /// Removes segment colliders
    /// </summary>
    private void ClearSegmentColliders()
    {
        // First clear our tracked list
        for (int i = _segmentColliders.Count - 1; i >= 0; i--)
        {
            if (_segmentColliders[i] != null)
            {
                Destroy(_segmentColliders[i]);
            }
        }
        _segmentColliders.Clear();
        
        // Then look for any others that might have been created outside our knowledge
        var toRemove = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("SegmentCollider_"))
            {
                toRemove.Add(child.gameObject);
            }
        }

        foreach (var obj in toRemove)
        {
            Destroy(obj);
        }
    }

    private void OnDestroy()
    {
        ClearSegmentColliders();
    }
}