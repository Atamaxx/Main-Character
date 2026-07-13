using System.Collections;
using UnityEngine;

public class TransformShaker : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeFrequency = 2f; // Controls how quickly the noise changes.
    [SerializeField] private bool enableRotationShake = false;
    [SerializeField] private float rotationMagnitude = 10f;
    [SerializeField] private bool autoStart = true;

    [Header("Position Shake Options")]
    [Tooltip("If true, the shake will use the specified bounds rather than a symmetric magnitude.")]
    [SerializeField] private bool _useShakeBounds = false;
    [SerializeField] private Vector2 shakeBoundsMin = new Vector2(-0.2f, -0.2f);
    [SerializeField] private Vector2 shakeBoundsMax = new Vector2(0.2f, 0.2f);
    [Tooltip("Used only when 'Use Shake Bounds' is false.")]
    [SerializeField] private float shakeMagnitude = 0.2f;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _shakeCoroutine;


    private void Awake()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }
    void OnEnable()
    {
        if (autoStart)
        {
            StartShaking();
        }
    }

    private void OnDisable()
    {
        StopShaking();
    }

    /// <summary>
    /// Starts the endless shake effect.
    /// </summary>
    public void StartShaking()
    {
        if (_shakeCoroutine == null)
        {
            _shakeCoroutine = StartCoroutine(ShakeRoutine());
        }
    }

    /// <summary>
    /// Stops the shake effect and resets the transform.
    /// </summary>
    public void StopShaking()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
            //ResetTransform();
        }
    }

    public void UseShakeBounds(bool useBounds)
    {
        _useShakeBounds = useBounds;
    }

    /// <summary>
    /// Resets the transform to its original position and rotation.
    /// </summary>
    // private void ResetTransform()
    // {
    //     transform.position = _originalPosition;
    //     transform.rotation = _originalRotation;
    // }

    /// <summary>
    /// Coroutine that applies a smooth, endless shaking effect using Perlin noise.
    /// </summary>
    private IEnumerator ShakeRoutine()
    {
        float noiseSeed = Random.Range(0f, 1000f);
        while (true)
        {
            float t = Time.time;

            // Generate smooth noise values in the range [-1, 1].
            float noiseX = Mathf.PerlinNoise(noiseSeed + t * shakeFrequency, 0f) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(noiseSeed + t * shakeFrequency, 1f) * 2f - 1f;
            Vector3 offset;

            if (_useShakeBounds)
            {
                // Map the noise values from [-1, 1] to the specified bounds.
                float offsetX = Mathf.Lerp(shakeBoundsMin.x, shakeBoundsMax.x, (noiseX + 1f) / 2f);
                float offsetY = Mathf.Lerp(shakeBoundsMin.y, shakeBoundsMax.y, (noiseY + 1f) / 2f);
                offset = new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                // Use a symmetric offset based on shakeMagnitude.
                offset = new Vector3(noiseX, noiseY, 0f) * shakeMagnitude;
            }

            transform.position = _originalPosition + offset;

            if (enableRotationShake)
            {
                float noiseRot = Mathf.PerlinNoise(noiseSeed + t * shakeFrequency, 2f) * 2f - 1f;
                transform.rotation = Quaternion.Euler(0f, 0f, noiseRot * rotationMagnitude);
            }
            else
            {
                transform.rotation = _originalRotation;
            }

            yield return null; // Update every frame for a smooth effect.
        }
    }
}
