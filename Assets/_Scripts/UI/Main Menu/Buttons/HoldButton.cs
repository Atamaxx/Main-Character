using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class HoldButton : Button, IHoldableButton
{
    [Header("Hold Settings")]
    [SerializeField] private float _initialHoldDelay = 0.5f;
    [SerializeField] private float _holdRepeatRate = 0.1f;
    [SerializeField] private bool _enableAcceleration = false;
    [SerializeField] private float _accelerationStartTime = 1.5f;
    [SerializeField] private float _maxAccelerationMultiplier = 3.0f;
    [SerializeField] private float _accelerationRampTime = 2.0f;

    [Header("Visual Feedback")]
    [SerializeField] private float _holdScale = 0.9f;
    [SerializeField] private float _scaleTweenDuration = 0.1f;

    [Header("Hold Events")]
    [SerializeField] private UnityEvent _onHoldTick;
    [SerializeField] private UnityEvent _onReleaseEvent;

    private bool _isHeld = false;
    private float _holdTime = 0f;
    private float _lastHoldTickTime = 0f;
    private float _currentAccelerationMultiplier = 1f;

    // Tween cache for hold scale animation
    private Tween _holdTween;

    public bool IsHeld => _isHeld;

    private void Update()
    {
        if (_isHeld && _interactable)
        {
            // Use unscaledDeltaTime so that the hold timer works even when Time.timeScale is 0.
            _holdTime += Time.unscaledDeltaTime;

            if (_enableAcceleration && _holdTime >= _accelerationStartTime)
            {
                float progress = Mathf.Clamp01((_holdTime - _accelerationStartTime) / _accelerationRampTime);
                _currentAccelerationMultiplier = Mathf.Lerp(1f, _maxAccelerationMultiplier, progress);
            }
            else
            {
                _currentAccelerationMultiplier = 1f;
            }

            // Use unscaledTime for comparing time values
            if (_holdTime >= _initialHoldDelay && Time.unscaledTime >= _lastHoldTickTime + _holdRepeatRate / _currentAccelerationMultiplier)
            {
                _lastHoldTickTime = Time.unscaledTime;
                _onHoldTick?.Invoke();
            }
        }
    }

    public override void OnClick()
    {
        base.OnClick();
        StartHold();
    }

    private void StartHold()
    {
        _isHeld = true;
        _holdTime = 0f;
        _lastHoldTickTime = Time.unscaledTime;
        _currentAccelerationMultiplier = 1f;

        // Kill any existing tween and start a new scale tween for hold state
        _holdTween?.Kill();
        _holdTween = transform.DOScale(new Vector3(_holdScale, _holdScale, _holdScale), _scaleTweenDuration).SetUpdate(true);
    }

    public void OnRelease()
    {
        _isHeld = false;
        _holdTime = 0f;
        _currentAccelerationMultiplier = 1f;

        // Kill the current tween and animate back to default scale (assumed Vector3.one)
        _holdTween?.Kill();
        _holdTween = transform.DOScale(Vector3.one, _scaleTweenDuration).SetUpdate(true);

        _onReleaseEvent?.Invoke();
    }

    protected override void UpdateVisuals()
    {
        if (this == null || transform == null)
            return;
        float targetScale = 1f;
        switch (_currentState)
        {
            case ButtonState.Normal:
                targetScale = 1f;
                break;
            case ButtonState.Selected:
                targetScale = 1.1f;
                break;
            case ButtonState.Pressed:
                targetScale = _holdScale;
                break;
            case ButtonState.Disabled:
                targetScale = 0.95f;
                break;
        }
        _holdTween?.Kill();
        _holdTween = transform.DOScale(new Vector3(targetScale, targetScale, targetScale), _scaleTweenDuration).SetUpdate(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // Clean up tween when the object is disabled
        _holdTween?.Kill();
    }
}
