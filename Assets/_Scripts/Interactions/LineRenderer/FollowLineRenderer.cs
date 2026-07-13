using System.Collections;
using My;
using UnityEngine;

public class FollowLineRenderer : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField]
    private Transform _followObject;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private float _travelTime = 5f;

    [SerializeField]
    private bool _useWorldSpace = true;

    [Header("Movement Control")]
    [SerializeField]
    private bool _paused = false;

    private Vector3[] _pathPoints;
    private float _totalPathLength = 0f;
    private Coroutine _followCoroutine;

    private void Start()
    {
        if (_lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not assigned.");
            return;
        }

        BuildPath();
    }

    /// <summary>
    /// Retrieves the points from the LineRenderer and calculates the total path length.
    /// </summary>
    private void BuildPath()
    {
        int pointCount = _lineRenderer.positionCount;
        if (pointCount < 2)
        {
            Debug.LogWarning("LineRenderer must have at least two points to form a path.");
            return;
        }

        _pathPoints = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            _pathPoints[i] = _useWorldSpace
                ? _lineRenderer.GetPosition(i)
                : _lineRenderer.transform.TransformPoint(_lineRenderer.GetPosition(i));
        }

        _totalPathLength = 0f;
        for (int i = 0; i < pointCount - 1; i++)
        {
            _totalPathLength += Vector3.Distance(_pathPoints[i], _pathPoints[i + 1]);
        }
    }

    /// <summary>
    /// Coroutine that moves the GameObject along the path over the specified travel time.
    /// </summary>
    private IEnumerator FollowPath()
    {
        int pointCount = _pathPoints.Length;
        float elapsedTime = 0f;

        while (elapsedTime < _travelTime)
        {
            if (!_paused)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / _travelTime);
                float distanceToCover = t * _totalPathLength;

                float accumulatedDistance = 0f;
                Vector3 newPosition = _pathPoints[0];
                for (int i = 0; i < pointCount - 1; i++)
                {
                    float segmentLength = Vector3.Distance(_pathPoints[i], _pathPoints[i + 1]);
                    if (accumulatedDistance + segmentLength >= distanceToCover)
                    {
                        float segmentT = (distanceToCover - accumulatedDistance) / segmentLength;
                        newPosition = Vector3.Lerp(_pathPoints[i], _pathPoints[i + 1], segmentT);
                        break;
                    }
                    accumulatedDistance += segmentLength;
                }
                _followObject.position = newPosition;
            }
            yield return null;
        }

        // Ensure the GameObject ends exactly at the final point.
        _followObject.position = _pathPoints[pointCount - 1];
    }

    /// <summary>
    /// Pauses the movement along the path.
    /// </summary>
    public void Pause()
    {
        _paused = true;
    }

    /// <summary>
    /// Resumes the movement along the path.
    /// </summary>
    public void Resume()
    {
        _paused = false;
    }

    /// <summary>
    /// Optionally restart the path following from the beginning.
    /// </summary>
    public void RestartPath()
    {
        if (_followCoroutine != null)
        {
            StopCoroutine(_followCoroutine);
        }
        if (_pathPoints != null && _pathPoints.Length > 0)
        {
            _followObject.position = _pathPoints[0];
        }
        _paused = false;
        _followCoroutine = StartCoroutine(FollowPath());
    }
}
