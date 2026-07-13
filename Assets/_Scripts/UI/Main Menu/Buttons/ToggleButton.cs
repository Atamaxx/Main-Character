using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class ToggleButton : Button
{
    [BoxGroup("Toggle Settings")]
    [SerializeField, Tooltip("The initial toggle state of the button")]
    private bool _isActive = false;

    [BoxGroup("Toggle Events")]
    [SerializeField, Tooltip("Event triggered when the button is activated")]
    private UnityEvent _onActivate;

    [BoxGroup("Toggle Events")]
    [SerializeField, Tooltip("Event triggered when the button is deactivated")]
    private UnityEvent _onDeactivate;

    [BoxGroup("Toggle Style")]
    [SerializeField, Tooltip("Style to use when the button is in active state")]
    protected ButtonProperties _activeStyle;

    [BoxGroup("Toggle Style")]
    [SerializeField, Tooltip("Style to use when the button is in inactive state")]
    protected ButtonProperties _inactiveStyle;

    [BoxGroup("Toggle Settings")]
    [SerializeField, Tooltip("Should the toggle effect be immediate or wait until button returns from pressed state")]
    private bool _toggleImmediately = true;

    [SerializeField, Tooltip("Duration for tween animations in seconds")]
    private float _tweenDuration = 0.2f;

    // Cache the tween reference for scale animations
    private Tween _scaleTween;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                UpdateToggleState(false);
            }
        }
    }

    public UnityEvent OnActivate => _onActivate;
    public UnityEvent OnDeactivate => _onDeactivate;

    protected override void Awake()
    {
        base.Awake();
        UpdateToggleState(false);
    }

    public override void OnClick()
    {
        base.OnClick();

        if (_toggleImmediately)
        {
            ToggleState();
        }
        else
        {
            Invoke(nameof(ToggleState), 0.1f);
        }
    }

    public void ToggleState()
    {
        _isActive = !_isActive;
        UpdateToggleState(true);
    }

    public void SetState(bool active)
    {
        if (_isActive != active)
        {
            _isActive = active;
            UpdateToggleState(true);
        }
    }

    protected void UpdateToggleState(bool invokeEvents)
    {
        UpdateVisuals();

        if (invokeEvents)
        {
            if (_isActive)
                _onActivate?.Invoke();
            else
                _onDeactivate?.Invoke();
        }
    }

    public override void ChangeState(ButtonState newState)
    {
        base.ChangeState(newState);
        if (newState == ButtonState.Normal || newState == ButtonState.Selected)
            UpdateVisuals();
    }

    protected override void ReturnToSelectedState()
    {
        base.ReturnToSelectedState();
        UpdateVisuals();
    }

    protected override void UpdateVisuals()
    {
        ButtonProperties styleToUse = _isActive ? _activeStyle : _inactiveStyle;
        if (styleToUse == null)
            return;

        float targetScale = 1.0f;
        switch (_currentState)
        {
            case ButtonState.Normal:
                targetScale = styleToUse.NormalScale;
                break;
            case ButtonState.Selected:
                targetScale = styleToUse.SelectedScale;
                break;
            case ButtonState.Pressed:
                targetScale = styleToUse.PressedScale;
                break;
            case ButtonState.Disabled:
                targetScale = styleToUse.NormalScale;
                break;
        }

        // Kill any active tween before starting a new one
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(new Vector3(targetScale, targetScale, targetScale), _tweenDuration).SetUpdate(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _scaleTween?.Kill();
    }
}
