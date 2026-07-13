using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class TimeLine : StatefulMonoBehaviour
{
    #region Serialized Fields

    [SerializeField, BoxGroup("MAIN")]
    private bool _pause = false;

    [SerializeField, BoxGroup("FILLING OPTIONS")]
    private bool _useRealTime = false;

    // Time-based filling options (shown when _useRealTime == true)
    [SerializeField, BoxGroup("FILLING OPTIONS"), ShowIf("_useRealTime")]
    private float _timeToFill = 5f;

    [SerializeField, BoxGroup("FILLING OPTIONS"), ShowIf("_useRealTime")]
    private float _fillSpeedMultiplier = 1f;

    [SerializeField, BoxGroup("FILLING OPTIONS"), ShowIf("_useRealTime")]
    private bool _useMoveForward = true;

    [SerializeField, BoxGroup("FILLING OPTIONS"), ShowIf("_useRealTime")]
    private bool _useFillWhileMove = false;

    // Fill-speed based filling option (shown when _useRealTime == false)
    [SerializeField, BoxGroup("FILLING OPTIONS"), HideIf("_useRealTime")]
    private float _fillSpeedValue = 1f;

    [SerializeField, BoxGroup("FILLING SETTINGS")]
    private bool _useObstacles = false;

    [SerializeField, BoxGroup("FILLING SETTINGS"), ShowIf("_useObstacles")]
    private LayerMask _obstaclesLayer;

    [SerializeField, BoxGroup("FILLING SETTINGS")]
    private bool _useUpdateLine = false;

    [SerializeField, BoxGroup("SERIALIZE")]
    private LineRenderer _lineRenderer;

    [SerializeField, BoxGroup("SERIALIZE")]
    private Transform _timeObject;

    [BoxGroup("RESULTS")]
    public float LineLength;

    [BoxGroup("RESULTS")]
    public float CurrentLength;

    [BoxGroup("RESULTS")]
    public float CurrentTime = 0f;

    [BoxGroup("RESULTS")]
    public float PercentPassed;

    // Collider settings
    [
        SerializeField,
        BoxGroup("COLLIDER SETTINGS"),
        Tooltip("If true, create a one-time full line collider in Start()")
    ]
    private bool generateFullColliderOnStart = false;

    [
        SerializeField,
        BoxGroup("COLLIDER SETTINGS"),
        Tooltip("If true, update collider each frame to match the filled portion")
    ]
    private bool generateDynamicCollider = false;

    // Timeline actions – each action now has a RepeatingTrigger option.
    [SerializeField, BoxGroup("TIMELINE ACTIONS")]
    private List<TimeLineAction> _timelineActions = new List<TimeLineAction>();

    #endregion

    #region Private Fields

    private EdgeCollider2D _edgeCollider;
    private List<Vector2> _linePoints = new();
    private Vector2 _timePoint;
    private float _initialSpeed;

    // Used for edge detection when checking timeline actions.
    private float _previousTime;

    #endregion

    #region Unity Methods
    protected override void OnStatefulAwake()
    {
        _initialSpeed = _fillSpeedValue;
    }

    private void Start()
    {
        InitializeLineData();
        UpdateTimePointAndVisuals();

        // Initialize the previous time for action checks.
        _previousTime = CurrentTime; // Or CurrentLength, depending on how actions are triggered

        // Reset each timeline action’s trigger flag.
        if (_timelineActions != null)
        {
            foreach (var action in _timelineActions)
            {
                action.HasTriggered = false;
            }
        }

        // Set up collider if needed.
        if (generateFullColliderOnStart || generateDynamicCollider)
        {
            _edgeCollider = GetComponent<EdgeCollider2D>();
            if (!_edgeCollider)
            {
                _edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            }
        }

        if (generateFullColliderOnStart)
        {
            GenerateFullCollider();
        }
        else if (generateDynamicCollider)
        {
            GeneratePartialCollider(CurrentLength);
        }
    }

    private void Update()
    {
        if (_pause)
            return;
        if (_useUpdateLine)
        {
            RefreshLineData();
        }

        if (_useRealTime)
        {
            ProcessRealTimeFilling();
        }
        else
        {
            ProcessFillSpeedFilling();
        }

        // Check for timeline action triggers.
        CheckTimelineActions();
    }

    #endregion

    #region Initialization & Data Refresh

    private void InitializeLineData()
    {
        if (!_lineRenderer)
        {
            Debug.LogError("LineRenderer is not assigned.", this);
            _pause = true; // Prevent further errors
            return;
        }
        _linePoints = My.Line.GetPoints(_lineRenderer);
        LineLength = My.Line.CalculateLength(_linePoints);
        _timePoint = My.Line.FindPointByLength(_lineRenderer, 0f); // Assumes My.Line.FindPointByLength handles LineRenderer
        if (_timeObject != null)
            _timeObject.position = _timePoint;

        // Initialize derived values.
        PercentPassed = (LineLength > 0f) ? CurrentLength / LineLength : 0f;
        CurrentTime = PercentPassed * _timeToFill;
    }

    private void RefreshLineData()
    {
        if (!_lineRenderer)
            return;
        _linePoints = My.Line.GetPoints(_lineRenderer);
        LineLength = My.Line.CalculateLength(_linePoints);
        // Potentially re-evaluate CurrentLength/CurrentTime if the line changes significantly
        // For now, just updates line data. Visuals and position will update in the next cycle.
    }

    #endregion

    #region Filling Logic

    private void ProcessRealTimeFilling()
    {
        float delta = Time.deltaTime * _fillSpeedMultiplier;
        float directionMultiplier = _useMoveForward ? 1f : -1f;
        // In real-time mode, CurrentTime drives CurrentLength
        float timeDelta = delta * directionMultiplier; // How much time to add/subtract

        CurrentTime = Mathf.Clamp(CurrentTime + timeDelta, 0f, _timeToFill);
        float t = (Mathf.Approximately(_timeToFill, 0f)) ? 0f : CurrentTime / _timeToFill;
        float idealLength = Mathf.Lerp(0f, LineLength, t);

        if (_useObstacles)
        {
            // Pass the original directionMultiplier, AdjustIdealLengthForObstacles will determine if it's unfilling
            idealLength = AdjustIdealLengthForObstacles(idealLength, directionMultiplier);
            // If idealLength was adjusted by an obstacle (meaning it wasn't unfilling and hit something),
            // or if it was unfilling (and idealLength wasn't changed by obstacles),
            // CurrentTime might need to be re-synced to the actual CurrentLength achieved.
            if (LineLength > 0)
            { // Avoid division by zero
                CurrentTime = (Mathf.Clamp(idealLength, 0f, LineLength) / LineLength) * _timeToFill;
            }
            else
            {
                CurrentTime = 0f;
            }
        }

        CurrentLength = Mathf.Clamp(idealLength, 0f, LineLength);
        UpdateTimePointAndVisuals(); // This updates _timePoint and PercentPassed based on new CurrentLength

        if (generateDynamicCollider && _edgeCollider != null)
        {
            GeneratePartialCollider(CurrentLength);
        }
    }

    private void ProcessFillSpeedFilling()
    {
        float fillIncrementBase = Time.deltaTime * _fillSpeedValue;
        float directionMultiplier = _useMoveForward ? 1f : -1f;
        float actualFillIncrement = fillIncrementBase * directionMultiplier;

        float idealLength = CurrentLength + actualFillIncrement;

        if (_useObstacles)
        {
            // Pass the original directionMultiplier
            idealLength = AdjustIdealLengthForObstacles(idealLength, directionMultiplier);
        }

        CurrentLength = Mathf.Clamp(idealLength, 0f, LineLength);
        UpdateTimePointAndVisuals(); // This updates _timePoint and PercentPassed

        // Optionally update CurrentTime for consistency if _timeToFill is meaningful
        if (_timeToFill > 0f && LineLength > 0f)
        {
            CurrentTime = PercentPassed * _timeToFill;
        }
        else if (Mathf.Approximately(LineLength, 0f))
        {
            CurrentTime = 0f;
        }
        // If _timeToFill is 0, CurrentTime might not be relevant or could be set to PercentPassed directly

        if (generateDynamicCollider && _edgeCollider != null)
        {
            GeneratePartialCollider(CurrentLength);
        }
    }

    /// <summary>
    /// Adjusts the ideal length based on obstacles. If the line is unfilling, obstacles are ignored.
    /// </summary>
    /// <param name="idealLength">The ideal new length calculated by the filling logic.</param>
    /// <param name="intendedDirectionMultiplier">Indicates the primary intended direction (1 for forward, -1 for backward based on _useMoveForward).</param>
    /// <returns>The adjusted length if an obstacle is hit while filling, otherwise the original idealLength.</returns>
    private float AdjustIdealLengthForObstacles(
        float idealLength,
        float intendedDirectionMultiplier
    )
    {
        // Calculate the actual change in length proposed before considering obstacles.
        float actualProposedChange = idealLength - CurrentLength;

        // If the line is unfilling (i.e., its length would decrease), ignore obstacles.
        if (actualProposedChange < 0f)
        {
            return idealLength; // Return the unfilling length directly.
        }

        // If there's no change or a negligible change, no need for obstacle check.
        if (Mathf.Approximately(actualProposedChange, 0f))
        {
            return idealLength;
        }

        // At this point, actualProposedChange > 0, so the line is trying to fill/extend.
        // Proceed with obstacle check only when extending.

        if (_linePoints == null || _linePoints.Count < 2)
            return idealLength; // Not enough points for a tangent or line.

        // Tangent should point along the line's growth direction.
        // My.Line.GetTangent should provide the direction from CurrentLength outwards.
        Vector2 tangentAtCurrentPoint = My.Line.GetTangent(_linePoints, CurrentLength);
        Vector2 rayDirection = tangentAtCurrentPoint; // We are extending, so ray is along the tangent.

        Vector2 rayOrigin = _timePoint;
        // checkDistance is the magnitude of the intended extension.
        float checkDistance = actualProposedChange + 0.05f; // actualProposedChange is positive here. Add a small buffer.

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            rayDirection.normalized, // Ensure normalized direction for the raycast.
            checkDistance,
            _obstaclesLayer
        );

        if (hit.collider != null)
        {
            // An obstacle was hit in the direction of extension.
            float distToObstacle = Vector2.Distance(rayOrigin, hit.point);

            // If the intended extension (actualProposedChange) is greater than the distance to the obstacle,
            // it means we would pass through or hit the obstacle.
            if (actualProposedChange > distToObstacle)
            {
                // Adjust length to stop just before the obstacle.
                float blockedLength = CurrentLength + distToObstacle;
                return Mathf.Clamp(blockedLength, 0f, LineLength); // Clamp to ensure it stays within line bounds.
            }
        }

        // No obstacle hit within the check distance while extending, or obstacle is further than intended move.
        return idealLength;
    }

    /// <summary>
    /// Updates the _timePoint and visual properties based on CurrentLength.
    /// </summary>
    private void UpdateTimePointAndVisuals()
    {
        if (!_lineRenderer)
            return;
        // My.Line.FindPointByLength might take LineRenderer or List<Vector2> depending on your implementation.
        // Assuming it can take LineRenderer and the current length:
        _timePoint = My.Line.FindPointByLength(_lineRenderer, CurrentLength);
        // If My.Line.FindPointByLength needs the points list and total length:
        // _timePoint = My.Line.FindPointByLength(_linePoints, CurrentLength, LineLength);


        if (_timeObject != null)
            _timeObject.position = _timePoint;

        if (LineLength > 0f)
        {
            PercentPassed = CurrentLength / LineLength;
        }
        else
        {
            PercentPassed = 0f;
            if (CurrentLength > 0)
                CurrentLength = 0; // Ensure consistency
        }
    }

    #endregion

    #region Collider Generation

    /// <summary>
    /// Generates a collider covering the entire line.
    /// </summary>
    private void GenerateFullCollider()
    {
        if (_edgeCollider == null || _linePoints == null || _linePoints.Count == 0)
            return;
        _edgeCollider.SetPoints(_linePoints);
    }

    /// <summary>
    /// Generates or updates a collider covering the filled portion of the line.
    /// </summary>
    /// <param name="distance">The current filled length.</param>
    private void GeneratePartialCollider(float distance)
    {
        if (_edgeCollider == null || _linePoints == null || _linePoints.Count < 1)
            return;

        if (distance <= 0f)
        {
            if (_linePoints.Count > 0)
            {
                _edgeCollider.SetPoints(new List<Vector2> { _linePoints[0], _linePoints[0] });
            }
            else
            {
                _edgeCollider.SetPoints(new List<Vector2>());
            }
            return;
        }

        if (distance >= LineLength)
        {
            _edgeCollider.SetPoints(_linePoints);
            return;
        }

        List<Vector2> partialPoints = new List<Vector2> { _linePoints[0] };
        float accumulated = 0f;

        for (int i = 1; i < _linePoints.Count; i++)
        {
            float segmentLength = Vector2.Distance(_linePoints[i - 1], _linePoints[i]);
            if (accumulated + segmentLength < distance)
            {
                partialPoints.Add(_linePoints[i]);
                accumulated += segmentLength;
            }
            else
            {
                float remaining = distance - accumulated;
                float t = (segmentLength > 0) ? (remaining / segmentLength) : 0f;
                Vector2 finalPoint = Vector2.Lerp(_linePoints[i - 1], _linePoints[i], t);
                partialPoints.Add(finalPoint);
                break;
            }
        }
        if (partialPoints.Count == 1 && distance > 0)
        {
            partialPoints.Add(partialPoints[0]);
        }
        _edgeCollider.SetPoints(partialPoints);
    }

    #endregion

    #region Timeline Actions

    /// <summary>
    /// Checks each timeline action and invokes its UnityEvent if the timeline has just passed its trigger.
    /// Supports repeating triggers.
    /// </summary>
    private void CheckTimelineActions()
    {
        if (_timelineActions == null || _timelineActions.Count == 0)
            return;

        // Determine what _previousTime and currentTimeMarker represent (e.g. CurrentLength or CurrentTime)
        // This example assumes actions are triggered based on CurrentLength
        float currentTimeMarker = CurrentLength;

        foreach (var action in _timelineActions)
        {
            // Assuming action.TriggerTime is a length value.
            // If it's a time value (0 to _timeToFill), it needs conversion:
            // float triggerPoint = (action.TriggerTime / _timeToFill) * LineLength;
            float triggerPoint = action.TriggerTime; // Use this if TriggerTime is already a length

            // Ensure triggerPoint is within valid bounds if it can be set arbitrarily
            // triggerPoint = Mathf.Clamp(triggerPoint, 0, LineLength);

            bool crossedForward = (
                _previousTime < triggerPoint && currentTimeMarker >= triggerPoint
            );
            bool crossedBackward = (
                _previousTime > triggerPoint && currentTimeMarker <= triggerPoint
            );

            if (action.RepeatingTrigger)
            {
                if (
                    (action.TriggerForward && crossedForward)
                    || (action.TriggerBackward && crossedBackward)
                )
                {
                    action.OnTrigger.Invoke();
                }
            }
            else
            {
                if (
                    !action.HasTriggered
                    && (
                        (action.TriggerForward && crossedForward)
                        || (action.TriggerBackward && crossedBackward)
                    )
                )
                {
                    action.OnTrigger.Invoke();
                    action.HasTriggered = true;
                }
            }
        }
        _previousTime = currentTimeMarker;
    }

    #endregion

    #region Public Control Methods

    public void OnStopped() => _pause = true;

    public void OnResumed() => _pause = false;

    public void SetSpeed(float newSpeed) => _fillSpeedValue = newSpeed;

    public void ResetSpeed() => _fillSpeedValue = _initialSpeed;

    public void SetMoveDirection(bool forward) => _useMoveForward = forward;

    public void ToggleMoveDirection() => _useMoveForward = !_useMoveForward;

    #endregion

    #region IStateful Implementation

    [System.Serializable]
    private struct TimeLineSaveData
    {
        public bool pause;
        public float fillSpeedMultiplier;
        public bool useMoveForward;
        public bool useFillWhileMove;
        public float fillSpeedValue;
        public float currentLength;
        public float currentTime;
        public float percentPassed;
        public float previousTime;
        public bool[] timelineActionHasTriggered;
    }

    public override object CaptureState()
    {
        TimeLineSaveData data = new TimeLineSaveData
        {
            pause = _pause,
            fillSpeedMultiplier = _fillSpeedMultiplier,
            useMoveForward = _useMoveForward,
            useFillWhileMove = _useFillWhileMove,
            fillSpeedValue = _fillSpeedValue,
            currentLength = CurrentLength,
            currentTime = CurrentTime,
            percentPassed = PercentPassed,
            previousTime = _previousTime,
            timelineActionHasTriggered = new bool[
                _timelineActions != null ? _timelineActions.Count : 0
            ],
        };

        if (_timelineActions != null)
        {
            for (int i = 0; i < _timelineActions.Count; i++)
            {
                data.timelineActionHasTriggered[i] = _timelineActions[i].HasTriggered;
            }
        }
        return data;
    }

    public override void RestoreState(object state)
    {
        if (state is TimeLineSaveData data)
        {
            _pause = data.pause;
            _fillSpeedMultiplier = data.fillSpeedMultiplier;
            _useMoveForward = data.useMoveForward;
            _useFillWhileMove = data.useFillWhileMove;
            _fillSpeedValue = data.fillSpeedValue;
            CurrentLength = data.currentLength;
            CurrentTime = data.currentTime;
            PercentPassed = data.percentPassed;
            _previousTime = data.previousTime;

            UpdateTimePointAndVisuals();

            if (
                _timelineActions != null
                && data.timelineActionHasTriggered != null
                && data.timelineActionHasTriggered.Length == _timelineActions.Count
            )
            {
                for (int i = 0; i < _timelineActions.Count; i++)
                {
                    _timelineActions[i].HasTriggered = data.timelineActionHasTriggered[i];
                }
            }
        }
        else
        {
            Debug.LogError("Failed to restore TimeLine state: invalid state object.");
        }
    }

    #endregion

    #region TimeLineAction Class

    [System.Serializable]
    public class TimeLineAction
    {
        public float TriggerTime; // Define if this is length or time (0 to _timeToFill)
        public UnityEvent OnTrigger;
        public bool RepeatingTrigger;
        public bool TriggerForward = true;
        public bool TriggerBackward = false;

        [System.NonSerialized]
        public bool HasTriggered;
    }

    #endregion
}
