using NaughtyAttributes;
using UnityEngine;

public class LineMimic : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField]
    private LineRenderer _lineToMimic; // The original line to copy from

    [SerializeField]
    private LineRenderer _line; // The line that will be built or cleared

    [Header("Mimic Settings")]
    [Tooltip("x = start index, y = end index (inclusive) of points to mimic.")]
    [SerializeField]
    private Vector2 _positionsToMimicRange;

    [SerializeField]
    private float _mimicDuration = 5f; // Total time to mimic (or un-mimic) the points

    // Internal state variables
    private int startIndex;
    private int endIndex;
    private int totalPoints;

    /// <summary>
    /// Normalized progress of the mimic process. 0 means no points; 1 means all points.
    /// </summary>
    private float progress = 0f;

    /// <summary>
    /// Direction of the process: 1 for forward (adding points), -1 for reverse (removing points).
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// When true, the mimic process is active. (Note: if the component is disabled, Update won’t run.)
    /// </summary>
    private bool isMimicking = false;

    private void Start()
    {
        // Determine the indices to copy from _lineToMimic based on the range.
        startIndex = Mathf.RoundToInt(_positionsToMimicRange.x);
        endIndex = Mathf.RoundToInt(_positionsToMimicRange.y);

        // Clamp indices to be within the available points of _lineToMimic.
        startIndex = Mathf.Clamp(startIndex, 0, _lineToMimic.positionCount - 1);
        endIndex = Mathf.Clamp(endIndex, 0, _lineToMimic.positionCount - 1);

        // Ensure startIndex is not greater than endIndex.
        if (startIndex > endIndex)
        {
            int temp = startIndex;
            startIndex = endIndex;
            endIndex = temp;
        }

        // Calculate the total number of points that will be copied.
        totalPoints = endIndex - startIndex + 1;

        // Clear the target line.
        _line.positionCount = 0;

        // Initialize progress (0 means empty) and direction (1 means start filling).
        progress = 0f;
    }

    private void Update()
    {
        // When the component is disabled, Update won't run so progress is effectively paused.
        if (!isMimicking)
            return;

        // Update progress based on time and direction.
        progress += direction * (Time.deltaTime / _mimicDuration);
        progress = Mathf.Clamp01(progress); // Ensure progress stays between 0 and 1.

        // Determine how many points to show based on progress.
        int currentPointCount = Mathf.RoundToInt(totalPoints * progress);
        currentPointCount = Mathf.Clamp(currentPointCount, 0, totalPoints);

        // Update the _line renderer to display the current set of points.
        _line.positionCount = currentPointCount;
        for (int i = 0; i < currentPointCount; i++)
        {
            _line.SetPosition(i, _lineToMimic.GetPosition(startIndex + i));
        }

        // If the process has reached an end state (fully filled or fully cleared), stop updating.
        if ((progress == 1f && direction == 1) || (progress == 0f && direction == -1))
        {
            isMimicking = false;
        }
    }

    // Called when the component is disabled.
    // (No special code is needed here since Update naturally stops running,
    //  pausing the mimic process.)
    private void OnDisable()
    {
        isMimicking = false;
    }

    // Called when the component is enabled.
    // The mimic process will resume in Update() from its last progress value.
    private void OnEnable()
    {
        // Optionally, if you want the process to resume automatically,
        // you can set isMimicking = true here if progress is between 0 and 1.
        if (progress > 0f && progress < 1f)
        {
            isMimicking = true;
        }
    }

    /// <summary>
    /// Starts (or resumes) the mimic process in the forward direction (filling _line).
    /// </summary>
    [Button]
    public void StartMimicForward()
    {
        direction = 1;
        isMimicking = true;
    }

    /// <summary>
    /// Starts (or resumes) the mimic process in reverse (clearing _line to return to its original empty state).
    /// </summary>
    [Button]
    public void StartMimicReverse()
    {
        direction = -1;
        isMimicking = true;
    }

    public void StopMimic()
    {
        isMimicking = false;
    }

    /// <summary>
    /// Toggle the mimic direction. For example, call this to reverse the process mid-way.
    /// </summary>
    public void ToggleMimicDirection()
    {
        direction *= -1;
        isMimicking = true;
    }

    public void SetMimicDuration(float duration)
    {
        _mimicDuration = duration;
    }
}
