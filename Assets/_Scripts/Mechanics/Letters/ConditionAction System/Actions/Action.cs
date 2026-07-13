using UnityEngine;
namespace Letters
{
    public abstract class Action : MonoBehaviour
    {
        /// <summary>
        /// Called when the condition is met.
        /// </summary>
        public abstract void OnConditionMet();

        /// <summary>
        /// Called when the condition is not met (for “undo”).
        /// </summary>
        public abstract void OnConditionNotMet();
    }


}