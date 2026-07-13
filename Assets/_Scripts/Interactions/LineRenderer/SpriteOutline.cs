using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    // Cached components
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;

    [Header("Line Renderer Settings")]
    [SerializeField, Tooltip("Width of the outline line.")]
    private float _lineWidth = 0.1f;

    [SerializeField, Tooltip("Color of the outline line.")]
    private Color _lineColor = Color.red;

    [SerializeField, Tooltip("Custom material for the line. If left empty, the default 'Sprites/Default' shader is used.")]
    private Material _lineMaterial;

    [SerializeField, Tooltip("Resolution for the outline line. " +
                              "Specifies the number of interpolated points between each physics shape vertex (0 for no interpolation).")]
    private int _lineResolution = 0;

    private void Awake()
    {
        // Cache the SpriteRenderer component.
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Optionally, draw the outline automatically on start.
        DrawOutline();
    }

    /// <summary>
    /// Draws a LineRenderer outline around the sprite based on its physics shape.
    /// Ensure the sprite has "Generate Physics Shape" enabled in its import settings.
    /// </summary>
    [Button("Draw Outline")]
    public void DrawOutline()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null)
        {
            Debug.LogError("SpriteRenderer or its sprite is missing.");
            return;
        }

        // Retrieve the physics shape from the sprite.
        List<Vector2> physicsShape = new List<Vector2>();
        if (_spriteRenderer.sprite.GetPhysicsShapeCount() > 0)
        {
            _spriteRenderer.sprite.GetPhysicsShape(0, physicsShape);
        }
        else
        {
            Debug.LogError("Sprite does not have physics shape data. " +
                           "Please enable 'Generate Physics Shape' in the Sprite Import Settings.");
            return;
        }

        // Create or get the LineRenderer component.
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configure the LineRenderer.
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;
        _lineRenderer.widthMultiplier = _lineWidth;
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;
        _lineRenderer.material = (_lineMaterial != null)
            ? _lineMaterial
            : new Material(Shader.Find("Sprites/Default"));

        // Convert the physics shape points (in local space) to world space.
        List<Vector3> rawWorldPoints = new List<Vector3>();
        foreach (Vector2 point in physicsShape)
        {
            Vector3 localPoint = new Vector3(point.x, point.y, 0f);
            rawWorldPoints.Add(_spriteRenderer.transform.TransformPoint(localPoint));
        }

        // Generate the final set of points, with interpolation if desired.
        List<Vector3> finalPoints = new List<Vector3>();
        int count = rawWorldPoints.Count;
        if (count < 2)
        {
            Debug.LogError("Not enough points to create an outline.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 currentPoint = rawWorldPoints[i];
            Vector3 nextPoint = rawWorldPoints[(i + 1) % count];

            // Add the current vertex.
            finalPoints.Add(currentPoint);

            // Add interpolated points between the current and next vertex.
            if (_lineResolution > 0)
            {
                for (int j = 1; j <= _lineResolution; j++)
                {
                    float t = j / (float)(_lineResolution + 1);
                    Vector3 interpolatedPoint = Vector3.Lerp(currentPoint, nextPoint, t);
                    finalPoints.Add(interpolatedPoint);
                }
            }
        }

        // Update the LineRenderer with the calculated positions.
        _lineRenderer.positionCount = finalPoints.Count;
        _lineRenderer.SetPositions(finalPoints.ToArray());
    }
}
