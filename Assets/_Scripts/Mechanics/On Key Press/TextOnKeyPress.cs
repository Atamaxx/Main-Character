using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;
using NaughtyAttributes;
public class TextOnKeyPress : MonoBehaviour
{

    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private LetterVisual _letterVisual;
    [SerializeField, BoxGroup("COLOR SETTINGS")] private bool _useCustomColor;
    [Tooltip("Color when the key is not pressed.")]
    [SerializeField, BoxGroup("COLOR SETTINGS"), ShowIf("_useCustomColor")] private Color _normalColor = Color.white;

    [Tooltip("Color when the key is pressed.")]
    [SerializeField, BoxGroup("COLOR SETTINGS"), ShowIf("_useCustomColor")] private Color _activeColor = Color.red;
    private Coroutine _fillCoroutine;

    private void Awake()
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();
    }
    private void Start()
    {
        if (!_useCustomColor)
        {
            _activeColor = ColorController.Instance.Palette.FillColorDefault;
            _normalColor = ColorController.Instance.Palette.InteractableColorDefault;
        }
        // Initialize color
        _tmpText.color = _normalColor;

    }

    public void FillLetter()
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }
        _fillCoroutine = StartCoroutine(_letterVisual.SmoothColorTransition(_tmpText, _activeColor));
    }

    public void UnfillLetter()
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }
        _fillCoroutine = StartCoroutine(_letterVisual.SmoothColorTransition(_tmpText, _normalColor));
    }
}
