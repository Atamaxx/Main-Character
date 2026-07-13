using System.Collections.Generic;
using UnityEngine;

public class SmoothLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int segments = 10; // Number of segments between original points

    void Start()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("Line Renderer not set.");
            return;
        }

        SmoothOutLine();
    }

    [ContextMenu("Smooth Line")]
    void SmoothOutLine()
    {
        List<Vector3> originalPoints = new List<Vector3>();
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            originalPoints.Add(lineRenderer.GetPosition(i));
        }

        if (originalPoints.Count < 2)
        {
            Debug.LogError("Not enough points in the Line Renderer.");
            return;
        }

        List<Vector3> smoothedPoints = new List<Vector3>();

        for (int i = 0; i < originalPoints.Count - 1; i++)
        {
            Vector3 p0 = originalPoints[Mathf.Clamp(i - 1, 0, originalPoints.Count - 1)];
            Vector3 p1 = originalPoints[i];
            Vector3 p2 = originalPoints[i + 1];
            Vector3 p3 = originalPoints[Mathf.Clamp(i + 2, 0, originalPoints.Count - 1)];

            for (int s = 0; s < segments; s++)
            {
                float t = (float)s / segments;
                smoothedPoints.Add(GetCatmullRomPoint(t, p0, p1, p2, p3));
            }
        }

        lineRenderer.positionCount = smoothedPoints.Count;
        lineRenderer.SetPositions(smoothedPoints.ToArray()); // Convert List<Vector3> to Vector3[]
    }

    Vector3 GetCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t) +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + p2) * t +
            2f * p1
        );
    }
}
