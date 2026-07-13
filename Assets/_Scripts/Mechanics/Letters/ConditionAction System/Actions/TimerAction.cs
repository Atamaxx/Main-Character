using UnityEngine;
using Timer;

namespace Letters
{
    /// <summary>
    /// Ties the ConditionActionSystem hooks to a TimerCore instance
    /// and handles whether we restart/stop/pause based on condition changes.
    /// </summary>
    public class TimerAction : Action
    {

        [Header("References")]
        [SerializeField] private TimerCore _timerCore;
        [Header("Behavior")]
        [SerializeField] private bool _runTimer = true;
        [Tooltip("If true, then every time conditions become met, the timer restarts.")]
        [SerializeField] private bool _autoRestartOnConditionMet = true;

        [Tooltip("If true, OnConditionNotMet just pauses the timer. If false, it fully stops (resets).")]
        [SerializeField] private bool _pauseOnConditionNotMet = false;

        public override void OnConditionMet()
        {
            if (_timerCore == null) return;
            if (_runTimer)
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }
        }

        public override void OnConditionNotMet()
        {
            if (_timerCore == null) return;

            if (_runTimer)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }

        private void StartTimer()
        {
            if (_autoRestartOnConditionMet)
            {
                _timerCore.StartTimer();
            }
            else
            {
                _timerCore.ResumeTimer();
            }
        }

        private void StopTimer()
        {
            if (_pauseOnConditionNotMet)
            {
                _timerCore.PauseTimer();
            }
            else
            {
                _timerCore.StopTimer();
            }
        }
    }
}
