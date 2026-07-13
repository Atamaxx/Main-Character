using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Letters
{
    [System.Serializable]
    public class ConditionActionPair
    {
        [Header("Condition Setup")]
        public List<Condition> Conditions = new();

        [Header("Action Setup")]
        public List<Action> Actions = new();

        // Remember previous condition state
        public bool? PreviousConditionState = null;
    }

    public class ConditionActionSystem : MonoBehaviour
    {
        [Header("Condition → Action Pairs")]
        [SerializeField] private ConditionActionPair[] _pairs;

        public void CheckAndExecute()
        {
            if (_pairs == null || _pairs.Length == 0)
                return;

            foreach (var pair in _pairs)
            {
                if (pair.Conditions == null || pair.Conditions.Count == 0 ||
                    pair.Actions == null || pair.Actions.Count == 0)
                    continue;

                // Check if ALL conditions in this pair are met
                bool allConditionsMet = true;
                foreach (var cond in pair.Conditions)
                {
                    if (!cond.IsConditionMet())
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                // Skip execution if the condition state hasn't changed
                if (pair.PreviousConditionState.HasValue && pair.PreviousConditionState.Value == allConditionsMet)
                    continue;

                // Update the previous condition state
                pair.PreviousConditionState = allConditionsMet;

                // Execute or revert ALL actions based on conditions result
                if (allConditionsMet)
                {
                    foreach (var act in pair.Actions)
                    {
                        act.OnConditionMet();
                    }
                }
                else
                {
                    foreach (var act in pair.Actions)
                    {
                        act.OnConditionNotMet();
                    }
                }
            }
        }

        [Button]
        public void ExecuteOnMet()
        {
            foreach (var pair in _pairs)
            {
                foreach (var act in pair.Actions)
                {
                    act.OnConditionMet();
                }
            }
        }

        [Button]
        public void ExecuteOnNotMet()
        {
            foreach (var pair in _pairs)
            {
                foreach (var act in pair.Actions)
                {
                    act.OnConditionNotMet();
                }
            }
        }
    }
}
