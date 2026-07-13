using UnityEngine;

namespace Timer
{
    [System.Serializable]
    public class TimerSettings
    {
        [Header("Timer Settings")]
        public TimerMode TimerMode = TimerMode.CountDown;
        public float TimerDuration = 15f;
        public float StartTime = 15f;
    }
}
