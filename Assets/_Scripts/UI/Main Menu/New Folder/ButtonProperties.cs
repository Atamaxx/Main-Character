using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "ButtonStyle", menuName = "Menu System/Button Style")]
public class ButtonProperties : ScriptableObject
{
    [Foldout("Colors")]
    public Color NormalColor = Color.black;
    
    [Foldout("Colors")]
    public Color HoverColor = Color.gray;
    
    [Foldout("Colors")]
    public Color SelectedColor = Color.white;
    
    [Foldout("Colors")]
    public Color PressedColor = new Color(0.9f, 0.9f, 1f);
    
    [Foldout("Colors")]
    public Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Foldout("Scaling")]
    [Range(0.5f, 1.5f)]
    public float NormalScale = 1.0f;
    
    [Foldout("Scaling")]
    [Range(0.5f, 1.5f)]
    public float HoverScale = 1.1f;
    
    [Foldout("Scaling")]
    [Range(0.5f, 1.5f)]
    public float SelectedScale = 1.2f;
    
    [Foldout("Scaling")]
    [Range(0.5f, 1.5f)]
    public float PressedScale = 0.95f;
    
    [Foldout("Animation")]
    [Range(1f, 20f)]
    public float AnimationSpeed = 10f;
    
    [Foldout("Animation")]
    [Range(0.01f, 1f)]
    public float PressAnimationDuration = 0.1f;
}