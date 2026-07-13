using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class TextToggleVolumeButton : ToggleButton
{

    [SerializeField] private VolumeType _volumeType;

    [BoxGroup("References")]
    [Required("Button must have a TextMeshPro component")]
    [SerializeField] private TextMeshPro _buttonText;
    [BoxGroup("Toggle Text")]
    [SerializeField] private string _inactiveText;

    // Auto-populated with GameObject name
    [SerializeField] private string _saveKey = "Toggle_";
    private AudioSystem _audioSystem;

    protected override void Awake()
    {
        // Load saved state before base.Awake() runs
        LoadState();

        base.Awake();

        _audioSystem = AudioSystem.Instance;

        if (_buttonText == null)
            _buttonText = GetComponent<TextMeshPro>();
    }

    protected override void Start()
    {
        base.Start();
        // Trigger appropriate events based on current state
        UpdateToggleState(true);

    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();

        if (_buttonText == null)
            return;

        ButtonProperties styleToUse = IsActive ? _activeStyle : _inactiveStyle;
        if (styleToUse == null)
            return;

        Color targetColor = Color.white;
        switch (_currentState)
        {
            case ButtonState.Normal: targetColor = styleToUse.NormalColor; break;
            case ButtonState.Selected: targetColor = styleToUse.SelectedColor; break;
            case ButtonState.Pressed: targetColor = styleToUse.PressedColor; break;
            case ButtonState.Disabled: targetColor = styleToUse.DisabledColor; break;
        }

        _buttonText.color = targetColor;
    }

    public override void OnClick()
    {
        base.OnClick();
        SaveState();
    }

    public void SetActiveText(TextMeshPro toggleText)
    {
        int percentage = Mathf.RoundToInt(GetCurrentVolume() * 100);
        toggleText.text = $"{percentage}%";

    }

    public void SetInactiveText(TextMeshPro toggleText)
    {
        toggleText.text = _inactiveText;
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



    public void SaveState()
    {
        PlayerPrefs.SetInt(_saveKey, IsActive ? 1 : 0);
        PlayerPrefs.SetString(_saveKey + "_inactiveText", _inactiveText);
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        if (PlayerPrefs.HasKey(_saveKey))
            IsActive = PlayerPrefs.GetInt(_saveKey) == 1;

        if (PlayerPrefs.HasKey(_saveKey + "_inactiveText"))
            _inactiveText = PlayerPrefs.GetString(_saveKey + "_inactiveText");
    }
}