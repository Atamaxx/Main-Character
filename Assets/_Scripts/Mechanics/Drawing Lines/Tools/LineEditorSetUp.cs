using NaughtyAttributes;
using UnityEngine;


[ExecuteInEditMode]
public class LineEditorSetUp : MonoBehaviour
{
    public bool _useBoxColliderUpdate = false;
    public bool _useEdgeColliderUpdate = false;

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
    private bool _collidersDirty = true; // if anything changes, we mark colliders as dirty

    private void Start()
    {
        if (Application.isPlaying)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        Tick();
    }

    private void OnEnable()
    {
        Ticker.OnTickAction += Tick;
        MarkCollidersDirty(); // in case something changed while disabled
    }

    private void OnDisable()
    {
        Ticker.OnTickAction -= Tick;
    }



    private void Tick()
    {
        // Ensure this only runs in the Editor, not during play mode
        if (!Application.isPlaying)
        {
            if (!_collidersDirty)
            {
                // We can do a quick check here as well in case user modifies the line’s points externally
                if (CheckLineRendererStateChanged())
                    MarkCollidersDirty();
            }

            if (_collidersDirty)
            {
                // Rebuild only if user has toggled the relevant option
                if (_useEdgeColliderUpdate)
                    SetUpEdgeCollider();

                if (_useBoxColliderUpdate)
                    SetUpSegmentedBoxColliders();

                _collidersDirty = false;
            }
        }
    }

    /// <summary>
    /// Sets a flag to indicate the collider setup should be rebuilt on the next Tick.
    /// </summary>
    private void MarkCollidersDirty()
    {
        _collidersDirty = true;
    }

    /// <summary>
    /// Checks the current LineRenderer state and compares to cached data to see if it’s changed.
    /// This helps us catch changes from external scripts, moving endpoints in the scene, etc.
    /// </summary>
    /// <returns>true if changed; false otherwise.</returns>
    private bool CheckLineRendererStateChanged()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return false;

        int currentPointCount = lineRenderer.positionCount;
        if (currentPointCount != _cachedPointCount) return true;

        // Compare positions
        if (_cachedPositions == null || _cachedPositions.Length != currentPointCount)
            return true;

        Vector3[] currentPositions = new Vector3[currentPointCount];
        lineRenderer.GetPositions(currentPositions);

        for (int i = 0; i < currentPointCount; i++)
        {
            if (currentPositions[i] != _cachedPositions[i])
                return true;
        }

        // Compare thickness & toggles
        float effectiveThickness = (useLineWidthForThickness && boxThickness <= 0f)
            ? lineRenderer.startWidth
            : boxThickness;

        if (!Mathf.Approximately(effectiveThickness, _cachedBoxThickness))
            return true;

        if (_lastUseLineWidthForThickness != useLineWidthForThickness)
            return true;

        return false;
    }

    /// <summary>
    /// After rebuilding colliders, cache the line state so we know if anything changes later.
    /// </summary>
    private void CacheLineState()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        _cachedPointCount = lineRenderer.positionCount;
        if (_cachedPointCount > 0)
        {
            _cachedPositions = new Vector3[_cachedPointCount];
            lineRenderer.GetPositions(_cachedPositions);
        }
        else
        {
            _cachedPositions = null;
        }

        float effectiveThickness = (useLineWidthForThickness && boxThickness <= 0f)
            ? lineRenderer.startWidth
            : boxThickness;
        _cachedBoxThickness = effectiveThickness;
        _lastUseLineWidthForThickness = useLineWidthForThickness;
    }

    [Button("Set Up Edge Collider")]
    private void SetUpEdgeCollider()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError($"[{name}] Missing LineRenderer component!");
            return;
        }

        // Ensure we have an EdgeCollider2D
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }

        int pointCount = lineRenderer.positionCount;
        if (pointCount < 2)
        {
            Debug.LogWarning($"[{name}] LineRenderer has fewer than 2 points. Nothing to build an EdgeCollider from.");
            return;
        }

        // Gather line positions. If the line is using local space, convert them to world space for consistent calculations.
        Vector3[] worldPositions = new Vector3[pointCount];
        if (lineRenderer.useWorldSpace)
        {
            // Positions are already in world space
            lineRenderer.GetPositions(worldPositions);
        }
        else
        {
            // Positions are in local space, so convert them to world space
            Vector3[] localPositions = new Vector3[pointCount];
            lineRenderer.GetPositions(localPositions);
            for (int i = 0; i < pointCount; i++)
            {
                worldPositions[i] = transform.TransformPoint(localPositions[i]);
            }
        }

        // Convert to local space for the EdgeCollider2D
        // (EdgeCollider2D expects positions in the local space of its own transform)
        Vector2[] edgeColliderPoints = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            // Apply extra offset if desired, still in world space
            Vector3 offsetPos = worldPositions[i] + (Vector3)edgeOffset;
            // Now convert to the local space of the EdgeCollider’s transform
            edgeColliderPoints[i] = edgeCollider.transform.InverseTransformPoint(offsetPos);
        }

        // Assign points
        edgeCollider.points = edgeColliderPoints;

        // Cache state so we know we've just updated
        CacheLineState();
    }

    [Button("Set Up Segmented Box Colliders")]
    private void SetUpSegmentedBoxColliders()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError($"[{name}] Missing LineRenderer component!");
            return;
        }

        int pointCount = lineRenderer.positionCount;
        if (pointCount < 2)
        {
            Debug.LogWarning($"[{name}] LineRenderer has fewer than 2 points. No segments to create colliders from.");
            return;
        }

        // Remove old segment colliders if desired
        if (clearOldSegmentColliders)
        {
            ClearOldSegmentColliders();
        }

        // Determine actual thickness
        float effectiveThickness = (useLineWidthForThickness && boxThickness <= 0f)
            ? lineRenderer.startWidth
            : boxThickness;

        // Get line positions in world space regardless of lineRenderer.useWorldSpace
        Vector3[] worldPositions = new Vector3[pointCount];
        if (lineRenderer.useWorldSpace)
        {
            lineRenderer.GetPositions(worldPositions);
        }
        else
        {
            Vector3[] localPositions = new Vector3[pointCount];
            lineRenderer.GetPositions(localPositions);
            for (int i = 0; i < pointCount; i++)
            {
                worldPositions[i] = transform.TransformPoint(localPositions[i]);
            }
        }

        // Create a box collider for each segment (point[i] -> point[i+1]) in world space,
        // then place the child object locally under our transform.
        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 p1 = worldPositions[i];
            Vector3 p2 = worldPositions[i + 1];

            // Compute midpoint in world space then convert to local space
            Vector3 midpointWorld = p1 + (p2 - p1) * 0.5f;
            Vector3 midpointLocal = transform.InverseTransformPoint(midpointWorld);

            // Convert endpoints to local space for proper angle calculation
            Vector3 localP1 = transform.InverseTransformPoint(p1);
            Vector3 localP2 = transform.InverseTransformPoint(p2);
            Vector3 localSegment = localP2 - localP1;
            float localAngle = Mathf.Atan2(localSegment.y, localSegment.x) * Mathf.Rad2Deg;

            // Create a child object for the collider
            GameObject segmentObj = new GameObject($"SegmentCollider_{i}");
            segmentObj.transform.SetParent(transform, false);

            segmentObj.transform.localPosition = new Vector3(midpointLocal.x, midpointLocal.y, colliderZPosition);
            segmentObj.transform.localRotation = Quaternion.Euler(0f, 0f, localAngle);

            // Add box collider and set its properties as before...
            BoxCollider2D boxCollider = segmentObj.AddComponent<BoxCollider2D>();
            segmentObj.layer = segmentsLayer;

            float segmentLength = localSegment.magnitude; // This is now in local space
            float widthWithPadding = effectiveThickness + boxPadding.y;
            float lengthWithPadding = segmentLength + boxPadding.x;

            boxCollider.size = new Vector2(lengthWithPadding, widthWithPadding);
            boxCollider.offset = Vector2.zero;
        }


        // Cache state so we know we've just updated
        CacheLineState();
    }

    /// <summary>
    /// Removes any child objects named 'SegmentCollider_x'.
    /// </summary>
    [Button("Clear Segment Colliders")]
    private void ClearOldSegmentColliders()
    {
        // Collect objects to remove
        var toRemove = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("SegmentCollider_"))
            {
                toRemove.Add(child.gameObject);
            }
        }

        // Destroy them
        foreach (var obj in toRemove)
        {
            DestroyImmediate(obj);
        }
    }
}
