using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;

[Serializable]
public struct MaterialPropertyFloat : IProperty
{
    public string _materialRef;
    public string materialRef { get => _materialRef; set => _materialRef = value; }
    public float startValue;
    public float endValue;
    public Vector2 changingSegment;
    private int _propertyId;
    [HideInInspector] public int propertyId { get => _propertyId; set => _propertyId = value; }
}

[Serializable]
public struct MaterialPropertyVector2 : IProperty
{
    public string _materialRef;
    public string materialRef { get => _materialRef; set => _materialRef = value; }
    public Vector2 startValue;
    public Vector2 endValue;
    public Vector2 changingSegment;
    private int _propertyId;
    [HideInInspector] public int propertyId { get => _propertyId; set => _propertyId = value; }
}


[Serializable]
public struct MaterialPropertyColor : IProperty
{
    public string _materialRef;
    public string materialRef { get => _materialRef; set => _materialRef = value; }
    public Color startValue;
    public Color endValue;
    public Vector2 changingSegment;
    private int _propertyId;
    [HideInInspector] public int propertyId { get => _propertyId; set => _propertyId = value; }
}

[Serializable]
public class MaterialPropertyTexture : IProperty
{
    public string _materialRef;
    public string materialRef { get => _materialRef; set => _materialRef = value; }
    public Vector2 offsetStart;
    public Vector2 offsetEnd;
    public Vector2 changingSegment;
    private int _propertyId;
    [HideInInspector] public int propertyId { get => _propertyId; set => _propertyId = value; }

}
[Serializable]
public class MaterialChange
{
    public Material material;
    public List<MaterialPropertyFloat> propertyFloats = new();
    public List<MaterialPropertyVector2> propertyVector2s = new();
    public List<MaterialPropertyColor> propertyColors = new();
    public List<MaterialPropertyTexture> propertyTextures = new();
}


public interface IProperty
{
    string materialRef { get; set; }
    int propertyId { get; set; }
}