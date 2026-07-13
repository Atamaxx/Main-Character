using System.Collections;
using UnityEngine;

public class MaterialChanges : MonoBehaviour
{
    [SerializeField] private MaterialPropertyColor _materialColor;
    [SerializeField] private MaterialPropertyFloat _materialFloat;
    [SerializeField] private float _fillDuration;

    private void Start() {
        _materialColor.propertyId = Shader.PropertyToID(_materialColor.materialRef);
        _materialFloat.propertyId = Shader.PropertyToID(_materialFloat.materialRef);
    }

    public IEnumerator SmoothColorTransition(Material material, bool fromFill = false)
    {
        Color initialColor;
        Color targetColor;
        if (fromFill)
        {
            initialColor = material.GetColor(_materialColor.propertyId);
            targetColor = _materialColor.startValue;
        }
        else
        {
            initialColor = material.GetColor(_materialColor.propertyId);
            targetColor = _materialColor.endValue;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _fillDuration)
        {
            material.SetColor(_materialColor.propertyId, Color.Lerp(initialColor, targetColor, elapsedTime / _fillDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        material.SetColor(_materialColor.propertyId, targetColor);
    }

    public IEnumerator SmoothFloatTransition(Material material, bool fromFill = false)
    {
        float initialValue;
        float targetValue;

        if (fromFill)
        {
            initialValue = material.GetFloat(_materialFloat.propertyId);
            targetValue = _materialFloat.startValue;
        }
        else
        {
            initialValue = material.GetFloat(_materialFloat.propertyId);
            targetValue = _materialFloat.endValue;
        }


        float elapsedTime = 0f;

        while (elapsedTime < _fillDuration)
        {
            material.SetFloat(_materialFloat.propertyId, Mathf.Lerp(initialValue, targetValue, elapsedTime / _fillDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        material.SetFloat(_materialFloat.propertyId, targetValue);
    }



    public IEnumerator SmoothColorTransition(Material material, int propertyID, float fillDuration, Color startColor, Color targetColor)
    {
        Color initialColor = startColor;
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            material.SetColor(propertyID, Color.Lerp(initialColor, targetColor, elapsedTime / fillDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        material.SetColor(propertyID, targetColor);
    }
}
