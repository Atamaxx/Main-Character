using System;
using UnityEngine;
using UnityEngine.Events;

namespace Letters
{
    public class EventAction : Action
    {
        [SerializeField]
        private UnityEvent _onConditionMet;

        [SerializeField]
        private UnityEvent _onConditionNotMet;

        public override void OnConditionMet()
        {
            _onConditionMet.Invoke();
        }

        public override void OnConditionNotMet()
        {
            _onConditionNotMet.Invoke();
        }
    }
}
