using System.Collections;
using UnityEngine;
using NaughtyAttributes;

public class ResizeMethods : MonoBehaviour
{
    [SerializeField] private float _resizeSpeed = 5f;

    [SerializeField, BoxGroup("BY SCALE")]
    private Vector3 _targetScale;

    [SerializeField, BoxGroup("BY TRANSFORM")]
    private Transform _targetTransform;

    private Vector3 _originalScale;
    private Coroutine _resizeCoroutine;

    private void Awake()
    {
        // Store the original scale when the component initializes
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// Resize slowly to the target transform's scale using interpolation.
    /// </summary>
    public void ResizeSlowlyTrans()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        _resizeCoroutine = StartCoroutine(ResizeToTransform());
    }

    /// <summary>
    /// Immediately resize to the target transform's scale.
    /// </summary>
    public void ResizeImmediatelyTrans()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        transform.localScale = _targetTransform.localScale;
    }

    /// <summary>
    /// Resize slowly to the target scale using interpolation.
    /// </summary>
    public void ResizeSlowly()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        _resizeCoroutine = StartCoroutine(ResizeToTarget());
    }

    /// <summary>
    /// Immediately resize to the target scale.
    /// </summary>
    public void ResizeImmediately()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        transform.localScale = _targetScale;
    }

    /// <summary>
    /// Reset slowly to the original scale using interpolation.
    /// </summary>
    public void ResetSlowly()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        _resizeCoroutine = StartCoroutine(ResizeToOriginal());
    }

    /// <summary>
    /// Immediately reset to the original scale.
    /// </summary>
    public void ResetImmediately()
    {
        if (_resizeCoroutine != null)
        {
            StopCoroutine(_resizeCoroutine);
        }
        transform.localScale = _originalScale;
    }

    /// <summary>
    /// Interpolates the object's scale towards the target scale.
    /// </summary>
    private IEnumerator ResizeToTarget()
    {
        while (Vector3.Distance(transform.localScale, _targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, _resizeSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = _targetScale;
    }

    /// <summary>
    /// Interpolates the object's scale towards the target transform's scale.
    /// </summary>
    private IEnumerator ResizeToTransform()
    {
        while (Vector3.Distance(transform.localScale, _targetTransform.localScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetTransform.localScale, _resizeSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = _targetTransform.localScale;
    }

    /// <summary>
    /// Interpolates the object's scale towards the original scale.
    /// </summary>
    private IEnumerator ResizeToOriginal()
    {
        while (Vector3.Distance(transform.localScale, _originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, _resizeSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = _originalScale;
    }
}