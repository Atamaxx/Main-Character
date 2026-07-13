using NaughtyAttributes;
using UnityEngine;
namespace Britney
{

    [CreateAssetMenu(fileName = "VFXSettings", menuName = "Britney/VFXSettings")]
    public class VFXSettings : ScriptableObject
    {
        [BoxGroup("VFX Head"), SerializeField] public string ScaleParam = "ScaleVector";
        [BoxGroup("VFX Head"), SerializeField] public Vector2 HeadVelocityMul = new(-0.5f, 0.5f);
        [BoxGroup("VFX Head"), SerializeField] public float HeadYVelocityOnStatic = 0.25f;

        [BoxGroup("VFX Body"), SerializeField] public string ColorParam = "Color";
        [BoxGroup("VFX Body"), SerializeField] public Color FilledColor = new(0.0707547f, 0, 0, 0);
        [BoxGroup("VFX Body"), SerializeField] public Color UnfilledColor = new(0, 0, 0, 0);
        [BoxGroup("VFX Body"), SerializeField] public float ColorTransitionDuration = 0.5f;

        [BoxGroup("VFX Aura"), SerializeField] public string SpawnRateParam = "SpawnRate";
    }
}
