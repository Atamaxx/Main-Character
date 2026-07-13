using DG.Tweening;
using TMPro;
using UnityEngine;
using NaughtyAttributes;

public class TextButton : Button
{
    [BoxGroup("References")]
    [Required("Button must have a TextMeshPro component")]
    [SerializeField] private TextMeshPro _buttonText;

    [BoxGroup("References")]
    [SerializeField] private ButtonProperties _buttonStyle;

    // Store the original text so it can be restored
    public string OriginalText;

    // Duration for the tween animations in seconds
    [SerializeField, Tooltip("Duration for tween animations in seconds")]
    private float _tweenDuration = 0.2f;

    // Cache tween references
    private Tween _colorTween;
    private Tween _scaleTween;

    protected override void Awake()
    {
        base.Awake();
        if (_buttonText != null && string.IsNullOrEmpty(OriginalText))
        {
            OriginalText = _buttonText.text;
        }
    }

    // Override the abstract UpdateVisuals method to use tween caching
    protected override void UpdateVisuals()
    {
        if (_buttonText == null)
            return;

        // Determine target color and scale based on current state
        Color targetColor = Color.white;
        float targetScale = 1.0f;

        if (_buttonStyle != null)
        {
            switch (_currentState)
            {
                case ButtonState.Normal:
                    targetColor = _buttonStyle.NormalColor;
                    targetScale = _buttonStyle.NormalScale;
                    break;
                case ButtonState.Selected:
                    targetColor = _buttonStyle.SelectedColor;
                    targetScale = _buttonStyle.SelectedScale;
                    break;
                case ButtonState.Pressed:
                    targetColor = _buttonStyle.PressedColor;
                    targetScale = _buttonStyle.PressedScale;
                    break;
                case ButtonState.Disabled:
                    targetColor = _buttonStyle.DisabledColor;
                    targetScale = _buttonStyle.NormalScale;
                    break;
            }
        }

        // Kill any existing tweens to avoid overlapping animations
        _colorTween?.Kill();
        _scaleTween?.Kill();

        // Start new tweens and cache them, setting them to update using unscaled time
        _colorTween = _buttonText.DOColor(targetColor, _tweenDuration).SetUpdate(true);
        _scaleTween = transform.DOScale(new Vector3(targetScale, targetScale, targetScale), _tweenDuration).SetUpdate(true);

        // Update the text based on state
        if (_currentState == ButtonState.Selected || _currentState == ButtonState.Pressed)
        {
            _buttonText.text = OriginalText + ".";
        }
        else
        {
            _buttonText.text = OriginalText;
        }
    }

    // Kill tweens when the object is disabled
    protected override void OnDisable()
    {
        base.OnDisable();
        _colorTween?.Kill();
        _scaleTween?.Kill();
    }
}
