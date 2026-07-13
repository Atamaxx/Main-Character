using System.Collections.Generic;
using UnityEngine;

public class RotateObjectsAroundCenter2D : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private bool _pauseRotation = false;
    [SerializeField] private Transform _centerTransform;      // Center point of rotation.
    [SerializeField] private float _rotationSpeed = 20f;        // Rotation speed in degrees per second.
    [SerializeField] private float _fixedDistance = 5f;         // Fixed distance from center.

    [Header("Objects to Rotate")]
    [SerializeField] private List<Transform> _objectsToRotate;  // Manually assign the objects to rotate.

    [Header("Rotation Options")]
    [SerializeField] private bool _applyObjectRotation = false; // If true, update each object's rotation to face the center.

    // Store the current angle (in degrees) for each object.
    private List<float> _objectAngles = new List<float>();

    private void Start()
    {
        // Validate the center transform.
        if (_centerTransform == null)
        {
            Debug.LogError("Center transform is not assigned.");
            enabled = false;
            return;
        }

        // Validate the objects list.
        if (_objectsToRotate == null || _objectsToRotate.Count == 0)
        {
            Debug.LogWarning("No objects assigned for rotation.");
            enabled = false;
            return;
        }

        // Initialize each object's angle based on its current position relative to the center,
        // and re-position it exactly at the fixed distance.
        _objectAngles.Clear();
        foreach (Transform obj in _objectsToRotate)
        {
            if (obj == null)
            {
                _objectAngles.Add(0f);
                continue;
            }

            // Compute the offset from the center (in the X-Y plane).
            Vector2 offset = obj.position - _centerTransform.position;

            // If the object is nearly at the center, default to 0 degrees.
            float angleDeg = offset.sqrMagnitude < 0.0001f
                ? 0f
                : Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            _objectAngles.Add(angleDeg);

            // Place the object exactly at the fixed distance from the center.
            Vector2 fixedOffset = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad)) * _fixedDistance;
            obj.position = _centerTransform.position + (Vector3)fixedOffset;
        }
    }

    private void Update()
    {
        if (_pauseRotation) return;
        // For each object, update its angle and position.
        for (int i = 0; i < _objectsToRotate.Count; i++)
        {
            Transform obj = _objectsToRotate[i];
            if (obj == null)
                continue;

            // Increment the angle based on rotation speed.
            _objectAngles[i] += _rotationSpeed * Time.deltaTime;

            // Compute the new position using the updated angle.
            float angleRad = _objectAngles[i] * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * _fixedDistance;
            Vector3 newPosition = _centerTransform.position + (Vector3)offset;
            obj.position = newPosition;

            // Optionally update the object's rotation so it faces the center.
            if (_applyObjectRotation)
            {
                // In 2D, it's common to have the object's "up" vector point toward the center.
                Vector2 directionToCenter = (_centerTransform.position - newPosition).normalized;
                float rotationAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
                obj.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
            }
        }
    }

    public void SetPauseRotation(bool pause)
    {
        _pauseRotation = pause;
    }
}
