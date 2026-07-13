using System;
using Letters;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KnifeDamage : MonoBehaviour
{
    // Event that will be triggered when a target is hit
    public event Action<Vector3> OnTargetHit;

    [Header("Damage Settings")]
    [SerializeField]
    private float baseDamage = 1f;

    [SerializeField]
    private BritneyInkManager inkManager;

    [SerializeField]
    private int inkCost = 1;

    [SerializeField]
    private bool damageScalesWithInk = true;

    [Header("Visual Settings")]
    [SerializeField]
    private Renderer knifeRenderer;

    [SerializeField]
    private Color startColor = Color.black;

    [SerializeField]
    private Color endColor = new Color(0.5f, 0, 0, 1); // Dark red

    [SerializeField]
    private string colorPropertyName = "_TextureColor";

    // [Header("Visual Feedback")]
    // [SerializeField]
    // private ParticleSystem hitEffect;

    // [SerializeField]
    // private AudioClip hitSound;

    private Collider2D weaponCollider;

    private void Reset()
    {
        inkManager = FindFirstObjectByType<BritneyInkManager>();
        knifeRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        weaponCollider.enabled = false;

        if (knifeRenderer == null)
            knifeRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (inkManager != null)
        {
            inkManager.OnInkChanged += UpdateKnifeColor;
            // Initialize color
            UpdateKnifeColor(inkManager.CurrentInkAmount);
        }
    }

    private void OnDisable()
    {
        if (inkManager != null)
            inkManager.OnInkChanged -= UpdateKnifeColor;
    }

    private void UpdateKnifeColor(int inkAmount)
    {
        if (knifeRenderer == null)
            return;

        // Calculate color based on ink level
        // When ink is at max (5), use endColor; otherwise blend between start and end
        float ratio = (float)inkAmount / inkManager.MaxInkAmount;
        Color currentColor = Color.Lerp(startColor, endColor, ratio);

        // Update material property
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        knifeRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(colorPropertyName, currentColor);
        knifeRenderer.SetPropertyBlock(propBlock);
    }

    public void EnableDamage()
    {
        weaponCollider.enabled = true;
    }

    public void DisableDamage()
    {
        weaponCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<HittableLetter>(out var target))
            return;

        if (inkManager.CanUseInk(inkCost))
        {
            inkManager.UseInk(inkCost);

            // Calculate damage amount
            float calculatedDamage = baseDamage;

            if (damageScalesWithInk)
            {
                float inkRatio = (float)inkManager.CurrentInkAmount / inkManager.MaxInkAmount;
                calculatedDamage = baseDamage * (inkRatio + 0.5f);
            }

            target.TakeDamage(calculatedDamage);

            // Trigger the hit event with the collision point
            OnTargetHit?.Invoke(collision.transform.position);

            // // Play hit effects
            // if (hitEffect != null)
            //     hitEffect.Play();

            // if (hitSound != null)
            //     AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}
