using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this to the same GameObject that has the EdgeCollider2D.
/// It compares a Transform’s Y to the line’s Y at the same X.
/// </summary>
[RequireComponent(typeof(EdgeCollider2D))]
public class CheckUnderEdgeLine : MonoBehaviour
{
    private EdgeCollider2D _edgeCollider;

    private void Awake()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
    }

    /// <summary>
    /// Returns true if 'target' is below (less than) the line’s Y at the exact same X.
    /// </summary>
    public bool IsTransformBelowLine(Transform target)
    {
        if (_edgeCollider == null || _edgeCollider.pointCount < 2)
            return false;

        // 1) Get local points from EdgeCollider2D
        Vector2[] localPoints = _edgeCollider.points;
        // 2) Convert them to world coordinates
        List<Vector2> worldPoints = new List<Vector2>(localPoints.Length);
        for (int i = 0; i < localPoints.Length; i++)
        {
            Vector2 wp = _edgeCollider.transform.TransformPoint(localPoints[i]);
            worldPoints.Add(wp);
        }

        float x = target.position.x;
        float y = target.position.y;

        // 3) Quick checks if X is outside the range of the line
        if (x <= worldPoints[0].x || x >= worldPoints[worldPoints.Count - 1].x)
        {
            return false;
        }


        // 4) Find the segment that brackets this X
        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            float x1 = worldPoints[i].x;
            float x2 = worldPoints[i + 1].x;

            // Check if x is between these two points (allow for either ascending or descending X)
            if ((x1 <= x && x <= x2) || (x2 <= x && x <= x1))
            {
                float segmentXRange = x2 - x1;

                // Avoid division by zero if points share the same X
                float t = (Mathf.Abs(segmentXRange) > 1e-6f)
                    ? (x - x1) / segmentXRange
                    : 0f;

                // Linear interpolation to find the line’s Y at this X
                float lineY = Mathf.Lerp(worldPoints[i].y, worldPoints[i + 1].y, t);

                // Finally, compare target’s Y to the line’s Y
                return (y < lineY);
            }
        }

        // If no bracket was found (could happen if line doubles back in X), 
        // you might return false or handle it differently
        return false;
    }
}
