using UnityEngine;

namespace Letters
{

    public abstract class Condition : MonoBehaviour
    {
        /// <summary>
        /// Returns whether this condition is currently met.
        /// </summary>
        public abstract bool IsConditionMet();
    }
}