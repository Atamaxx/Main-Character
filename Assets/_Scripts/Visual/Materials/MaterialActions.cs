using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MaterialActions : MonoBehaviour
{
    [System.Serializable]
    private class PropertyChange<T>
    {
        public string propertyName;

        [HideInInspector]
        public int propertyID;
        public T startValue;
        public T endValue;
        public float duration;
        public float elapsedTime = 0;
        public Coroutine coroutine;
    }

    [SerializeField]
    private Material _targetMaterial;

    [SerializeField]
    private bool _createInstance = true;

    [SerializeField]
    private bool _changeMaterialOnStart = true;

    [SerializeField, ShowIf("_changeMaterialOnStart")]
    private List<SpriteRenderer> _spriteRenderers = new();
    public Material MaterialInc { get; private set; }

    [Header("COLOR PROPERTIES")]
    [SerializeField]
    private List<PropertyChange<Color>> colorChanges = new();

    [Header("FLOAT PROPERTIES")]
    [SerializeField]
    private List<PropertyChange<float>> floatChanges = new();

    [Header("VECTOR2 PROPERTIES")]
    [SerializeField]
    private List<PropertyChange<Vector2>> vector2Changes = new();

    private void Awake()
    {
        foreach (var colorChange in colorChanges)
            colorChange.propertyID = Shader.PropertyToID(colorChange.propertyName);

        foreach (var floatChange in floatChanges)
            floatChange.propertyID = Shader.PropertyToID(floatChange.propertyName);

        foreach (var vector2Change in vector2Changes)
            vector2Change.propertyID = Shader.PropertyToID(vector2Change.propertyName);

        if (_createInstance)
            MaterialInc = new Material(_targetMaterial);
        else
            MaterialInc = _targetMaterial;
    }

    private void Start()
    {
        if (_changeMaterialOnStart)
        {
            foreach (var spriteRenderer in _spriteRenderers)
                spriteRenderer.material = MaterialInc;
        }
    }

    #region ANIMATED

    // --- Animated Transitions ---

    // Color transitions
    public void ChangeColorsToStart()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
                StopCoroutine(colorChange.coroutine);

            colorChange.coroutine = StartCoroutine(ChangeColorToStart(colorChange));
        }
    }

    public void ChangeColorsToEnd()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
                StopCoroutine(colorChange.coroutine);

            colorChange.coroutine = StartCoroutine(ChangeColorToEnd(colorChange));
        }
    }

    // Float transitions
    public void ChangeFloatsToStart()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
                StopCoroutine(floatChange.coroutine);

            floatChange.coroutine = StartCoroutine(ChangeFloatToStart(floatChange));
        }
    }

    public void ChangeFloatsToEnd()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
                StopCoroutine(floatChange.coroutine);

            floatChange.coroutine = StartCoroutine(ChangeFloatToEnd(floatChange));
        }
    }

    // Vector2 transitions
    public void ChangeVector2sToStart()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
                StopCoroutine(vector2Change.coroutine);

            vector2Change.coroutine = StartCoroutine(ChangeVector2ToStart(vector2Change));
        }
    }

    public void ChangeVector2sToEnd()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
                StopCoroutine(vector2Change.coroutine);

            vector2Change.coroutine = StartCoroutine(ChangeVector2ToEnd(vector2Change));
        }
    }

    // Change all properties via animation
    public void AllToEnd()
    {
        ChangeColorsToEnd();
        ChangeFloatsToEnd();
        ChangeVector2sToEnd();
    }

    public void AllToStart()
    {
        ChangeColorsToStart();
        ChangeFloatsToStart();
        ChangeVector2sToStart();
    }
    #endregion

    // --- Immediate Setting Methods ---
    #region IMMEDIATE
    // Colors
    public void SetColorsToStartImmediate()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
            {
                StopCoroutine(colorChange.coroutine);
                colorChange.coroutine = null;
            }
            colorChange.elapsedTime = 0;

            MaterialInc.SetColor(colorChange.propertyID, colorChange.startValue);
        }
    }

    public void SetColorsToEndImmediate()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
            {
                StopCoroutine(colorChange.coroutine);
                colorChange.coroutine = null;
            }
            colorChange.elapsedTime = colorChange.duration;
            print(colorChange.endValue + " " + colorChange.propertyID);
            MaterialInc.SetColor(colorChange.propertyID, colorChange.endValue);
        }
    }

    // Floats
    public void SetFloatsToStartImmediate()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
            {
                StopCoroutine(floatChange.coroutine);
                floatChange.coroutine = null;
            }
            floatChange.elapsedTime = 0;
            MaterialInc.SetFloat(floatChange.propertyID, floatChange.startValue);
        }
    }

    public void SetFloatsToEndImmediate()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
            {
                StopCoroutine(floatChange.coroutine);
                floatChange.coroutine = null;
            }
            floatChange.elapsedTime = floatChange.duration;
            MaterialInc.SetFloat(floatChange.propertyID, floatChange.endValue);
        }
    }

    // Vector2s
    public void SetVector2sToStartImmediate()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
            {
                StopCoroutine(vector2Change.coroutine);
                vector2Change.coroutine = null;
            }
            vector2Change.elapsedTime = 0;
            MaterialInc.SetVector(
                vector2Change.propertyID,
                new Vector4(vector2Change.startValue.x, vector2Change.startValue.y, 0f, 0f)
            );
        }
    }

    public void SetVector2sToEndImmediate()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
            {
                StopCoroutine(vector2Change.coroutine);
                vector2Change.coroutine = null;
            }
            vector2Change.elapsedTime = vector2Change.duration;
            MaterialInc.SetVector(
                vector2Change.propertyID,
                new Vector4(vector2Change.endValue.x, vector2Change.endValue.y, 0f, 0f)
            );
        }
    }

    // Optional: Immediately set all properties
    public void AllToStartImmediate()
    {
        SetColorsToStartImmediate();
        SetFloatsToStartImmediate();
        SetVector2sToStartImmediate();
    }

    public void AllToEndImmediate()
    {
        SetColorsToEndImmediate();
        SetFloatsToEndImmediate();
        SetVector2sToEndImmediate();
    }
    #endregion

    #region RESET
    public void ChangeColorsToStartReset()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
                StopCoroutine(colorChange.coroutine);
            colorChange.elapsedTime = colorChange.duration;
            colorChange.coroutine = StartCoroutine(ChangeColorToStart(colorChange));
        }
    }

    public void ChangeColorsToEndReset()
    {
        foreach (var colorChange in colorChanges)
        {
            if (colorChange.coroutine != null)
                StopCoroutine(colorChange.coroutine);
            colorChange.elapsedTime = 0;
            colorChange.coroutine = StartCoroutine(ChangeColorToEnd(colorChange));
        }
    }

    // Float transitions
    public void ChangeFloatsToStartReset()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
                StopCoroutine(floatChange.coroutine);
            floatChange.elapsedTime = floatChange.duration;
            floatChange.coroutine = StartCoroutine(ChangeFloatToStart(floatChange));
        }
    }

    public void ChangeFloatsToEndReset()
    {
        foreach (var floatChange in floatChanges)
        {
            if (floatChange.coroutine != null)
                StopCoroutine(floatChange.coroutine);
            floatChange.elapsedTime = 0;

            floatChange.coroutine = StartCoroutine(ChangeFloatToEnd(floatChange));
        }
    }

    // Vector2 transitions
    public void ChangeVector2sToStartReset()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
                StopCoroutine(vector2Change.coroutine);
            vector2Change.elapsedTime = vector2Change.duration;
            vector2Change.coroutine = StartCoroutine(ChangeVector2ToStart(vector2Change));
        }
    }

    public void ChangeVector2sToEndReset()
    {
        foreach (var vector2Change in vector2Changes)
        {
            if (vector2Change.coroutine != null)
                StopCoroutine(vector2Change.coroutine);
            vector2Change.elapsedTime = 0;

            vector2Change.coroutine = StartCoroutine(ChangeVector2ToEnd(vector2Change));
        }
    }

    public void AllToStartReset()
    {
        ChangeColorsToStartReset();
        ChangeFloatsToStartReset();
        ChangeVector2sToStartReset();
    }

    public void AllToEndReset()
    {
        ChangeColorsToEndReset();
        ChangeFloatsToEndReset();
        ChangeVector2sToEndReset();
    }
    #endregion

    #region COROUTINES

    // Color coroutines
    private IEnumerator ChangeColorToStart(PropertyChange<Color> colorChange, float minTime = 0)
    {
        while (colorChange.elapsedTime > minTime)
        {
            float t = Mathf.Clamp01(colorChange.elapsedTime / colorChange.duration);
            // Lerp from startValue to endValue; as elapsedTime decreases, t goes from 1 to minTime.
            Color newColor = Color.Lerp(colorChange.startValue, colorChange.endValue, t);
            MaterialInc.SetColor(colorChange.propertyID, newColor);

            colorChange.elapsedTime -= Time.deltaTime;
            yield return null;
        }
        colorChange.elapsedTime = minTime;
        MaterialInc.SetColor(colorChange.propertyID, colorChange.startValue);
        colorChange.coroutine = null;
    }

    private IEnumerator ChangeColorToEnd(PropertyChange<Color> colorChange)
    {
        while (colorChange.elapsedTime < colorChange.duration)
        {
            float t = Mathf.Clamp01(colorChange.elapsedTime / colorChange.duration);
            Color newColor = Color.Lerp(colorChange.startValue, colorChange.endValue, t);
            MaterialInc.SetColor(colorChange.propertyID, newColor);

            colorChange.elapsedTime += Time.deltaTime;
            yield return null;
        }
        colorChange.elapsedTime = colorChange.duration;
        MaterialInc.SetColor(colorChange.propertyID, colorChange.endValue);
        colorChange.coroutine = null;
    }

    // Float coroutines
    private IEnumerator ChangeFloatToStart(PropertyChange<float> floatChange, float minTime = 0)
    {
        while (floatChange.elapsedTime > minTime)
        {
            float t = Mathf.Clamp01(floatChange.elapsedTime / floatChange.duration);
            float newFloat = Mathf.Lerp(floatChange.startValue, floatChange.endValue, t);
            MaterialInc.SetFloat(floatChange.propertyID, newFloat);
            floatChange.elapsedTime -= Time.deltaTime;
            yield return null;
        }
        floatChange.elapsedTime = minTime;
        MaterialInc.SetFloat(floatChange.propertyID, floatChange.startValue);
        floatChange.coroutine = null;
    }

    private IEnumerator ChangeFloatToEnd(PropertyChange<float> floatChange)
    {
        while (floatChange.elapsedTime < floatChange.duration)
        {
            float t = Mathf.Clamp01(floatChange.elapsedTime / floatChange.duration);
            float newFloat = Mathf.Lerp(floatChange.startValue, floatChange.endValue, t);
            MaterialInc.SetFloat(floatChange.propertyID, newFloat);

            floatChange.elapsedTime += Time.deltaTime;
            yield return null;
        }
        floatChange.elapsedTime = floatChange.duration;
        MaterialInc.SetFloat(floatChange.propertyID, floatChange.endValue);
        floatChange.coroutine = null;
    }

    // Vector2 coroutines
    private IEnumerator ChangeVector2ToStart(
        PropertyChange<Vector2> vector2Change,
        float minTime = 0
    )
    {
        while (vector2Change.elapsedTime > minTime)
        {
            float t = Mathf.Clamp01(vector2Change.elapsedTime / vector2Change.duration);
            Vector2 newVector2 = Vector2.Lerp(vector2Change.startValue, vector2Change.endValue, t);
            // Material.SetVector expects a Vector4; convert the Vector2 accordingly.
            MaterialInc.SetVector(
                vector2Change.propertyID,
                new Vector4(newVector2.x, newVector2.y, 0f, 0f)
            );

            vector2Change.elapsedTime -= Time.deltaTime;
            yield return null;
        }
        vector2Change.elapsedTime = minTime;
        MaterialInc.SetVector(
            vector2Change.propertyID,
            new Vector4(vector2Change.startValue.x, vector2Change.startValue.y, 0f, 0f)
        );
        vector2Change.coroutine = null;
    }

    private IEnumerator ChangeVector2ToEnd(PropertyChange<Vector2> vector2Change)
    {
        while (vector2Change.elapsedTime < vector2Change.duration)
        {
            float t = Mathf.Clamp01(vector2Change.elapsedTime / vector2Change.duration);
            Vector2 newVector2 = Vector2.Lerp(vector2Change.startValue, vector2Change.endValue, t);
            MaterialInc.SetVector(
                vector2Change.propertyID,
                new Vector4(newVector2.x, newVector2.y, 0f, 0f)
            );

            vector2Change.elapsedTime += Time.deltaTime;
            yield return null;
        }
        vector2Change.elapsedTime = vector2Change.duration;
        MaterialInc.SetVector(
            vector2Change.propertyID,
            new Vector4(vector2Change.endValue.x, vector2Change.endValue.y, 0f, 0f)
        );
        vector2Change.coroutine = null;
    }

    #endregion
}
