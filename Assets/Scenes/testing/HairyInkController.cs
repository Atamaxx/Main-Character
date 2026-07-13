using TMPro;
using UnityEngine;

/// <summary>
/// Drives the “hairy ink” parameters on the TMP material.
/// Clones the material at runtime so changes stay local to this object.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class HairyInkController : MonoBehaviour
{
    // Inspector sliders ---------------------------------------------------
    [Header("Static look")]
    [Range(1f, 200f)]
    public float noiseScale = 60f;

    [Range(0f, 0.10f)]
    public float strandThinness = 0.03f;

    [Range(0f, 0.10f)]
    public float bleed = 0.03f;

    [Header("Animation")]
    public bool pulseSpeed = true;

    [Range(0f, 2f)]
    public float baseSpeed = 0.15f;

    [Range(0f, 2f)]
    public float pulseAmplitude = 0.05f;

    [Range(0.1f, 5f)]
    public float pulsePeriod = 3f;

    // ---------------------------------------------------------------------
    static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");
    static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
    static readonly int StrandThinID = Shader.PropertyToID("_StrandThinness");
    static readonly int BleedID = Shader.PropertyToID("_Bleed");

    Material runtimeMat;

    void Awake()
    {
        // Duplicate the material so each label can have independent settings
        TMP_Text tmp = GetComponent<TMP_Text>();
        runtimeMat = Instantiate(tmp.fontMaterial);
        tmp.fontMaterial = runtimeMat;
    }

    void Update()
    {
        float speed = baseSpeed;
        if (pulseSpeed)
            speed += Mathf.Sin(Time.time * (2 * Mathf.PI / pulsePeriod)) * pulseAmplitude;

        runtimeMat.SetFloat(NoiseScaleID, noiseScale);
        runtimeMat.SetFloat(NoiseSpeedID, speed);
        runtimeMat.SetFloat(StrandThinID, strandThinness);
        runtimeMat.SetFloat(BleedID, bleed);
    }
}
