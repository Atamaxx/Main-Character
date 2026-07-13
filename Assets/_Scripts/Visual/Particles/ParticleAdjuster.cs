using UnityEngine;

public class ParticleAdjuster : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private int _minParticles = 10;
    [SerializeField] private int _maxParticles = 100;

    private ParticleSystem.EmissionModule emissionModule;

    void Start()
    {
        if (_particleSystem == null)
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        if (_particleSystem != null)
        {
            emissionModule = _particleSystem.emission;
        }
        else
        {
            Debug.LogError("ParticleSystem is not assigned or found on the GameObject.");
        }
    }


    public void UpdateParticleEmission(float value, float maxValue)
    {
        if (_particleSystem == null) return;
        float percent = Mathf.Clamp01(value / maxValue);
        print(percent);
        int particleCount = Mathf.RoundToInt(Mathf.Lerp(_minParticles, _maxParticles, percent));
        emissionModule.rateOverTime = particleCount;
    }
}
