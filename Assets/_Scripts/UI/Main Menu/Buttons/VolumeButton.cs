using TMPro;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Defines available volume control types
/// </summary>
public enum VolumeType
{
    Master,
    Music,
    Ambience,
    SoundEffects
}

/// <summary>
/// Defines the direction of volume adjustment
/// </summary>
public enum VolumeChangeDirection
{
    Increase,
    Decrease
}

/// <summary>
/// Handles volume control buttons with hold functionality and acceleration
/// </summary>
public class VolumeButton : Button, IHoldableButton
{
    #region Serialized Fields

    [BoxGroup("Volume Settings")]
    [SerializeField] private VolumeType _volumeType;

    [BoxGroup("Volume Settings")]
    [SerializeField] private VolumeChangeDirection _direction;

    [BoxGroup("Volume Settings")]
    [Range(0.01f, 0.25f)]
    [SerializeField] private float _stepSize = 0.05f; // 5% increments

    [BoxGroup("Hold Settings")]
    [SerializeField] private bool _enableHold = true;

    [BoxGroup("Hold Settings")]
    [SerializeField] private float _initialHoldDelay = 0.5f; // Time before holding starts working

    [BoxGroup("Hold Settings")]
    [SerializeField] private float _holdRepeatRate = 0.1f; // How frequently to apply change while holding

    [BoxGroup("Acceleration Settings")]
    [SerializeField] private bool _enableAcceleration = true;

    [BoxGroup("Acceleration Settings")]
    [SerializeField] private float _accelerationStartTime = 1.5f; // Time before acceleration kicks in

    [BoxGroup("Acceleration Settings")]
    [SerializeField] private float _maxAccelerationMultiplier = 5.0f; // Maximum speed multiplier

    [BoxGroup("Acceleration Settings")]
    [SerializeField] private float _accelerationRampTime = 3.0f; // Time to reach max acceleration

    [BoxGroup("References")]
    [SerializeField] private TextMeshPro _percentageText;

    [BoxGroup("References")]
    [SerializeField] private ButtonProperties _buttonStyle;

    [SerializeField] private bool _debugLogging = false;

    #endregion

    #region Private Fields

    private AudioSystem _audioSystem;
    private bool _isHeld = false;
    private float _holdTime = 0f;
    private float _lastHoldActionTime = 0f;
    private float _currentAccelerationMultiplier = 1.0f;

    #endregion

    #region Properties

    public bool IsHeld => _isHeld;

    #endregion

    #region Unity Lifecycle Methods

    protected override void Awake()
    {
        base.Awake();
        _audioSystem = AudioSystem.Instance;


    }

    protected override void Start()
    {
        base.Start();

    }

    private void Update()
    {
        ProcessHoldLogic();
    }

    protected override void OnDisable()
    {
        OnRelease();
        base.OnDisable();
    }

    #endregion

    #region Button Interaction Methods

    public override void OnClick()
    {
        LogDebug($"OnClick for {gameObject.name}");

        if (_currentState != ButtonState.Pressed)
        {
            ChangeState(ButtonState.Pressed);
            _onButtonPressed?.Invoke();
        }

        ChangeVolume();

        if (_enableHold)
        {
            StartHold();
        }

        CancelInvoke(nameof(ReturnToSelectedState));
    }

    public void OnRelease()
    {
        LogDebug($"OnRelease for {gameObject.name}, isHeld: {_isHeld}");

        if (_isHeld)
        {
            StopHold();

            if (_currentState == ButtonState.Pressed)
            {
                CancelInvoke(nameof(ReturnToSelectedState));

                NavigationGroup group = FindNavigationGroup();

                if (group != null && (object)group.CurrentButton == this)
                {
                    ChangeState(ButtonState.Selected);
                }
                else
                {
                    ChangeState(ButtonState.Normal);
                }
            }
        }
    }

    public override void ChangeState(ButtonState newState)
    {
        LogDebug($"ChangeState from {_currentState} to {newState} for {gameObject.name}, isHeld: {_isHeld}");

        if (ShouldBlockStateChange(newState))
        {
            LogDebug($"Preventing state change while held");
            return;
        }

        base.ChangeState(newState);

        if (newState == ButtonState.Disabled)
        {
            OnRelease();
        }
    }

    #endregion

    #region Protected Methods

    protected override bool ShouldAutoReturnToSelectedState()
    {
        return false;
    }

    protected override void ReturnToSelectedState()
    {
        if (!_isHeld)
        {
            base.ReturnToSelectedState();
        }
    }

    protected override void UpdateVisuals()
    {
        if (_buttonStyle == null) return;

        Color targetColor = Color.white;
        float targetScale = 1.0f;

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

        transform.localScale = new Vector3(targetScale, targetScale, targetScale);

        TextMeshPro buttonText = GetComponentInChildren<TextMeshPro>();
        if (buttonText != null)
        {
            buttonText.color = targetColor;
        }
    }

    #endregion

    #region Private Methods

    private void ProcessHoldLogic()
    {
        if (!(_enableHold && _isHeld && _interactable)) return;

        // Use unscaledDeltaTime so that hold logic works even when Time.timeScale is 0.
        _holdTime += Time.unscaledDeltaTime;

        UpdateAcceleration();

        if (_holdTime >= _initialHoldDelay)
        {
            float adjustedRepeatRate = _holdRepeatRate / _currentAccelerationMultiplier;

            // Use unscaledTime to compare time values independently of Time.timeScale.
            if (Time.unscaledTime >= _lastHoldActionTime + adjustedRepeatRate)
            {
                LogDebug($"Applying hold volume change at rate: {adjustedRepeatRate}");
                ChangeVolume();
                _lastHoldActionTime = Time.unscaledTime;
            }
        }
    }

    private void UpdateAcceleration()
    {
        if (_enableAcceleration && _holdTime >= _accelerationStartTime)
        {
            float accelerationProgress = Mathf.Clamp01((_holdTime - _accelerationStartTime) / _accelerationRampTime);
            _currentAccelerationMultiplier = Mathf.Lerp(1.0f, _maxAccelerationMultiplier, accelerationProgress);

            if (_debugLogging && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[VolumeButton] Acceleration multiplier: {_currentAccelerationMultiplier}");
            }
        }
        else
        {
            _currentAccelerationMultiplier = 1.0f;
        }
    }

    private void StartHold()
    {
        _isHeld = true;
        _holdTime = 0f;
        // Use unscaledTime here as well.
        _lastHoldActionTime = Time.unscaledTime;
        _currentAccelerationMultiplier = 1.0f;

        LogDebug($"Hold started for {gameObject.name}");
    }

    private void StopHold()
    {
        _isHeld = false;
        _holdTime = 0f;
        _currentAccelerationMultiplier = 1.0f;
    }

    private bool ShouldBlockStateChange(ButtonState newState)
    {
        return _isHeld &&
               _currentState == ButtonState.Pressed &&
               newState != ButtonState.Disabled &&
               newState != ButtonState.Pressed;
    }

    private NavigationGroup FindNavigationGroup()
    {
        foreach (var group in MenuController.Instance.GetAllGroups())
        {
            if (group.GroupButtons.Contains(this))
            {
                return group;
            }
        }
        return null;
    }

    private void ChangeVolume()
    {
        float currentVolume = GetCurrentVolume();
        float change = _stepSize * _currentAccelerationMultiplier;
        float newVolume = CalculateNewVolume(currentVolume, change);

        if (newVolume == currentVolume) return;

        LogDebug($"Changing volume from {currentVolume} to {newVolume} with multiplier: {_currentAccelerationMultiplier}");

        SetVolume(newVolume);
        UpdatePercentageText();

        if (_holdTime > _initialHoldDelay)
        {
            PlayHoldTickSound();
        }
    }

    private float CalculateNewVolume(float currentVolume, float change)
    {
        float newVolume = _direction == VolumeChangeDirection.Increase ?
            currentVolume + change :
            currentVolume - change;

        return Mathf.Clamp01(newVolume);
    }

    private void PlayHoldTickSound()
    {
        if (FMODEvents.Instance != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.Instance.ButtonSelectSFX);
        }
    }

    private float GetCurrentVolume()
    {
        if (_audioSystem == null)
        {
            Debug.LogWarning("[VolumeButton] AudioSystem is null!");
            return 0f;
        }

        switch (_volumeType)
        {
            case VolumeType.Master: return _audioSystem.MasterVolume;
            case VolumeType.Music: return _audioSystem.MusicVolume;
            case VolumeType.Ambience: return _audioSystem.AmbienceVolume;
            case VolumeType.SoundEffects: return _audioSystem.SFXVolume;
            default: return 0f;
        }
    }

    private void SetVolume(float volume)
    {
        if (_audioSystem == null)
        {
            Debug.LogWarning("[VolumeButton] AudioSystem is null!");
            return;
        }

        switch (_volumeType)
        {
            case VolumeType.Master:
                _audioSystem.SetMasterVolume(volume);
                break;
            case VolumeType.Music:
                _audioSystem.SetMusicVolume(volume);
                break;
            case VolumeType.Ambience:
                _audioSystem.SetAmbienceVolume(volume);
                break;
            case VolumeType.SoundEffects:
                _audioSystem.SetSFXVolume(volume);
                break;
        }
    }

    /// <summary>
    /// Updates the percentage text display to show current volume value
    /// </summary>
    private void UpdatePercentageText()
    {
        if (_percentageText != null)
        {
            int percentage = Mathf.RoundToInt(GetCurrentVolume() * 100);
            _percentageText.text = $"{percentage}%";
        }
    }

    /// <summary>
    /// Public method to refresh the volume display. Can be called from external systems.
    /// </summary>
    public void RefreshVolumeDisplay()
    {
        UpdatePercentageText();
    }

    private void LogDebug(string message)
    {
        if (_debugLogging)
        {
            Debug.Log($"[VolumeButton] {message}");
        }
    }

    #endregion
}
