using System.Collections;
using TMPro;
using UnityEngine;

public class LetterVisual : MonoBehaviour
{
    [SerializeField] private Color32 _fillColor = new Color32(30, 0, 0, 255);
    [SerializeField] private bool _customStartColor = false;
    [SerializeField] private Color32 _startColor = new Color32(0, 0, 0, 50);
    [SerializeField] private float _fillDuration = 0.15f;


    public IEnumerator FillCharacterOverTime(int charIndex, bool fromFilled, TMP_Text tmpText)
    {
        TMP_TextInfo textInfo = tmpText.textInfo;
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

        if (!charInfo.isVisible)
        {
            yield return null;
        }

        int meshIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

        // Current color is our start point
        Color32 initialColor = vertexColors[vertexIndex];

        // Determine end color
        Color32 endColor = fromFilled ? _startColor : _fillColor;

        float elapsed = 0f;

        while (elapsed < _fillDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fillDuration);

            byte r = (byte)Mathf.Lerp(initialColor.r, endColor.r, t);
            byte g = (byte)Mathf.Lerp(initialColor.g, endColor.g, t);
            byte b = (byte)Mathf.Lerp(initialColor.b, endColor.b, t);
            byte a = (byte)Mathf.Lerp(initialColor.a, endColor.a, t);

            Color32 currentColor = new Color32(r, g, b, a);

            vertexColors[vertexIndex + 0] = currentColor;
            vertexColors[vertexIndex + 1] = currentColor;
            vertexColors[vertexIndex + 2] = currentColor;
            vertexColors[vertexIndex + 3] = currentColor;

            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null;
        }

        // Ensure final color is set
        vertexColors[vertexIndex + 0] = endColor;
        vertexColors[vertexIndex + 1] = endColor;
        vertexColors[vertexIndex + 2] = endColor;
        vertexColors[vertexIndex + 3] = endColor;
        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        yield return null;
    }

    public IEnumerator SmoothColorTransition(TMP_Text tmpText, Color targetColor)
    {
        Color initialColor = tmpText.color;
        float elapsedTime = 0f;

        while (elapsedTime < _fillDuration)
        {
            tmpText.color = Color.Lerp(initialColor, targetColor, elapsedTime / _fillDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        tmpText.color = targetColor;
    }

    #region PUBLIC EVENTS
    public void SetAllCharactersToStartColor(TMP_Text tmpText)
    {
        if (!_customStartColor)
        {
            _startColor = tmpText.color;
        }

        TMP_TextInfo textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

            vertexColors[vertexIndex + 0] = _startColor;
            vertexColors[vertexIndex + 1] = _startColor;
            vertexColors[vertexIndex + 2] = _startColor;
            vertexColors[vertexIndex + 3] = _startColor;
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }



    #endregion
}
