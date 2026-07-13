//TrajectoryRenderer : MonoBehaviour
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    public int NumberOfPoints = 100;
    public float TimeOfFly = 5f;
    public LayerMask LayersToCheck;

    public List<Vector3> Waypoints;// { get; private set; }

    private const float GRAVITY = 9.81f;
    //private LineRenderer _lineRenderer;
    private Vector2 _prevPosition;
    private Vector2 _currentPosition;
    public Vector2 _velocityVector;
    private float _prevTime;

    private Vector2 _topLeftCorner;
    private Vector2 _topRightCorner;
    private Vector2 _bottomLeftCorner;
    private Vector2 _bottomRightCorner;
    private BoxCollider2D _boxCollider;
    void Start()
    {
        Waypoints = new();
        _boxCollider = GetComponent<BoxCollider2D>();
        //_lineRenderer = GetComponent<LineRenderer>();
        _currentPosition = transform.position;
        _prevPosition = transform.position;
        _prevTime = Time.time;
        _topLeftCorner = transform.position + new Vector3(-_boxCollider.size.x / 2, _boxCollider.size.y / 2, 0);
        _bottomRightCorner = transform.position + new Vector3(_boxCollider.size.x / 2, -_boxCollider.size.y / 2, 0);

        _topRightCorner = transform.position + new Vector3(_boxCollider.size.x / 2, _boxCollider.size.y / 2, 0);
        _bottomLeftCorner = transform.position + new Vector3(-_boxCollider.size.x / 2, -_boxCollider.size.y / 2, 0);
    }


    // private void Update()
    // {
    //     _currentPosition = transform.position;
    //     _velocityVector = CalculateVelocity();
    // }

    public void RenderTrajectory()
    {
        _currentPosition = transform.position;
        //_lineRenderer.positionCount = 0;
        ///!!!!!!!!!!!!!_velocityVector = new(MainController.CharacterController2D.PlayerInstance._currentHorizontalSpeed, MainController.CharacterController2D.PlayerInstance._currentVerticalSpeed);

        float timeStep = TimeOfFly / NumberOfPoints;
        float currTime = 0;
        Waypoints.Clear();
        for (int i = 0; i < NumberOfPoints; i++)
        {
            Vector2 point = new(_currentPosition.x + currTime * _velocityVector.x, _currentPosition.y + currTime * _velocityVector.y - 0.5f * GRAVITY * currTime * currTime);
            _topRightCorner = point + new Vector2(_boxCollider.size.x / 2, _boxCollider.size.y / 2);
            _bottomLeftCorner = point + new Vector2(-_boxCollider.size.x / 2, -_boxCollider.size.y / 2);

            bool overlap = Physics2D.OverlapArea(_bottomLeftCorner, _topRightCorner, LayersToCheck);


            if (overlap)
            {
                Debug.DrawLine(_topRightCorner, _topRightCorner + Vector2.right, Color.red, 10);
                Debug.DrawLine(_bottomLeftCorner, _bottomLeftCorner + Vector2.left, Color.yellow, 10);
                return;
            }
            //_lineRenderer.positionCount++;
            //_lineRenderer.SetPosition(i, point);
            Waypoints.Add(point);
            currTime += timeStep;
        }
    }

    private Vector2 CalculateVelocity()
    {
        float deltaTime = Time.time - _prevTime;

        Vector2 positionChange = _currentPosition - _prevPosition;
        Vector2 velocity = positionChange / deltaTime;

        _prevPosition = _currentPosition;
        _prevTime = Time.time;

        return velocity;
    }
}

