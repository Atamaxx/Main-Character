using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(LineSmoother))]
public class LineSmootherEditor : Editor 
{
    private LineSmoother smoother;

    // Serialized properties
    private SerializedProperty lineProp;
    private SerializedProperty initialStateProp;
    private SerializedProperty smoothingLengthProp;
    private SerializedProperty totalPointsProp;

    // GUI labels
    private GUIContent updateInitialStateContent = new GUIContent("Set Initial State");
    private GUIContent smoothButtonContent = new GUIContent("Smooth Path");
    private GUIContent restoreDefaultContent = new GUIContent("Restore Default Path");

    // Cached Bezier curves – one for each segment between positions
    private BezierCurve[] curves;

    private void OnEnable()
    {
        smoother = (LineSmoother)target;
        if (smoother.Line == null)
        {
            smoother.Line = smoother.GetComponent<LineRenderer>();
        }

        lineProp = serializedObject.FindProperty("Line");
        initialStateProp = serializedObject.FindProperty("InitialState");
        smoothingLengthProp = serializedObject.FindProperty("SmoothingLength");
        totalPointsProp = serializedObject.FindProperty("TotalPoints");

        EnsureCurvesMatchLineRendererPositions();
    }

    public override void OnInspectorGUI()
    {
        if (smoother == null)
            return;

        EnsureCurvesMatchLineRendererPositions();

        EditorGUILayout.PropertyField(lineProp);
        EditorGUILayout.PropertyField(initialStateProp);
        EditorGUILayout.PropertyField(smoothingLengthProp);
        EditorGUILayout.PropertyField(totalPointsProp);

        if (GUILayout.Button(updateInitialStateContent))
        {
            // Backup the current positions
            smoother.InitialState = new Vector3[smoother.Line.positionCount];
            smoother.Line.GetPositions(smoother.InitialState);
        }

        EditorGUILayout.BeginHorizontal();
        {
            GUI.enabled = smoother.Line.positionCount >= 3;
            if (GUILayout.Button(smoothButtonContent))
            {
                SmoothPath();
            }

            // Only allow "restore" if the current line differs from the backup.
            bool lineAndInitialStateMatch = smoother.Line.positionCount == smoother.InitialState.Length;
            if (lineAndInitialStateMatch)
            {
                Vector3[] positions = new Vector3[smoother.Line.positionCount];
                smoother.Line.GetPositions(positions);
                lineAndInitialStateMatch = positions.SequenceEqual(smoother.InitialState);
            }
            GUI.enabled = !lineAndInitialStateMatch;
            if (GUILayout.Button(restoreDefaultContent))
            {
                smoother.Line.positionCount = smoother.InitialState.Length;
                smoother.Line.SetPositions(smoother.InitialState);
                EnsureCurvesMatchLineRendererPositions();
            }
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Smooth the path by re-sampling along the composite Bézier curve.
    /// Instead of specifying points per segment, the user chooses a total number of points.
    /// </summary>
    private void SmoothPath()
    {
        int totalPoints = totalPointsProp.intValue;
        int curveCount = curves.Length; // one curve per segment between original points

        // Allocate new positions for the LineRenderer.
        smoother.Line.positionCount = totalPoints;

        // For each point index, compute a global parameter (0…1) and then figure out which segment (curve) that falls into.
        for (int i = 0; i < totalPoints; i++)
        {
            float tGlobal = (float)i / (totalPoints - 1); // goes from 0 to 1
            float tCurve = tGlobal * curveCount;          // maps to the total number of curves
            int curveIndex = Mathf.Min(Mathf.FloorToInt(tCurve), curveCount - 1);
            float localT = tCurve - curveIndex;             // parameter within the chosen segment

            Vector3 point = EvaluateBezier(curves[curveIndex].Points, localT);
            smoother.Line.SetPosition(i, point);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Evaluate a cubic Bézier at parameter t.
    /// </summary>
    private Vector3 EvaluateBezier(Vector3[] pts, float t)
    {
        float u = 1 - t;
        return u * u * u * pts[0]
             + 3 * u * u * t * pts[1]
             + 3 * u * t * t * pts[2]
             + t * t * t * pts[3];
    }

    /// <summary>
    /// In the Scene view, show control points and draw the sampled Bézier segments.
    /// The tangents are computed via a Catmull–Rom style approach to help avoid sharp corners.
    /// </summary>
    private void OnSceneGUI()
    {
        if (smoother.Line.positionCount < 3)
            return;

        EnsureCurvesMatchLineRendererPositions();

        // Retrieve the current positions from the LineRenderer.
        Vector3[] positions = new Vector3[smoother.Line.positionCount];
        smoother.Line.GetPositions(positions);

        // For each segment between positions, compute the control points.
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 p0 = positions[i];
            Vector3 p1 = positions[i + 1];

            // Compute the tangent at the start point.
            Vector3 tangent0 = (i == 0)
                ? (p1 - p0)
                : (p1 - positions[i - 1]) * (smoothingLengthProp.floatValue / 2f);

            // Compute the tangent at the end point.
            Vector3 tangent1 = (i + 1 == positions.Length - 1)
                ? (p1 - p0)
                : (positions[i + 2] - p0) * (smoothingLengthProp.floatValue / 2f);

            // Convert the Catmull–Rom tangents to cubic Bézier control points.
            curves[i].Points[0] = p0;
            curves[i].Points[1] = p0 + tangent0 / 3f;
            curves[i].Points[2] = p1 - tangent1 / 3f;
            curves[i].Points[3] = p1;

            // Draw handles for the control points.
            Handles.color = Color.green;
            Handles.DotHandleCap(0, curves[i].Points[1], Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.blue;
            Handles.DotHandleCap(0, curves[i].Points[2], Quaternion.identity, 0.1f, EventType.Repaint);
        }

        DrawSegments();
    }

    /// <summary>
    /// Draw each Bézier segment (using a fixed resolution) so you can see the smooth curve in the Scene view.
    /// </summary>
    private void DrawSegments()
    {
        int resolution = 20; // resolution for drawing each segment
        for (int i = 0; i < curves.Length; i++)
        {
            Vector3[] segPoints = curves[i].GetSegments(resolution);
            for (int j = 0; j < segPoints.Length - 1; j++)
            {
                Handles.color = Color.white;
                Handles.DrawLine(segPoints[j], segPoints[j + 1]);
            }
        }
    }

    /// <summary>
    /// Makes sure that the internal array of Bézier curves matches the number of segments in the LineRenderer.
    /// </summary>
    private void EnsureCurvesMatchLineRendererPositions()
    {
        int positionsCount = smoother.Line.positionCount;
        int requiredCurveCount = Mathf.Max(positionsCount - 1, 0);
        if (curves == null || curves.Length != requiredCurveCount)
        {
            curves = new BezierCurve[requiredCurveCount];
            for (int i = 0; i < curves.Length; i++)
            {
                curves[i] = new BezierCurve();
            }
        }
    }
}
