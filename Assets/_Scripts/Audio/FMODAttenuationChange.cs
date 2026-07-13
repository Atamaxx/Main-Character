using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAttenuationChange : MonoBehaviour
{
    // Reference to the StudioEventEmitter component
    [SerializeField] private StudioEventEmitter _emitter;

    private float _initialMinDistance = 1.0f;
    private float _initialMaxDistance = 20.0f;

    private float _currentMinDistance;
    private float _currentMaxDistance;

    private void Start()
    {
        EventInstance eventInstance = _emitter.EventInstance;
        eventInstance.getProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, out _initialMinDistance);
        eventInstance.getProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, out _initialMaxDistance);

        _currentMinDistance = _initialMinDistance;
        _currentMaxDistance = _initialMaxDistance;
    }

    /// <summary>
    /// Sets the attenuation minimum and maximum distances for the FMOD event.
    /// </summary>
    /// <param name="min">Minimum distance for attenuation.</param>
    /// <param name="max">Maximum distance for attenuation.</param>
    public void SetAttenuation(float min, float max)
    {
        // Ensure min is less than max
        if (min >= max)
        {
            Debug.LogWarning("minDistance should be less than maxDistance.");
            return;
        }

        // Update the serialized fields (optional)
        _currentMinDistance = min;
        _currentMaxDistance = max;

        // Get the EventInstance from the emitter
        EventInstance eventInstance = _emitter.EventInstance;

        // Set the minimum distance
        eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, min);

        // Set the maximum distance
        eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, max);
    }

    public void MultiplyAttenuation(float attenutationMul)
    {
        SetAttenuation(_currentMinDistance * attenutationMul, _currentMaxDistance * attenutationMul);
    }

    public void SetMaxAttenuation(float attenutationMax)
    {
        SetAttenuation(_currentMinDistance, attenutationMax);
    }

    public void SetMinAttenuation(float attenutationMin)
    {
        SetAttenuation(attenutationMin, _currentMaxDistance);
    }

    public void ResetAttenuation()
    {
        SetAttenuation(_initialMinDistance, _initialMaxDistance);
    }
}
