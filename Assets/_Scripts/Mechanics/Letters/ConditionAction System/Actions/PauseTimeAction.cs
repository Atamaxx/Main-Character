using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Letters
{
    public class PauseTimeAction : Action
    {
        [SerializeField] private bool _pause = true;
        [SerializeField] private List<TimeLine> _timeLines = new();

        public override void OnConditionMet()
        {
            if (_pause)
            {
                PauseTime();
            }
            else
            {
                ResumeTime();
            }
        }

        public override void OnConditionNotMet()
        {
            if (_pause)
            {
                ResumeTime();
            }
            else
            {
                PauseTime();
            }
        }

        private void PauseTime()
        {
            foreach (TimeLine timeLine in _timeLines)
            {
                timeLine.OnStopped();
            }
        }

        private void ResumeTime()
        {
            foreach (TimeLine timeLine in _timeLines)
            {
                timeLine.OnResumed();
            }
        }
    }
}
