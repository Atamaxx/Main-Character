using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
public class TimeRotation : MonoBehaviour, ITimelineTask
{

    [SerializeField, BoxGroup("SETTINGS")] private bool _pause = false;
    [SerializeField, BoxGroup("SETTINGS")] private bool _useRB = false;
    [SerializeField, BoxGroup("SETTINGS")] private bool _isClockwise = false;

    [SerializeField, BoxGroup("+T")] private float _timeBC = 0f;
    [SerializeField, BoxGroup("+T")] private float _timeTC = 10f;
    [SerializeField, BoxGroup("+T")] private float _rotateMin = 0f;
    [SerializeField, BoxGroup("+T")] private float _rotateMax = 180f;
    [SerializeField, BoxGroup("PATH")] private bool _usePath = false;
    [SerializeField, BoxGroup("PATH"), ShowIf("_usePath")] private List<Vector4> _paths = new();

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
            currentTime = RotateConstant();
        }

        if (_usePath)
        {
            RotateByPath(currentTime);
        }
        else
        {
            Rotate(currentTime);
        }

    }

    private void OnStopped()
    {
        _pause = true;
    }

    private void OnResumed()
    {
        _pause = false;
    }
    #endregion

    #region ROTATE
    void Rotate(float currentTime)
    {
        float rotationValue;
        float distanceValue = Mathf.Clamp(currentTime, _timeBC, _timeTC);
        rotationValue = Mathf.Lerp(_rotateMin, _rotateMax, (distanceValue - _timeBC) / (_timeTC - _timeBC));
        SetRotation(0f, 0f, rotationValue);
    }


    private float _lastTimeDistance;
    private void RotateByPath(float timeDistance)
    {
        if (_lastTimeDistance == timeDistance)
        {
            return;
        }


        for (int i = 0; i < _paths.Count; i++)
        {
            if (_paths[i].x > timeDistance || _paths[i].y < timeDistance)
            {
                continue;
            }

            _lastTimeDistance = timeDistance;
            float distanceValue = Mathf.Clamp(timeDistance, _paths[i].x, _paths[i].y);
            float rotationValue = Mathf.Lerp(_paths[i].z, _paths[i].w, (distanceValue - _paths[i].x) / (_paths[i].y - _paths[i].x));
            SetRotation(0f, 0f, rotationValue);

            return;
        }


        for (int i = 0; i < _paths.Count; i++)
        {
            if (_lastTimeDistance < _paths[i].x && _paths[i].x < timeDistance ||
                    timeDistance < _paths[i].x && _paths[i].x < _lastTimeDistance)
            {
                SetRotation(0f, 0f, _paths[i].z);
                _lastTimeDistance = timeDistance;
            }
            else if (_lastTimeDistance < _paths[i].y && _paths[i].y < timeDistance ||
                    timeDistance < _paths[i].y && _paths[i].y < _lastTimeDistance)
            {
                SetRotation(0f, 0f, _paths[i].w);
                _lastTimeDistance = timeDistance;
            }
        }
    }


    private float constStart;
    private float constEnd;
    private float currentDistance;
    private float RotateConstant()
    {
        float t = Time.deltaTime * _constantSpeed;
        currentDistance += t;
        if (_infiniteMovement && currentDistance > constEnd)
            currentDistance = constStart;

        currentDistance = Mathf.Clamp(currentDistance, constStart, constEnd);
        return currentDistance;
    }

    private void SetRotation(float rotationX, float rotationY, float rotationZ)
    {
        Vector3 targetRotation;

        targetRotation = new(rotationX, rotationY, rotationZ);
        if (_isClockwise)
            targetRotation = -targetRotation;

        if (_useRB)
        {
            _rigidbody.MoveRotation(targetRotation.z);
            return;
        }

        transform.rotation = Quaternion.Euler(targetRotation);
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
            constStart = _paths[0].x;
            constEnd = _paths[^1].y;
        }
        else
        {
            constStart = _timeBC;
            constEnd = _timeTC;
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
}

