using NaughtyAttributes;
using UnityEngine;
namespace Britney
{

    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Britney/SoundSettings")]
    public class SoundSettings : ScriptableObject
    {
        [BoxGroup("MUlTIPLIERS")] public float VolumeMultiplier = 1f;
        // [BoxGroup("MUlTIPLIERS")] public float SpeedDownMultiplier = 1.5f;
        // [BoxGroup("MUlTIPLIERS")] public float SpeedUpMultiplier = 0.5f;
        [BoxGroup("STATIC")] public float VolumeWhenStatic = 0f;
        [BoxGroup("STATIC")] public float EaseOffDuration = 0.1f;
        [BoxGroup("WALKING")] public float WalkingVolumeBase = 0.05f;
        [BoxGroup("WALKING")] public Vector2 WalkingVolumePlusRange = new(0.2f, 0.35f);
        [BoxGroup("WALKING")] public float StepSoundDuration = 0.1f;
        [BoxGroup("AIR")] public Vector2 AirSpeedInterval = new(0f, 0.1f);
        [BoxGroup("AIR")] public Vector2 AirVolumeInterval = new(0f, 0.6f);


    }
}
