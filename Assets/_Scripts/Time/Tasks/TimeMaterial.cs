using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class TimeMaterial : MonoBehaviour, ITimelineTask
{
    [SerializeField, BoxGroup("SETTINGS")]
    private bool _pause = false;
    [SerializeField, BoxGroup("SETTINGS")] private bool _createInstance = false;

    [SerializeField, BoxGroup("SETTINGS")]
    private List<MaterialChange> _materialChanges = new();

    [SerializeField, BoxGroup("-T")]
    private bool _useConstantChange;
    // Use _timeBC as lower bound and _timeTC as upper bound (default 0 to 10)
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantChange")]
    private float _timeBC = 0f;
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantChange")]
    private float _timeTC = 10f;
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantChange")]
    private float _constantSpeed = 1f;
    [SerializeField, BoxGroup("-T"), ShowIf("_useConstantChange")]
    private bool _pingPong = true;

    // Accumulator for constant time change
    private float _accumulator = 0f;
    private SpriteRenderer _spriteRenderer;

    #region BASE
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Start()
    {
        foreach (var materialChange in _materialChanges)
        {
            UpdatePropertyIds(materialChange.propertyFloats);
            UpdatePropertyIds(materialChange.propertyVector2s);
            UpdatePropertyIds(materialChange.propertyColors);
            UpdatePropertyIds(materialChange.propertyTextures);
        }
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

    private void UpdatePropertyIds<T>(List<T> properties) where T : IProperty
    {
        if (properties == null) return;
        for (int i = 0; i < properties.Count; i++)
        {
            T property = properties[i];
            property.propertyId = Shader.PropertyToID(property.materialRef);
            properties[i] = property;
        }
    }
    #endregion

    #region INTERFACE
    public event Action Stopped;
    public event Action Resumed;

    public void OnUpdate(float currentTime)
    {
        if (_pause) return;

        // If using constant change, ignore the passed-in currentTime entirely.
        if (_useConstantChange)
        {
            _accumulator += Time.deltaTime * _constantSpeed;
            float range = _timeTC - _timeBC;
            currentTime = _pingPong
                ? Mathf.PingPong(_accumulator, range) + _timeBC
                : Mathf.Repeat(_accumulator, range) + _timeBC;
        }

        foreach (var materialChange in _materialChanges)
        {
            UpdateMaterialProperties(materialChange, currentTime);
        }
    }

    public void ManualyUpdateTime(float currentTime)
    {
        foreach (var materialChange in _materialChanges)
        {
            UpdateMaterialProperties(materialChange, currentTime);
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

    #region MATERIAL
    private void UpdateMaterialProperties(MaterialChange materialChange, float currentTime)
    {
        if (materialChange.material == null)
        {
            if (_createInstance)
            {
                materialChange.material = _spriteRenderer.material;
            }
            else
            {
                materialChange.material = _spriteRenderer.sharedMaterial;
            }
        }

        if (materialChange.propertyFloats != null)
        {
            foreach (var property in materialChange.propertyFloats)
            {
                float value = My.TimeChanges.FloatByTime(
                    property.startValue,
                    property.endValue,
                    property.changingSegment.x,
                    property.changingSegment.y,
                    currentTime);
                materialChange.material.SetFloat(property.propertyId, value);
            }
        }

        if (materialChange.propertyVector2s != null)
        {
            foreach (var property in materialChange.propertyVector2s)
            {
                Vector2 value = My.TimeChanges.Vector2ByTime(
                    property.startValue,
                    property.endValue,
                    property.changingSegment.x,
                    property.changingSegment.y,
                    currentTime);
                materialChange.material.SetVector(property.propertyId, value);
            }
        }

        if (materialChange.propertyColors != null)
        {
            foreach (var property in materialChange.propertyColors)
            {
                Color value = My.TimeChanges.ColorByTime(
                    property.startValue,
                    property.endValue,
                    property.changingSegment.x,
                    property.changingSegment.y,
                    currentTime);
                materialChange.material.SetColor(property.propertyId, value);
            }
        }

        if (materialChange.propertyTextures != null)
        {
            foreach (var property in materialChange.propertyTextures)
            {
                Vector2 offsetValue = My.TimeChanges.Vector2ByTime(
                    property.offsetStart,
                    property.offsetEnd,
                    property.changingSegment.x,
                    property.changingSegment.y,
                    currentTime);
                materialChange.material.SetTextureOffset(property.propertyId, offsetValue);
            }
        }
    }
    #endregion
}
