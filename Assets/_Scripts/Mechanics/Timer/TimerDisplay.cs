using UnityEngine;
using TMPro;
using Letters;

namespace Timer
{
    /// <summary>
    /// Responsible for displaying the current time of a TimerCore
    /// in a specific format.
    /// </summary>
    public class TimerDisplay : MonoBehaviour
    {
        [SerializeField] private TimerCore _timerCore;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private ShowTimerMode _showTimerMode = ShowTimerMode.Seconds;

        private void OnEnable()
        {
            // If TimerCore is assigned, subscribe to tick events
            if (_timerCore != null)
            {
                _timerCore.OnTimerTick.AddListener(UpdateTimerText);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe to avoid memory leaks
            if (_timerCore != null)
            {
                _timerCore.OnTimerTick.RemoveListener(UpdateTimerText);
            }
        }

        private void UpdateTimerText(float currentTime)
        {
            if (_timerText == null) return;

            switch (_showTimerMode)
            {
                case ShowTimerMode.Seconds:
                    _timerText.text = $"{Mathf.CeilToInt(currentTime)}";
                    break;

                case ShowTimerMode.Seconds_Milliseconds:
                    _timerText.text = $"{currentTime:F2}";
                    break;

                case ShowTimerMode.Minutes:
                    int totalMinutes = Mathf.CeilToInt(currentTime / 60f);
                    _timerText.text = $"{totalMinutes}";
                    break;

                case ShowTimerMode.Minutes_Seconds:
                    int minutes = Mathf.FloorToInt(currentTime / 60f);
                    int seconds = Mathf.FloorToInt(currentTime % 60f);
                    _timerText.text = $"{minutes}:{seconds}";
                    break;

                case ShowTimerMode.Hours:
                    int totalHours = Mathf.CeilToInt(currentTime / 3600f);
                    _timerText.text = $"{totalHours}";
                    break;

                case ShowTimerMode.Hours_Minutes:
                    int hours = Mathf.FloorToInt(currentTime / 3600f);
                    int remainingMinutes = Mathf.FloorToInt((currentTime % 3600f) / 60f);
                    _timerText.text = $"{hours}:{remainingMinutes}";
                    break;

                default:
                    break;
            }
            
            if (_timerText.TryGetComponent(out LetterFiller filler))
            {
                filler.RefillLetters();
            }
        }
    }
}
