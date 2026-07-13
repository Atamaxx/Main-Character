using System;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class TimeTextWriter : MonoBehaviour, ITimelineTask
{
    [BoxGroup("SETTINGS"), SerializeField] private bool _pause;
    [BoxGroup("SETTINGS"), Required, SerializeField] private TMP_Text _text;

    [BoxGroup("+T"), SerializeField] private float _timeBC = 0f; // Begin clamp
    [BoxGroup("+T"), SerializeField] private float _timeTC = 10f; // End clamp

    // If you want a constant speed approach:
    [BoxGroup("-T"), SerializeField] private bool _useConstantSpeed = false;
    [BoxGroup("-T"), ShowIf("_useConstantSpeed"), SerializeField] private float _charactersPerSecond = 5f;

    // Internal references
    private int _totalCharacters;
    private string _fullText;

    // For time-based approach
    private float _elapsed;

    // For constant movement style
    private float _currentDistance;
    private float _constStart;
    private float _constEnd;

    // Track the previously shown character count
    private int _prevCharactersToShow;
    private Vector3[] _cachedLetterPositions;
    #region INTERFACE
    public event Action Stopped;
    public event Action Resumed;

    public void OnUpdate(float currentTime)
    {
        if (_pause) return;

        if (_useConstantSpeed)
        {
            WriteTextConstant();
        }
        else
        {
            WriteTextInterpolated(currentTime);
        }
    }

    void Reset()
    {
        if(_text == null)
        {
            _text = GetComponent<TMP_Text>();
        }
    }

    public void OnStopped()
    {
        _pause = true;
    }

    public void OnResumed()
    {
        _pause = false;
    }
    #endregion

    #region WRITE TEXT
    private void WriteTextInterpolated(float currentTime)
    {
        // Clamp and normalize time.
        float clampedTime = Mathf.Clamp(currentTime, _timeBC, _timeTC);
        float t = (clampedTime - _timeBC) / (_timeTC - _timeBC);

        int newCount = Mathf.FloorToInt(t * _totalCharacters);
        newCount = Mathf.Clamp(newCount, 0, _totalCharacters);

        // If nothing changes, avoid unnecessary work.
        if(newCount == _prevCharactersToShow)
            return;

        if(newCount > _prevCharactersToShow)
        {
            // Letters are being added.
            // Update text and force mesh update so TMP can recalc positions.
            _text.text = _fullText.Substring(0, newCount);
            _text.ForceMeshUpdate();

            TMP_TextInfo textInfo = _text.textInfo;
            for (int i = _prevCharactersToShow; i < newCount; i++)
            {
                if(i < textInfo.characterCount)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    if(!char.IsWhiteSpace(charInfo.character))
                    {
                        // Calculate the character's center and convert to world space.
                        Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) / 2;
                        Vector3 worldPos = _text.transform.TransformPoint(charCenter);
                        AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.FillLetterSFX, worldPos);
                    }
                }
            }
        }
        else // newCount < _prevCharactersToShow, letters are being removed.
        {
            // Play removal SFX using cached positions from the previous frame.
            for (int i = newCount; i < _prevCharactersToShow; i++)
            {
                if(_cachedLetterPositions != null && i < _cachedLetterPositions.Length)
                {
                    AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.UnfillLetterSFX, _cachedLetterPositions[i]);
                }
                else
                {
                    // Fallback if for some reason the cache is not available.
                    AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.UnfillLetterSFX, transform.position);
                }
            }
            // Update text after processing removal SFX.
            _text.text = _fullText.Substring(0, newCount);
            _text.ForceMeshUpdate();
        }

        // Update the cached letter positions for currently visible characters.
        UpdateCachedLetterPositions(newCount);
        _prevCharactersToShow = newCount;
    }

    private void WriteTextConstant()
    {
        float delta = Time.deltaTime * _charactersPerSecond;
        _currentDistance += delta;

        if (_currentDistance > _constEnd)
            _currentDistance = _constEnd;

        float fraction = (_currentDistance - _constStart) / (_constEnd - _constStart);
        int newCount = Mathf.FloorToInt(fraction * _totalCharacters);
        newCount = Mathf.Clamp(newCount, 0, _totalCharacters);

        if(newCount == _prevCharactersToShow)
            return;

        if(newCount > _prevCharactersToShow)
        {
            // Addition: update text then get positions.
            _text.text = _fullText.Substring(0, newCount);
            _text.ForceMeshUpdate();

            TMP_TextInfo textInfo = _text.textInfo;
            for (int i = _prevCharactersToShow; i < newCount; i++)
            {
                if(i < textInfo.characterCount)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    if(!char.IsWhiteSpace(charInfo.character))
                    {
                        Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) / 2;
                        Vector3 worldPos = _text.transform.TransformPoint(charCenter);
                        AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.FillLetterSFX, worldPos);
                    }
                }
            }
        }
        else // newCount < _prevCharactersToShow, letters removed.
        {
            for (int i = newCount; i < _prevCharactersToShow; i++)
            {
                if(_cachedLetterPositions != null && i < _cachedLetterPositions.Length)
                {
                    AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.UnfillLetterSFX, _cachedLetterPositions[i]);
                }
                else
                {
                    AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.UnfillLetterSFX, transform.position);
                }
            }
            _text.text = _fullText.Substring(0, newCount);
            _text.ForceMeshUpdate();
        }

        UpdateCachedLetterPositions(newCount);
        _prevCharactersToShow = newCount;
    }

    /// <summary>
    /// Caches the world positions for the currently visible letters.
    /// </summary>
    /// <param name="count">The number of currently displayed letters.</param>
    private void UpdateCachedLetterPositions(int count)
    {
        TMP_TextInfo textInfo = _text.textInfo;
        if(_cachedLetterPositions == null || _cachedLetterPositions.Length < count)
        {
            _cachedLetterPositions = new Vector3[_totalCharacters];
        }
        for (int i = 0; i < count; i++)
        {
            if(i < textInfo.characterCount)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) / 2;
                _cachedLetterPositions[i] = _text.transform.TransformPoint(charCenter);
            }
        }
    }
    #endregion

    #region BASE
    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _fullText = _text.text;
        _text.text = ""; // Clear initially
        _totalCharacters = _fullText.Length;

        _constStart = _timeBC;
        _constEnd = _timeTC;

        // Initially, we're showing zero characters
        _prevCharactersToShow = 0;
    }

    private void OnEnable()
    {
        TimeManager.Instance.RegisterTask(this);
        Stopped += OnStopped;
        Resumed += OnResumed;
    }

    private void OnDisable()
    {
        TimeManager.Instance.UnregisterTask(this);
        Stopped -= OnStopped;
        Resumed -= OnResumed;
    }
    #endregion
}
