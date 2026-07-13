using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Collections;
public class TimeMovement : StatefulMonoBehaviour, ITimelineTask
{

    [SerializeField, BoxGroup("SETTINGS")] private bool _pause = false;
    [SerializeField, BoxGroup("SETTINGS")] private bool _useRB = false;
    [SerializeField, BoxGroup("SETTINGS")] private WaypointsCreator _waypointsCreator;

    [SerializeField, BoxGroup("+T")] private float _timeBC = 0f;
    [SerializeField, BoxGroup("+T")] private float _timeTC = 10f;
    [SerializeField, BoxGroup("PATH")] private bool _usePath = false;
    [SerializeField, BoxGroup("PATH"), ShowIf("_usePath")] private List<Vector2> _paths = new();

    [SerializeField, BoxGroup("-T")] private bool _useConstantMove;
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantMove")] private float _constantSpeed = 1f;
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantMove")] private bool _infiniteMovement = false;

    private Rigidbody2D _rigidbody;

    #region INTERFACE
    public event Action Stopped;
    public event Action Resumed;

    public void OnUpdate(float currentTime)
    {
        if (_pause) return;

        if (_useConstantMove)
        {
            currentTime = MoveConstant();
        }

        if (_usePath)
        {
            if (_useRB)
                MovePathRB(currentTime);
            else
                MovePath(currentTime);
        }
        else
        {
            if (_useRB)
                MoveRB(currentTime);
            else
                Move(currentTime);
        }

    }

    public void OnStopped()
    {
        _pause = true;
    }

    public void OnResumed()
    {
        _pause = false;
    }
    #endregion



    #region BASE
    private void Start()
    {
        _lastTimeDistance = TimeManager.Instance.CurrentTime;

        if (_useRB)
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
        }

        if (_usePath)
        {
            _constStart = _paths[0].x;
            _constEnd = _paths[^1].y;
        }
        else
        {
            _constStart = _timeBC;
            _constEnd = _timeTC;
        }
    }
    private void Reset()
    {
        if (_useRB)
            SetupRigidbodyProperties();
    }

    private void SetupRigidbodyProperties()
    {
        Rigidbody2D _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody != null)
        {
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }
    private void OnEnable()
    {
        TimeManager.Instance.RegisterTask(this);

        Stopped += OnStopped;
        Resumed += OnResumed;
    }

    private void OnDisable()
    {
        TimeManager.Instance.UnregisterTask(this);

        Stopped -= OnStopped;
        Resumed -= OnResumed;
    }
    #endregion

    #region MOVE
    void Move(float currentTime)
    {
        if (_waypointsCreator.OutOfClamp(currentTime, _timeBC, _timeTC))
        {
            return;
        }

        float platformPercent = Mathf.Clamp((currentTime - _timeBC) / (_timeTC - _timeBC), 0, 1);

        float platformDist = platformPercent * _waypointsCreator.WayLength;
        transform.position = My.Line.FindPointByLength(_waypointsCreator.WaypointsPosition, platformDist);
    }
    void MoveRB(float currentTime)
    {
        if (_waypointsCreator.OutOfClamp(currentTime, _timeBC, _timeTC))
        {
            return;
        }

        float platformPercent = Mathf.Clamp((currentTime - _timeBC) / (_timeTC - _timeBC), 0, 1);

        float platformDist = platformPercent * _waypointsCreator.WayLength;
        Vector3 moveTo = My.Line.FindPointByLength(_waypointsCreator.WaypointsPosition, platformDist);

        if (transform.position != moveTo)
            _rigidbody.MovePosition(moveTo);
    }


    float _lastTimeDistance;
    void MovePath(float currentTime)
    {
        if (_waypointsCreator.OutOfClamp(currentTime, _paths[0].x, _paths[^1].y))
        {
            return;
        }

        for (int i = 0; i < _paths.Count; i++)
        {
            if (_paths[i].x > currentTime)
            {
                if (_lastTimeDistance > _paths[i].x)
                    transform.position = _waypointsCreator.WaypointsPosition[i];
                continue;
            }

            if (_paths[i].y < currentTime)
            {
                if (_lastTimeDistance < _paths[i].y)
                    transform.position = _waypointsCreator.WaypointsPosition[i + 1];
                continue;
            }

            float platformPercent = Mathf.Clamp((currentTime - _paths[i].x) / (_paths[i].y - _paths[i].x), 0, 1);
            float wayLength = Vector2.Distance(_waypointsCreator.WaypointsPosition[i], _waypointsCreator.WaypointsPosition[i + 1]);
            float platformDist = platformPercent * wayLength;
            List<Vector2> way = new() { _waypointsCreator.WaypointsPosition[i], _waypointsCreator.WaypointsPosition[i + 1] };

            Vector3 moveTo = My.Line.FindPointByLength(way, platformDist);
            if (transform.position != moveTo)
                transform.position = moveTo;
            break;
        }
        _lastTimeDistance = currentTime;
    }

    void MovePathRB(float currentTime)
    {
        if (_waypointsCreator.OutOfClamp(currentTime, _paths[0].x, _paths[^1].y))
        {
            return;
        }

        for (int i = 0; i < _paths.Count; i++)
        {
            if (_paths[i].x > currentTime)
            {
                if (_lastTimeDistance > _paths[i].x)
                    _rigidbody.MovePosition(_waypointsCreator.WaypointsPosition[i]);
                continue;
            }

            if (_paths[i].y < currentTime)
            {
                if (_lastTimeDistance < _paths[i].y)
                    _rigidbody.MovePosition(_waypointsCreator.WaypointsPosition[i + 1]);
                continue;
            }

            float platformPercent = Mathf.Clamp((currentTime - _paths[i].x) / (_paths[i].y - _paths[i].x), 0, 1);
            float wayLength = Vector2.Distance(_waypointsCreator.WaypointsPosition[i], _waypointsCreator.WaypointsPosition[i + 1]);
            float platformDist = platformPercent * wayLength;
            List<Vector2> way = new() { _waypointsCreator.WaypointsPosition[i], _waypointsCreator.WaypointsPosition[i + 1] };

            Vector3 moveTo = My.Line.FindPointByLength(way, platformDist);
            if (transform.position != moveTo)
                _rigidbody.MovePosition(moveTo);
            break;
        }
        _lastTimeDistance = currentTime;
    }


    private float _constStart;
    private float _constEnd;
    private float _currentDistance;
    private float MoveConstant()
    {
        float t = Time.deltaTime * _constantSpeed;
        _currentDistance += t;
        if (_infiniteMovement && _currentDistance > _constEnd)
            _currentDistance = _constStart;

        _currentDistance = Mathf.Clamp(_currentDistance, _constStart, _constEnd);
        return _currentDistance;
    }
    #endregion

    public void SetConstantSpeed(float speed)
    {
        _constantSpeed = speed;
    }

    public void SetCurrentDistance(float distance)
    {
        _currentDistance = distance;
    }

    public override object CaptureState()
    {
        TimeMovementState state = new()
        {
            CurrentDistance = _currentDistance
        };
        return state; ;
    }

    public override void RestoreState(object state)
    {
        if (state is not TimeMovementState savedState)
        {
            Debug.LogWarning("Invalid state object for TimeMovement on " + gameObject.name);
            return;
        }

        _currentDistance = savedState.CurrentDistance;

        if (_useRB)
        {
            _useRB = false;
            StartCoroutine(DoAfterFrame());
        }
    }

    IEnumerator DoAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        _useRB = true;
    }
}
[Serializable]
public class TimeMovementState
{
    public float CurrentDistance;
}
