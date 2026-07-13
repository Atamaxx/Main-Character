using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAmputationKnife : MonoBehaviour
{
    [SerializeField] private Material _knifeMaterial;
    [SerializeField] private List<TextMeshPro> _textForCutting = new();
    [SerializeField] private Color _startingTexColor = Color.white;
    [SerializeField] private Color _finalTextColor = Color.red;
    [SerializeField] private string letterSortingLayer = "Letters"; // Desired sorting layer.
    [SerializeField] private int maxCuts = 10; // Maximum cuts for color transition.

    private int _numberOfCuts;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            Debug.LogError("Rigidbody2D not found on " + gameObject.name);
        }
    }

    // Called when entering a trigger collider.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D triggered with " + other.gameObject.name);

        // Check for TextMeshPro on the collided object.
        TextMeshPro tmp = other.GetComponent<TextMeshPro>();
        if (tmp == null)
        {
            Debug.Log("No TextMeshPro component found on " + other.gameObject.name);
            return;
        }
        if (!_textForCutting.Contains(tmp))
        {
            Debug.Log("TextMeshPro " + tmp.name + " is not in the list for cutting.");
            return;
        }

        // Get contact point approximation.
        Vector2 contactPoint = other.ClosestPoint(transform.position);
        // Use the knife's velocity magnitude as an approximation of impact depth.
        float impactDepth = _rb != null ? _rb.linearVelocity.magnitude : 1f;
        Debug.Log("Contact Point: " + contactPoint + " | Impact Depth: " + impactDepth);

        CutText(tmp, contactPoint, impactDepth);
    }

    private void CutText(TextMeshPro tmp, Vector2 contactPoint, float impactDepth)
    {
        if (string.IsNullOrEmpty(tmp.text))
        {
            Debug.Log("TextMeshPro " + tmp.name + " has no text left to cut.");
            return;
        }

        // Remove and retrieve the last character.
        int lastIndex = tmp.text.Length - 1;
        char lastChar = tmp.text[lastIndex];
        tmp.text = tmp.text.Substring(0, lastIndex);
        Debug.Log("Cut letter: " + lastChar);

        // Create a new GameObject for the separated letter.
        GameObject letterObj = new GameObject("Letter_" + lastChar);
        letterObj.transform.position = contactPoint;

        // Add a TextMeshPro component and copy the styling.
        TextMeshPro letterTMP = letterObj.AddComponent<TextMeshPro>();
        letterTMP.text = lastChar.ToString();
        letterTMP.font = tmp.font;
        letterTMP.fontSize = tmp.fontSize;
        letterTMP.color = tmp.color;

        // Attempt to set the sorting layer if a MeshRenderer is available.
        MeshRenderer renderer = letterObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = letterSortingLayer;
        }
        else
        {
            Debug.LogWarning("MeshRenderer not found on letter object " + letterObj.name);
        }

        // Add physics components.
        Rigidbody2D rb = letterObj.AddComponent<Rigidbody2D>();
        CircleCollider2D collider = letterObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = false;
        collider.radius = 0.5f; // Adjust as needed.

        // Rotate the letter based on the impact depth.
        float rotationAngle = Mathf.Clamp(impactDepth * 2f, -15f, 15f);
        rotationAngle *= (Random.value > 0.5f ? 1f : -1f);
        letterObj.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

        // Update cut counter and knife material color.
        _numberOfCuts++;
        float t = Mathf.Clamp01((float)_numberOfCuts / maxCuts);
        Color newColor = Color.Lerp(_startingTexColor, _finalTextColor, t);
        _knifeMaterial.SetColor("_TextureColor", newColor);

        Debug.Log("Cut count: " + _numberOfCuts + " | New Knife Color: " + newColor);
    }
}
