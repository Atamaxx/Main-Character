using UnityEngine;

namespace Britney
{

    [CreateAssetMenu(fileName = "ManagerSettings", menuName = "Britney/ManagerSettings")]
    public class ManagerSettings : ScriptableObject
    {
       public MovementSettings MovementParams;
       public SoundSettings SoundParams;
       public AnimationSettings AnimationParams;
       public VFXSettings VFXParams;
    }
}
