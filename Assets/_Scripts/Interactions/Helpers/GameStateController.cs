using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides methods for changing game states that can be called from Unity Events
/// </summary>
public class GameStateController : MonoBehaviour
{
    [Header("Response Events")]
    [SerializeField]
    private UnityEvent onMainMenu;

    [SerializeField]
    private UnityEvent onGameplay;

    [SerializeField]
    private UnityEvent onPause;

    [SerializeField]
    private UnityEvent onLevelLoading;

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                onMainMenu?.Invoke();
                break;
            case GameState.Gameplay:
                onGameplay?.Invoke();
                break;
            case GameState.Pause:
                onPause?.Invoke();
                break;
            case GameState.LevelLoading:
                onLevelLoading?.Invoke();
                break;
        }
    }

    public void OnLevelTriggerEntered()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelTriggerEntered();
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void ReturnToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    public void CompleteLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.LevelCompletion);
        }
    }

    public void SelectLevel(string levelName)
    {
        if (GameManager.Instance?.GetLevelLoader() != null)
        {
            // Set the selected level as an override
            GameManager.Instance.GetLevelLoader().SetSelectedLevel(levelName);
        }
    }

    public void LoadSelectedLevel()
    {
        if (GameManager.Instance != null)
        {
            // This will use the override if one is set
            GameManager.Instance.LoadNextAvailableLevel();
        }
    }
}
