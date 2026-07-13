using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Materials;

public class MaterialPropertyChanger : MonoBehaviour
{
    [System.Serializable]
    public class PropertyChange<T>
    {
        public string propertyName;
        public int propertyID;
        public T startValue;
        public T endValue;
        public float duration;
        public Coroutine coroutine;
    }

    [SerializeField] private Material targetMaterial;
    
    [Header("Color Properties")]
    [SerializeField] private List<PropertyChange<Color>> colorChanges = new List<PropertyChange<Color>>();
    
    [Header("Float Properties")]
    [SerializeField] private List<PropertyChange<float>> floatChanges = new List<PropertyChange<float>>();
    
    [Header("Vector2 Properties")]
    [SerializeField] private List<PropertyChange<Vector2>> vector2Changes = new List<PropertyChange<Vector2>>();

    private void Awake()
    {        
        foreach (var colorChange in colorChanges)
            colorChange.propertyID = Shader.PropertyToID(colorChange.propertyName);
        
        foreach (var floatChange in floatChanges)
            floatChange.propertyID = Shader.PropertyToID(floatChange.propertyName);
        
        foreach (var vector2Change in vector2Changes)
            vector2Change.propertyID = Shader.PropertyToID(vector2Change.propertyName);
    }

    private void StartCoroutineSafe<T>(PropertyChange<T> propertyChange, IEnumerator coroutine)
    {
        if (propertyChange.coroutine != null)
            StopCoroutine(propertyChange.coroutine);
        
        propertyChange.coroutine = StartCoroutine(coroutine);
    }

    public void ChangeColorsToEnd()
    {
        foreach (var colorChange in colorChanges)
            StartCoroutineSafe(colorChange, PropertyChangeHelper.ChangeColorCoroutine(targetMaterial, colorChange.propertyID, colorChange.startValue, 
            colorChange.endValue, colorChange.duration));
    }

    public void ChangeColorsToStart()
    {
        foreach (var colorChange in colorChanges)
            StartCoroutineSafe(colorChange, PropertyChangeHelper.ChangeColorCoroutine(targetMaterial, colorChange.propertyID, 
            colorChange.endValue, colorChange.startValue, colorChange.duration));
    }

    public void ChangeFloatsToEnd()
    {
        foreach (var floatChange in floatChanges)
            StartCoroutineSafe(floatChange, PropertyChangeHelper.ChangeFloatCoroutine(targetMaterial, floatChange.propertyID, floatChange.startValue, floatChange.endValue, floatChange.duration));
    }

    public void ChangeFloatsToStart()
    {
        foreach (var floatChange in floatChanges)
            StartCoroutineSafe(floatChange, PropertyChangeHelper.ChangeFloatCoroutine(targetMaterial, floatChange.propertyID, floatChange.endValue, floatChange.startValue, floatChange.duration));
    }

    public void ChangeVector2sToEnd()
    {
        foreach (var vector2Change in vector2Changes)
            StartCoroutineSafe(vector2Change, PropertyChangeHelper.ChangeVector2Coroutine(targetMaterial, vector2Change.propertyID, vector2Change.startValue, vector2Change.endValue, vector2Change.duration));
    }

    public void ChangeVector2sToStart()
    {
        foreach (var vector2Change in vector2Changes)
            StartCoroutineSafe(vector2Change, PropertyChangeHelper.ChangeVector2Coroutine(targetMaterial, vector2Change.propertyID, vector2Change.endValue, vector2Change.startValue, vector2Change.duration));
    }

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
}