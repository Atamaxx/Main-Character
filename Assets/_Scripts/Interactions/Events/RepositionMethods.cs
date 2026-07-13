using System.Collections;
using UnityEngine;
using NaughtyAttributes;

public class RepositionMethods : MonoBehaviour
{
    [SerializeField, Required] private Transform _objectToReposition;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField, BoxGroup("BY POSITION")] private Vector3 _targetPosition;
    [SerializeField, BoxGroup("BY POSITION")] private Vector3 _targetEulerRotation;
    [SerializeField, BoxGroup("BY TRANSFORM")] private Transform _targetTransform;
    [SerializeField] private Rigidbody2D _rigidbody;

    private Coroutine _moveCoroutine;

    // Store the initial transform properties
    private Vector3 _initialPosition;
    private Vector3 _initialEulerRotation;

    void Reset()
    {
        if(_objectToReposition == null)
        {
            _objectToReposition = transform;
        }
    }
    private void Start()
    {
        if (_objectToReposition == null)
        {
            _objectToReposition = transform;
        }
        // Capture the initial position and rotation (Euler angles)
        _initialPosition = _objectToReposition.position;
        _initialEulerRotation = _objectToReposition.eulerAngles;
    }

    // Reposition Methods

    public void RepositionSlowlyTrans()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveToTransform());
    }

    public void RepositionSlowlyTrans_rigidbody()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveToTransform_rigidbody());
    }

    public void RepositionImmediatelyTrans()
    {
        _objectToReposition.position = _targetTransform.position;
        _objectToReposition.eulerAngles = _targetTransform.eulerAngles;
    }

    public void RepositionSlowly()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveToTarget());
    }

    public void RepositionImmediately()
    {
        _objectToReposition.position = _targetPosition;
        _objectToReposition.eulerAngles = _targetEulerRotation;
    }

    // Reset Methods

    public void ResetImmediately()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _objectToReposition.position = _initialPosition;
        _objectToReposition.eulerAngles = _initialEulerRotation;
    }

    public void ResetSlowly()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveToInitial());
    }

    public void ResetSlowlyRigidbody()
    {
        if (_rigidbody == null)
        {
            Debug.LogWarning("No Rigidbody2D assigned; using standard ResetSlowly instead.");
            ResetSlowly();
            return;
        }
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveToInitial_rigidbody());
    }

    // Helper method to compare Euler angles with a threshold using DeltaAngle
    private bool AreAnglesClose(Vector3 current, Vector3 target, float threshold)
    {
        return Mathf.Abs(Mathf.DeltaAngle(current.x, target.x)) < threshold &&
               Mathf.Abs(Mathf.DeltaAngle(current.y, target.y)) < threshold &&
               Mathf.Abs(Mathf.DeltaAngle(current.z, target.z)) < threshold;
    }

    // Coroutine for moving to a target position and Euler rotation
    private IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(_objectToReposition.position, _targetPosition) > 0.01f ||
               !AreAnglesClose(_objectToReposition.eulerAngles, _targetEulerRotation, 0.1f))
        {
            _objectToReposition.position = Vector3.Lerp(_objectToReposition.position, _targetPosition, _moveSpeed * Time.deltaTime);
            _objectToReposition.eulerAngles = new Vector3(
                Mathf.LerpAngle(_objectToReposition.eulerAngles.x, _targetEulerRotation.x, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.y, _targetEulerRotation.y, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.z, _targetEulerRotation.z, _rotationSpeed * Time.deltaTime)
            );
            yield return null;
        }
        _objectToReposition.position = _targetPosition;
        _objectToReposition.eulerAngles = _targetEulerRotation;
    }

    // Coroutine for moving to the transform's position and Euler rotation
    private IEnumerator MoveToTransform()
    {
        while (Vector3.Distance(_objectToReposition.position, _targetTransform.position) > 0.01f ||
               !AreAnglesClose(_objectToReposition.eulerAngles, _targetTransform.eulerAngles, 0.1f))
        {
            _objectToReposition.position = Vector3.Lerp(_objectToReposition.position, _targetTransform.position, _moveSpeed * Time.deltaTime);
            _objectToReposition.eulerAngles = new Vector3(
                Mathf.LerpAngle(_objectToReposition.eulerAngles.x, _targetTransform.eulerAngles.x, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.y, _targetTransform.eulerAngles.y, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.z, _targetTransform.eulerAngles.z, _rotationSpeed * Time.deltaTime)
            );
            yield return null;
        }
        _objectToReposition.position = _targetTransform.position;
        _objectToReposition.eulerAngles = _targetTransform.eulerAngles;
    }

    // Coroutine for moving back to the initial position and Euler rotation
    private IEnumerator MoveToInitial()
    {
        while (Vector3.Distance(_objectToReposition.position, _initialPosition) > 0.01f ||
               !AreAnglesClose(_objectToReposition.eulerAngles, _initialEulerRotation, 0.1f))
        {
            _objectToReposition.position = Vector3.Lerp(_objectToReposition.position, _initialPosition, _moveSpeed * Time.deltaTime);
            _objectToReposition.eulerAngles = new Vector3(
                Mathf.LerpAngle(_objectToReposition.eulerAngles.x, _initialEulerRotation.x, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.y, _initialEulerRotation.y, _rotationSpeed * Time.deltaTime),
                Mathf.LerpAngle(_objectToReposition.eulerAngles.z, _initialEulerRotation.z, _rotationSpeed * Time.deltaTime)
            );
            yield return null;
        }
        _objectToReposition.position = _initialPosition;
        _objectToReposition.eulerAngles = _initialEulerRotation;
    }

    // Rigidbody2D-based coroutine for moving to the transform's position and Euler rotation
    private IEnumerator MoveToTransform_rigidbody()
    {
        while (Vector3.Distance(_rigidbody.position, _targetTransform.position) > 0.01f ||
               Mathf.Abs(Mathf.DeltaAngle(_rigidbody.rotation, _targetTransform.eulerAngles.z)) > 0.1f)
        {
            Vector2 newPosition = Vector2.Lerp(_rigidbody.position, _targetTransform.position, _moveSpeed * Time.deltaTime);
            float newRotation = Mathf.LerpAngle(_rigidbody.rotation, _targetTransform.eulerAngles.z, _rotationSpeed * Time.deltaTime);
            _rigidbody.MovePosition(newPosition);
            _rigidbody.MoveRotation(newRotation);
            yield return null;
        }
        _rigidbody.position = _targetTransform.position;
        _rigidbody.rotation = _targetTransform.eulerAngles.z;
    }

    // Rigidbody2D-based coroutine for moving back to the initial position and Euler rotation
    private IEnumerator MoveToInitial_rigidbody()
    {
        while (Vector3.Distance(_rigidbody.position, _initialPosition) > 0.01f ||
               Mathf.Abs(Mathf.DeltaAngle(_rigidbody.rotation, _initialEulerRotation.z)) > 0.1f)
        {
            Vector2 newPosition = Vector2.Lerp(_rigidbody.position, _initialPosition, _moveSpeed * Time.deltaTime);
            float newRotation = Mathf.LerpAngle(_rigidbody.rotation, _initialEulerRotation.z, _rotationSpeed * Time.deltaTime);
            _rigidbody.MovePosition(newPosition);
            _rigidbody.MoveRotation(newRotation);
            yield return null;
        }
        _rigidbody.position = _initialPosition;
        _rigidbody.rotation = _initialEulerRotation.z;
    }
}
