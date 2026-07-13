using System.Collections;
using UnityEngine;

public class OffsetMethods : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _duration = 0.5f;

    private Vector2 _originalPosition;
    private Coroutine _currentCoroutine;

    private void Start()
    {
        if (_targetTransform == null)
        {
            _targetTransform = transform;
        }
        _originalPosition = _targetTransform.localPosition;
    }

    public void OffsetTransform()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _targetTransform.position = _originalPosition + _offset;
    }
    public void OffsetTransformSmooth()
    {
        StartSmoothAnimation(_originalPosition + _offset);
    }
    public void ResetTransform()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _targetTransform.position = _originalPosition;
    }
    public void ResetTransformSmooth()
    {
        StartSmoothAnimation(_originalPosition);
    }

    private void StartSmoothAnimation(Vector2 targetPosition)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _currentCoroutine = StartCoroutine(AnimateTransformOffset(targetPosition));
    }

    private IEnumerator AnimateTransformOffset(Vector2 targetPos)
    {
        Vector2 startPos = _targetTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _targetTransform.localPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        _targetTransform.localPosition = targetPos;
    }
}
