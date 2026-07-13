using System;
using UnityEngine;
using NaughtyAttributes;

public class TimeMoveDirection : MonoBehaviour, ITimelineTask
{
    [SerializeField, BoxGroup("SETTINGS")] private bool _pause = false;
    [SerializeField, BoxGroup("SETTINGS")] private bool _useRB = false;
    [SerializeField, BoxGroup("DIRECTION")] private Vector2 _movementDirection = Vector2.right;
    [SerializeField, BoxGroup("SPEED")] private float _movementSpeed = 1f;
    [SerializeField, BoxGroup("MOVEMENT") ] private bool _infiniteMovement = false;
    
    private Rigidbody2D _rigidbody;
    private Vector3 _startPosition;
    
    public event Action Stopped;
    public event Action Resumed;

    private void Start()
    {
        _startPosition = transform.position;

        if (_useRB)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            SetupRigidbodyProperties();
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

    public void OnUpdate(float currentTime)
    {
        if (_pause) return;
        Move();
    }

    public void OnStopped()
    {
        _pause = true;
    }

    public void OnResumed()
    {
        _pause = false;
    }

    private void Move()
    {
        Vector3 movement = _movementDirection.normalized * _movementSpeed * Time.deltaTime;
        
        if (_useRB)
        {
            _rigidbody.MovePosition(transform.position + movement);
        }
        else
        {
            transform.position += movement;
        }

        if (_infiniteMovement && Vector3.Distance(transform.position, _startPosition) > 10f)
        {
            transform.position = _startPosition;
        }
    }

    private void SetupRigidbodyProperties()
    {
        if (_rigidbody != null)
        {
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void SetMovementDirection(Vector2 direction)
    {
        _movementDirection = direction;
    }

    public void SetMovementSpeed(float speed)
    {
        _movementSpeed = speed;
    }
}
