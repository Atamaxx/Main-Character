using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class RestartLevel : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textComponent;
    [SerializeField] private string _originalText = "restart";
    [SerializeField] private string _restartText = "restart.";
    [SerializeField] private ButtonProperties _buttonProperties;
    [SerializeField] private RenderTextureLevelCapture _levelCapture;

    private Coroutine _restartCoroutine;
    private bool _isCompleting = false;

    public void Restart()
    {
        _levelCapture.CaptureLevel();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void StartRestartSequence()
    {
        // Check component references
        if (_textComponent == null || _buttonProperties == null)
        {
            return;
        }

        if (_restartCoroutine == null && !_isCompleting)
        {
            _restartCoroutine = StartCoroutine(RestartLevelButtonStart());
        }
    }

    public IEnumerator RestartLevelButtonStart()
    {
        _isCompleting = false;

        // Verify Rich Text is enabled
        if (!_textComponent.richText)
        {
            _textComponent.richText = true;
        }

        // Use full RGBA color strings including alpha
        string pressedColorHex = ColorUtility.ToHtmlStringRGBA(_buttonProperties.PressedColor);
        string normalColorHex = ColorUtility.ToHtmlStringRGBA(_buttonProperties.NormalColor);

        // Make sure text is set with the normal color first
        _textComponent.text = $"<color=#{normalColorHex}>{_restartText}</color>";

        AudioSystem.Instance.PlaySFXLoop("restartLevelHold", FMODEvents.Instance.LevelResetHoldSFX);

        // Fill letters progressively using WaitForSecondsRealtime instead of WaitForSeconds
        ColorText(1, pressedColorHex, normalColorHex);
        yield return new WaitForSecondsRealtime(0.2f);

        yield return new WaitForSecondsRealtime(0.32f);
        ColorText(3, pressedColorHex, normalColorHex);

        yield return new WaitForSecondsRealtime(0.33f);
        ColorText(4, pressedColorHex, normalColorHex);

        yield return new WaitForSecondsRealtime(0.32f);
        ColorText(6, pressedColorHex, normalColorHex);

        yield return new WaitForSecondsRealtime(0.1f);
        ColorText(7, pressedColorHex, normalColorHex);

        yield return new WaitForSecondsRealtime(0.015f);
        ColorText(_restartText.Length, pressedColorHex, normalColorHex);
        _textComponent.color = _buttonProperties.PressedColor;

        _isCompleting = true;

        // Give time to see full colored text before restart
        yield return new WaitForSecondsRealtime(0.3f);
        AudioSystem.Instance.StopSFXLoop("restartLevelHold");

        Restart();
    }

    private void ColorText(int coloredLetterCount, string pressedColorHex, string normalColorHex)
    {
        string coloredPart = _restartText.Substring(0, Mathf.Min(coloredLetterCount, _restartText.Length));
        string normalPart = coloredLetterCount < _restartText.Length ?
            _restartText.Substring(coloredLetterCount) : "";

        // Force the colored part to be fully opaque with a custom color tag
        Color32 pressedColor = _buttonProperties.PressedColor;
        pressedColor.a = 255; // Fully opaque
        string opaqueColorHex = ColorUtility.ToHtmlStringRGBA(pressedColor);

        string newText = $"<color=#{opaqueColorHex}>{coloredPart}</color><color=#{normalColorHex}>{normalPart}</color>";
        _textComponent.text = newText;
    }

    public void CancelRestartSequence()
    {
        // Don't cancel if we're already completing the sequence
        if (_isCompleting) return;

        if (_restartCoroutine != null)
        {
            StopCoroutine(_restartCoroutine);
            _restartCoroutine = null;

            if (_textComponent != null && _buttonProperties != null)
            {
                string normalColorHex = ColorUtility.ToHtmlStringRGB(_buttonProperties.NormalColor);
                _textComponent.text = $"<color=#{normalColorHex}>{_originalText}</color>";
            }

            AudioSystem.Instance.StopSFXLoop("restartLevelHold");
        }
    }
}