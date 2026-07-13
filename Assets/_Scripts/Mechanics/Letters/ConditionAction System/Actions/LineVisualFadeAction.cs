using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Letters
{
    public class LineVisualFadeAction : Action
    {
        [SerializeField] private bool _toggleOn = true;
        [SerializeField] private List<LineRenderer> _renderers = new();

        [Header("Fade Settings")]
        [SerializeField, Range(0, 1)] private float _startAlpha = 1f;
        [SerializeField, Range(0, 1)] private float _targetAlpha = 0f;
        [SerializeField] private float _animationTime = 0.2f;

        private Dictionary<LineRenderer, Coroutine> _fadeCoroutines = new();

        // For demonstration, let's imagine these are the triggers for the logic:
        public override void OnConditionMet()
        {
            if (_toggleOn)
            {
                ShowVisuals();
            }
            else
            {
                HideVisuals();
            }
        }

        public override void OnConditionNotMet()
        {
            if (_toggleOn)
            {
                HideVisuals();
            }
            else
            {
                ShowVisuals();
            }
        }

        private void ShowVisuals()
        {
            // Example: fade in to alpha = 1
            foreach (var renderer in _renderers)
            {
                // You might want to always start from _startAlpha if you prefer:
                // renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, _startAlpha);

                HandleFade(renderer, 1f);
            }
        }

        private void HideVisuals()
        {
            // Example: fade out to alpha = 0
            foreach (var renderer in _renderers)
            {
                HandleFade(renderer, 0f);
            }
        }

        private void HandleFade(LineRenderer renderer, float targetAlpha)
        {
            if (_fadeCoroutines.TryGetValue(renderer, out var runningCoroutine) && runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }

            var newCoroutine = StartCoroutine(FadeTo(renderer, targetAlpha, _animationTime));
            _fadeCoroutines[renderer] = newCoroutine;
        }

        private IEnumerator FadeTo(LineRenderer renderer, float targetAlpha, float duration)
        {
            if (renderer == null) yield break;

            float elapsed = 0;
            Color startColor = renderer.startColor;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                Color color = Color.Lerp(startColor, targetColor, normalizedTime);
                renderer.startColor = color;
                renderer.endColor = color;
                yield return null;
            }

            renderer.startColor = targetColor;
            renderer.endColor = targetColor;
        }
    }
}
