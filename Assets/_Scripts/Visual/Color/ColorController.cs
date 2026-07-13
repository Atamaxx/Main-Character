using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.VFX;
using TMPro;

public class ColorController : Singleton<ColorController>
{
    [field: SerializeField] public ColorPalette Palette { get; private set; }


    [Header("COLOR OBJECTS")]
    [SerializeField] private TimeLineGFX _timeLineGFX;
    [SerializeField] private List<Material> _baseColorMaterials = new();
    [SerializeField] private List<VisualEffect> _vfx = new();
    [SerializeField] private List<Material> _backgroundColorMaterials = new();
    [SerializeField] private Transform _textParentObject;



    [Button]
    private void ChangeAllColors()
    {
        _timeLineGFX.ChangeColors();
        foreach (Material mat in _baseColorMaterials)
        {
            mat.SetColor("_Color", Palette.BasicColorDefault);
        }

        foreach (Material mat in _backgroundColorMaterials)
        {
            mat.SetColor("_Color", Palette.BackgroundColorDefault);
        }
    }
    [Button]
    private void ChangeAllColorsEditor()
    {
        foreach (VisualEffect vfx in _vfx)
        {
            vfx.ChangeVector4Parameter("Color", Palette.FillColorHDR);
        }
        ChangeTextColor(_textParentObject);
        
    }


    void ChangeTextColor(Transform parent)
    {
        // Loop through each child of the parent object
        foreach (Transform child in parent)
        {
            // Attempt to find a TextMeshPro (TMP_Text) component on the child
            TMP_Text textComponent = child.GetComponent<TMP_Text>();

            if (textComponent != null)
            {
                // Check if the child also has a LetterFiller component
                if (child.TryGetComponent<Letters.LetterFiller>(out var filler))
                {
                    // If it has LetterFiller, use unfillColor
                    textComponent.color = Palette.InteractableColorDefault;
                }
                else
                {
                    // Otherwise, use basicColor
                    textComponent.color = Palette.BasicColorDefault;
                }
            }

            if (child.childCount > 0)
            {
                ChangeTextColor(child);
            }
        }
    }

}
