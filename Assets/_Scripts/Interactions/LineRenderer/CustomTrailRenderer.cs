using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CustomTrailRenderer : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("How long each trail point stays visible (in seconds).")]
    public float trailTime = 0.2f;

    [Tooltip("Minimum distance between trail points.")]
    public float trailMinVertexDistance = 0.05f;

    [Tooltip("Starting width of the trail (newest point).")]
    public float startWidth = 0.1f;

    [Tooltip("Ending width of the trail (oldest point).")]
    public float endWidth = 0f;

    [Tooltip("Starting color of the trail (newest point).")]
    public Color startColor = Color.white;

    [Tooltip("Ending color of the trail (oldest point).")]
    public Color endColor = new Color(1f, 1f, 1f, 0f);

    [Header("Smoothing Settings")]
    [Tooltip("Number of interpolated points per segment for smoothing.")]
    public int smoothingSteps = 4;

    [Tooltip("Tension parameter for Kochanek–Bartels spline. (0 = Catmull–Rom)")]
    public float tension = 0f;

    [Tooltip("Bias parameter for Kochanek–Bartels spline.")]
    public float bias = 0f;

    [Tooltip("Continuity parameter for Kochanek–Bartels spline.")]
    public float continuity = 0f;

    // Represents a raw trail point.
    private class TrailPoint
    {
        public Vector3 position;
        public float time;

        public TrailPoint(Vector3 pos, float t)
        {
            position = pos;
            time = t;
        }
    }

    private List<TrailPoint> points = new List<TrailPoint>();
    private LineRenderer lineRenderer;

    // Stores the current trail length (computed from smoothed positions).
    private float _currentTrailLength = 0f;
    public float CurrentTrailLength
    {
        get { return _currentTrailLength; }
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void Update()
    {
        float currentTime = Time.unscaledTime;
        Vector3 currentPosition = transform.position;

        // Add new point if none exist or if moved sufficiently.
        if (
            points.Count == 0
            || Vector3.Distance(points[points.Count - 1].position, currentPosition)
                > trailMinVertexDistance
        )
        {
            points.Add(new TrailPoint(currentPosition, currentTime));
        }

        // Remove points older than the trail's lifetime.
        while (points.Count > 0 && currentTime - points[0].time > trailTime)
        {
            points.RemoveAt(0);
        }

        // Generate smooth positions using Kochanek–Bartels spline interpolation.
        List<Vector3> smoothPositions = GetSmoothedPositionsKochanekBartels(
            points,
            smoothingSteps,
            tension,
            bias,
            continuity
        );

        // Ensure at least two points exist.
        if (smoothPositions.Count < 2)
        {
            if (smoothPositions.Count == 1)
                smoothPositions.Add(smoothPositions[0]);
            else
            {
                smoothPositions.Add(currentPosition);
                smoothPositions.Add(currentPosition);
            }
        }

        // Update the LineRenderer with the smooth positions.
        lineRenderer.positionCount = smoothPositions.Count;
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            lineRenderer.SetPosition(i, smoothPositions[i]);
        }

        // Build and assign the width curve.
        AnimationCurve widthCurve = new AnimationCurve();
        int count = smoothPositions.Count;
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? (float)i / (count - 1) : 1f;
            float width = Mathf.Lerp(endWidth, startWidth, t);
            widthCurve.AddKey(t, width);
        }
        lineRenderer.widthCurve = widthCurve;

        // Build a gradient with a maximum of 8 keys.
        int keyCount = Mathf.Min(count, 8);
        GradientColorKey[] colorKeys = new GradientColorKey[keyCount];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyCount];
        for (int i = 0; i < keyCount; i++)
        {
            int index = count > 1 ? Mathf.RoundToInt((count - 1) * ((float)i / (keyCount - 1))) : 0;
            float t = count > 1 ? (float)index / (count - 1) : 1f;
            Color col = Color.Lerp(endColor, startColor, t);
            colorKeys[i] = new GradientColorKey(col, t);
            alphaKeys[i] = new GradientAlphaKey(col.a, t);
        }
        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;

        // Compute total trail length from the smooth positions.
        _currentTrailLength = 0f;
        for (int i = 1; i < smoothPositions.Count; i++)
        {
            _currentTrailLength += Vector3.Distance(smoothPositions[i - 1], smoothPositions[i]);
        }
    }

    // Returns smooth positions using Kochanek–Bartels spline interpolation.
    private List<Vector3> GetSmoothedPositionsKochanekBartels(
        List<TrailPoint> rawPoints,
        int steps,
        float tension,
        float bias,
        float continuity
    )
    {
        List<Vector3> smoothPositions = new List<Vector3>();
        int n = rawPoints.Count;
        if (n == 0)
            return smoothPositions;
        if (n < 2)
        {
            smoothPositions.Add(rawPoints[0].position);
            smoothPositions.Add(rawPoints[0].position);
            return smoothPositions;
        }

        for (int i = 0; i < n - 1; i++)
        {
            Vector3 p0 = (i == 0) ? rawPoints[i].position : rawPoints[i - 1].position;
            Vector3 p1 = rawPoints[i].position;
            Vector3 p2 = rawPoints[i + 1].position;
            Vector3 p3 = (i + 2 < n) ? rawPoints[i + 2].position : rawPoints[i + 1].position;

            // Calculate tangents using Kochanek–Bartels formulation.
            Vector3 m1 =
                (1 - tension)
                * (
                    (1 + bias) * (1 - continuity) * 0.5f * (p1 - p0)
                    + (1 - bias) * (1 + continuity) * 0.5f * (p2 - p1)
                );
            Vector3 m2 =
                (1 - tension)
                * (
                    (1 + bias) * (1 + continuity) * 0.5f * (p2 - p1)
                    + (1 - bias) * (1 - continuity) * 0.5f * (p3 - p2)
                );

            // Generate 'steps' interpolated points between p1 and p2.
            for (int j = 0; j < steps; j++)
            {
                float t = j / (float)steps;
                float t2 = t * t;
                float t3 = t2 * t;
                Vector3 point =
                    (2 * t3 - 3 * t2 + 1) * p1
                    + (t3 - 2 * t2 + t) * m1
                    + (-2 * t3 + 3 * t2) * p2
                    + (t3 - t2) * m2;
                smoothPositions.Add(point);
            }
        }
        smoothPositions.Add(rawPoints[n - 1].position);
        return smoothPositions;
    }

    // Add this method to your CustomTrailRenderer class
    public void ForceFirstPoint(Vector3 position)
    {
        if (points.Count > 0)
        {
            points[0] = new TrailPoint(position, points[0].time);
        }
        else
        {
            points.Add(new TrailPoint(position, Time.unscaledTime));
        }
    }
}
