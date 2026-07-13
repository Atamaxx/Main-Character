using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

/// <summary>
/// Types out text on a TextMeshPro label, playing a corresponding FMOD note event for each character.
/// The typing speed, pause durations, and note volume are randomized for a dynamic effect.
/// Uses NaughtyAttributes for a cleaner inspector.
/// </summary>
public class FmodTextTyper : MonoBehaviour
{
    [BoxGroup("UI Components")]
    [Required("Text Label must be assigned.")]
    [SerializeField]
    private TMP_Text textLabel;

    [BoxGroup("FMOD Settings")]
    [SerializeField]
    private EventReference noteSoundEvent;

    [BoxGroup("FMOD Settings")]
    [InfoBox("The name of the parameter in your FMOD event that controls the note (e.g., 'Note').")]
    [SerializeField]
    private string noteParameterName = "Note";

    [BoxGroup("FMOD Settings")]
    [InfoBox("The name of the parameter for note volume (0 to 1).")]
    [SerializeField]
    private string noteIntensityParameterName = "NoteIntensity";

    [BoxGroup("Typing Rhythm")]
    [SerializeField]
    private float tempo = 120.0f;

    [BoxGroup("Default Rhythm & Dynamics")]
    [MinMaxSlider(0.0f, 1.0f)]
    [SerializeField]
    [Tooltip("Default random volume for notes not preceding punctuation.")]
    private Vector2 noteIntensityRange = new Vector2(0.8f, 1.0f);

    // --- Individual Pause Durations ---
    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 spacePause = new Vector2(0.5f, 0.7f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 commaPause = new Vector2(1.0f, 1.2f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 semicolonPause = new Vector2(1.2f, 1.4f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 periodPause = new Vector2(1.5f, 2.0f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 questionMarkPause = new Vector2(1.6f, 2.1f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 exclamationMarkPause = new Vector2(1.7f, 2.2f);

    [BoxGroup("Pause Durations (in Beats)")]
    [MinMaxSlider(0.1f, 5f)]
    public Vector2 ellipsisPause = new Vector2(2.0f, 2.5f);

    // --- Punctuation Intensities ---
    [BoxGroup("Punctuation Intensity (for preceding note)")]
    [MinMaxSlider(0.0f, 1.0f)]
    public Vector2 periodIntensity = new Vector2(0.6f, 0.7f);

    [BoxGroup("Punctuation Intensity (for preceding note)")]
    [MinMaxSlider(0.0f, 1.0f)]
    public Vector2 questionMarkIntensity = new Vector2(1.0f, 1.0f);

    [BoxGroup("Punctuation Intensity (for preceding note)")]
    [MinMaxSlider(0.0f, 1.0f)]
    public Vector2 exclamationMarkIntensity = new Vector2(1.0f, 1.0f);

    [BoxGroup("Punctuation Intensity (for preceding note)")]
    [MinMaxSlider(0.0f, 1.0f)]
    public Vector2 ellipsisIntensity = new Vector2(0.4f, 0.5f);

    [BoxGroup("Content")]
    [ResizableTextArea]
    [SerializeField]
    private string storyText;

    private Coroutine _typingCoroutine;
    private Dictionary<char, int> _noteMapping;

    private void Awake()
    {
        InitializeNoteMapping();
        if (textLabel == null)
        {
            Debug.LogError("Text Label is not assigned! Disabling component.", this);
            this.enabled = false;
        }
        else
        {
            textLabel.text = string.Empty;
        }
    }

    private void Start()
    {
        StartTyping();
    }

    [Button("Restart Typing", EButtonEnableMode.Playmode)]
    public void StartTyping()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (Application.isPlaying)
        {
            textLabel.text = string.Empty;
            // CORRECTED: Call the coroutine without a parameter.
            _typingCoroutine = StartCoroutine(TypeText());
        }
        else
        {
            Debug.LogWarning("Cannot start typing in Edit Mode.");
        }
    }

    private void InitializeNoteMapping()
    {
        _noteMapping = new Dictionary<char, int>
        {
            { 'A', 9 },
            { 'B', 11 },
            { 'C', 0 },
            { 'D', 2 },
            { 'E', 4 },
            { 'F', 5 },
            { 'G', 7 },
            { 'H', 10 },
            { 'I', 1 },
            { 'J', 3 },
            { 'K', 6 },
            { 'L', 8 },
            { 'M', 9 },
            { 'N', 11 },
            { 'O', 0 },
            { 'P', 2 },
            { 'Q', 4 },
            { 'R', 5 },
            { 'S', 7 },
            { 'T', 10 },
            { 'U', 1 },
            { 'V', 3 },
            { 'W', 6 },
            { 'X', 8 },
            { 'Y', 4 },
            { 'Z', 9 },
        };
    }

    private void PlayNote(int noteValue, float intensity)
    {
        if (noteSoundEvent.IsNull)
            return;

        FMOD.Studio.EventInstance noteInstance = RuntimeManager.CreateInstance(noteSoundEvent);
        noteInstance.setParameterByName(noteParameterName, noteValue);
        noteInstance.setParameterByName(noteIntensityParameterName, intensity);
        noteInstance.start();
        noteInstance.release();
    }

    // CORRECTED: The method no longer takes a parameter. It will use the class field 'storyText'.
    private IEnumerator TypeText()
    {
        float singleBeatDelay = 60.0f / tempo;

        for (int i = 0; i < storyText.Length; i++)
        {
            char character = storyText[i];
            char upperChar = char.ToUpper(character);

            // --- Handle Letters ---
            if (_noteMapping.ContainsKey(upperChar))
            {
                // Start with default intensity
                float intensity = Random.Range(noteIntensityRange.x, noteIntensityRange.y);

                // Look ahead for punctuation to override intensity
                if (i + 1 < storyText.Length)
                {
                    // Check for ellipsis first
                    if (i + 3 < storyText.Length && storyText.Substring(i + 1, 3) == "...")
                    {
                        intensity = Random.Range(ellipsisIntensity.x, ellipsisIntensity.y);
                    }
                    else
                    {
                        char nextChar = storyText[i + 1];
                        if (nextChar == '.')
                            intensity = Random.Range(periodIntensity.x, periodIntensity.y);
                        else if (nextChar == '?')
                            intensity = Random.Range(
                                questionMarkIntensity.x,
                                questionMarkIntensity.y
                            );
                        else if (nextChar == '!')
                            intensity = Random.Range(
                                exclamationMarkIntensity.x,
                                exclamationMarkIntensity.y
                            );
                    }
                }

                textLabel.text += character;
                PlayNote(_noteMapping[upperChar], intensity);
                yield return new WaitForSeconds(singleBeatDelay);
            }
            // --- Handle Punctuation and Pauses ---
            else
            {
                textLabel.text += character;
                float randomPause = 1f; // Default pause of 1 beat

                // Handle special case for "..." to avoid triple pauses
                if (
                    character == '.'
                    && i <= storyText.Length - 3
                    && storyText.Substring(i, 3) == "..."
                )
                {
                    textLabel.text += ".."; // Append the other two dots
                    i += 2; // Skip the next two dots
                    randomPause = Random.Range(ellipsisPause.x, ellipsisPause.y);
                }
                else if (character == '.')
                    randomPause = Random.Range(periodPause.x, periodPause.y);
                else if (character == '?')
                    randomPause = Random.Range(questionMarkPause.x, questionMarkPause.y);
                else if (character == '!')
                    randomPause = Random.Range(exclamationMarkPause.x, exclamationMarkPause.y);
                else if (character == ',')
                    randomPause = Random.Range(commaPause.x, commaPause.y);
                else if (character == ';')
                    randomPause = Random.Range(semicolonPause.x, semicolonPause.y);
                else if (character == ' ')
                    randomPause = Random.Range(spacePause.x, spacePause.y);

                yield return new WaitForSeconds(singleBeatDelay * randomPause);
            }
        }
        _typingCoroutine = null;
    }
}
