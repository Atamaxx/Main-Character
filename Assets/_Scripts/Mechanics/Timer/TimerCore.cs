using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Timer
{
    /// <summary>
    /// Responsible for running the timer based on TimerSettings
    /// and exposing events for timer start/tick/end,
    /// plus extra actions at specific timestamps.
    /// </summary>
    public class TimerCore : MonoBehaviour
    {
        [Header("Timer Configuration")]
        [SerializeField] private TimerSettings _settings;
        [SerializeField] private bool _autoStart = false;

        [Header("Events")]
        public UnityEvent OnTimerStart;
        public UnityEvent<float> OnTimerTick;  // Pass current time
        public UnityEvent OnTimerEnd;

        [Header("Time-based Actions")]
        [Tooltip("List of time-based actions that will trigger once the timer crosses the specified time.")]
        [SerializeField] private List<TimeAction> _timeActions = new List<TimeAction>();

        // Internal states
        private float _currentTime;
        private Coroutine _timerRoutine;
        private bool _isTimerRunning;

        // To ensure each time-based action only fires once
        private bool[] _hasFiredAction;

        private void Start()
        {
            // Initialize current time and the 'fired' flags
            _currentTime = _settings.StartTime;
            _hasFiredAction = new bool[_timeActions.Count];
            if (_autoStart)
            {
                StartTimer();
            }
        }

        public void StartTimer()
        {
            // In case it’s already running, stop it first
            StopTimer();

            _currentTime = _settings.StartTime;
            _isTimerRunning = true;
            OnTimerStart?.Invoke();

            // Reset the time-action flags
            for (int i = 0; i < _hasFiredAction.Length; i++)
            {
                _hasFiredAction[i] = false;
            }

            _timerRoutine = StartCoroutine(TimerRoutine());
        }

        /// <summary>
        /// Pauses the timer (if it’s running).
        /// </summary>
        public void PauseTimer()
        {
            if (_isTimerRunning && _timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
                _isTimerRunning = false;
            }
        }

        /// <summary>
        /// Resumes the timer if it was paused and hasn’t finished yet.
        /// </summary>
        public void ResumeTimer()
        {
            if (!_isTimerRunning)
            {
                _isTimerRunning = true;
                _timerRoutine = StartCoroutine(TimerRoutine());
            }
        }

        /// <summary>
        /// Stops and resets the timer to zero.
        /// </summary>
        public void StopTimer()
        {
            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
            }

            _timerRoutine = null;
            _currentTime = _settings.StartTime;
            _isTimerRunning = false;
            // No "end" event here, because we’re forcibly stopping
        }

        /// <summary>
        /// Returns the current time of the timer.
        /// Useful if another component wants to read it.
        /// </summary>
        public float GetCurrentTime()
        {
            return _currentTime;
        }
        public void SetCurrentTime(float time)
        {
            _currentTime = time;
        }

        /// <summary>
        /// The main coroutine that increments/decrements current time until done.
        /// </summary>
        private IEnumerator TimerRoutine()
        {
            while (true)
            {
                // Update timer
                if (_settings.TimerMode == TimerMode.CountDown)
                {
                    _currentTime -= Time.deltaTime;
                    if (_currentTime <= 0f)
                    {
                        _currentTime = 0f;
                        OnTimerTick?.Invoke(_currentTime);
                        break;
                    }
                }
                else // CountUp
                {
                    _currentTime += Time.deltaTime;
                    if (_currentTime >= _settings.TimerDuration)
                    {
                        _currentTime = _settings.TimerDuration;
                        OnTimerTick?.Invoke(_currentTime);
                        break;
                    }
                }

                // Send tick event
                OnTimerTick?.Invoke(_currentTime);

                // Check all time-based actions
                for (int i = 0; i < _timeActions.Count; i++)
                {
                    if (!_hasFiredAction[i]) // Only fire once
                    {
                        float trigger = _timeActions[i].TriggerTime;

                        // If counting down, fire when we go *below or equal* the trigger time.
                        // If counting up, fire when we go *above or equal* the trigger time.
                        bool triggerReached = false;
                        if (_settings.TimerMode == TimerMode.CountDown)
                            triggerReached = (_currentTime <= trigger);
                        else
                            triggerReached = (_currentTime >= trigger);

                        if (triggerReached)
                        {
                            _timeActions[i].OnTrigger?.Invoke();
                            _hasFiredAction[i] = true;
                        }
                    }
                }

                yield return null; // Wait for next frame
            }

            // Timer is done
            _isTimerRunning = false;
            _timerRoutine = null;
            OnTimerEnd?.Invoke();
        }
    }

    /// <summary>
    /// Holds each custom time-based action: 
    /// a time to trigger plus the UnityEvent to invoke.
    /// </summary>
    [System.Serializable]
    public class TimeAction
    {
        public float TriggerTime;
        public UnityEvent OnTrigger;
    }
}
