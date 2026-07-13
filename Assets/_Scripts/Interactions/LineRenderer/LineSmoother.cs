using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineSmoother : MonoBehaviour
{
    [Tooltip("The LineRenderer to smooth.")]
    public LineRenderer Line;

    [Tooltip("Backup of the initial state (positions) of the LineRenderer.")]
    public Vector3[] InitialState = new Vector3[1];

    [Tooltip("Factor for computing tangents (0 gives a polyline).")]
    public float SmoothingLength = 2f;

    [Tooltip("Total number of points along the smoothed line.")]
    public int TotalPoints = 10;
}
