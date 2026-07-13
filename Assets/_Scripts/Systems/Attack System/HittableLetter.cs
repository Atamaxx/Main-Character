using System.Collections;
using Letters;
using UnityEngine;
using UnityEngine.Events;

public class HittableLetter : StatefulMonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    private float _maxHealth = 3f;

    [SerializeField]
    private float _currentHealth;

    [Header("Ink Settings")]
    [SerializeField]
    private int _inkRestoreAmount = 1;

    [SerializeField]
    private Transform _inkExposionParticles;

    [SerializeField]
    private ParticleSystem _inkExposionParticlesSystem;

    [Header("Hit Behavior")]
    [SerializeField]
    private bool _rotateWhenHit = true;

    [SerializeField]
    private Vector2 _rotationAmountRange = new(-10f, 10f);

    // [Header("Death Effects")]
    // [SerializeField]
    // private GameObject deathEffectPrefab;

    // [SerializeField]
    // private AudioClip hitSound;

    // [SerializeField]
    // private AudioClip deathSound;

    [SerializeField]
    private UnityEvent onDieEvent;

    [SerializeField]
    private BritneyInkManager inkManager;

    private void Reset()
    {
        inkManager = FindFirstObjectByType<BritneyInkManager>();
    }

    protected override void OnStatefulAwake()
    {
        // Initialize health if we're not restoring from a saved state
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;

        // Play hit sound
        // if (hitSound != null)
        //     AudioSource.PlayClipAtPoint(hitSound, transform.position);

        // Create ink splatter effect
        // if (inkSplatterPrefab != null)
        //     Instantiate(inkSplatterPrefab, transform.position, Quaternion.identity);


        // Rotate when hit
        if (_rotateWhenHit)
            RotateEffect();

        // Check if destroyed
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void RotateEffect()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation =
            currentRotation
            * Quaternion.Euler(0, 0, Random.Range(_rotationAmountRange.x, _rotationAmountRange.y));
        transform.rotation = targetRotation;
    }

    private void Die()
    {
        // Play death sound
        // if (deathSound != null)
        //     AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Restore ink to the player
        if (inkManager != null && _inkRestoreAmount > 0)
            inkManager.RestoreInk(_inkRestoreAmount);

        // death effect
        _inkExposionParticles.position = transform.position;
        _inkExposionParticlesSystem.Play();

        onDieEvent?.Invoke();
    }

    // Implementation of required IStateful interface methods
    public override object CaptureState()
    {
        // Return current health as the state to save
        return _currentHealth;
    }

    public override void RestoreState(object state)
    {
        // Restore currentHealth from the saved state
        if (state is float savedHealth)
        {
            _currentHealth = savedHealth;
        }
    }
}
