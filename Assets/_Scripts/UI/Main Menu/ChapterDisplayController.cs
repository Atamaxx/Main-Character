using UnityEngine;
using System.Collections;
using Systems.Persistence;

public class ChapterDisplayController : MonoBehaviour
{
    [System.Serializable]
    public class ChapterButton
    {
        public string levelName;
        public TextButton textButton;
    }

    [SerializeField] private ChapterButton[] chapterButtons;
    [SerializeField] private float updateDelay = 0.2f;

    private void OnEnable()
    {
        StartCoroutine(DelayedUpdate());
    }
    private IEnumerator DelayedUpdate()
    {
        yield return new WaitForSeconds(updateDelay);
        UpdateChapterIndicators();
    }

    public void UpdateChapterIndicators()
    {
        if (GameSession.Instance == null) return;

        foreach (var button in chapterButtons)
        {
            if (string.IsNullOrEmpty(button.levelName) || button.textButton == null) continue;

            bool isCompleted = GameSession.Instance.IsLevelCompleted(button.levelName);

            if (!string.IsNullOrEmpty(button.textButton.OriginalText) && button.textButton.OriginalText.Length > 1)
            {
                char indicator = isCompleted ? '+' : '-';
                button.textButton.OriginalText = indicator + button.textButton.OriginalText[1..];
                button.textButton.ChangeState(ButtonState.Normal);
            }
        }
    }
}