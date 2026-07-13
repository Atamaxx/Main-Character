using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Color System/Color Palette", order = 0)]
public class ColorPalette : ScriptableObject
{
    [Header("FILL COLOR")]              // Filled player, timeline, letters, etc.
    [ColorUsage(true, false)] public Color FillColorDefault;
    [ColorUsage(true, true)] public Color FillColorHDR;


    [Header("BASIC")]                   // Letters, platforms, timeline unfilled, etc.
    [ColorUsage(true, false)] public Color BasicColorDefault;
    [ColorUsage(true, true)] public Color BasicColorHDR;

    [Header("INTERACTABLE COLORS")]     // Unfilled letters, interactable objects
    [ColorUsage(true, false)] public Color InteractableColorDefault;
    [ColorUsage(true, true)] public Color InteractableColorHDR;

    [Header("BACKGROUND COLOR")]        // Background
    [ColorUsage(true, false)] public Color BackgroundColorDefault;
    [ColorUsage(true, true)] public Color BackgroundColorHDR;
}
