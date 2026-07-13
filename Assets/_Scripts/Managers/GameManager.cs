using System;
using System.Collections;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentSingleton<GameManager>
{
    public static event Action<GameState> OnStateChanged;
    public GameState State { get; private set; }

    [SerializeField]
    private LevelLoader _levelLoader;

    [SerializeField]
    private bool _loadSaveOnStart = true;
    private bool _isTransitioningState = false;
    private GameState _previousState;
    private string _currentSecretLevelToLoad; // To store the name of the secret level to load

    void Start()
    {
        InitializeGame();
    }

    private void OnEnable()
    {
        if (_levelLoader != null)
        {
            _levelLoader.OnLevelLoaded += HandleLevelLoaded;
        }
    }

    private void OnDisable()
    {
        if (_levelLoader != null)
        {
            _levelLoader.OnLevelLoaded -= HandleLevelLoaded;
        }
    }

    private void InitializeGame()
    {
        // Ensure time scale is normal at the very beginning
        Time.timeScale = 1f;
        if (_loadSaveOnStart && SaveLoadSystem.Instance != null)
        {
            try
            {
                SaveLoadSystem.Instance.LoadGame();
                string savedLevel = SaveLoadSystem.Instance.gameData.CurrentLevelName;

                if (!string.IsNullOrEmpty(savedLevel) && savedLevel != "MainMenu")
                {
                    ChangeState(GameState.MainMenu);
                    _levelLoader.LoadLevelByName(savedLevel);
                }
                else
                {
                    ChangeState(GameState.MainMenu);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"No save found: {e.Message}. Starting new game.");
                ChangeState(GameState.MainMenu);
            }
        }
        else
        {
            ChangeState(GameState.MainMenu);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (_isTransitioningState && State != GameState.LevelCompletionSecret) // Allow re-entry for specific cases if needed, otherwise prevent rapid changes.
            return;

        _isTransitioningState = true;
        _previousState = State;
        State = newState;

        Debug.Log($"Attempting to change state from {_previousState} to: {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.Starting:
                HandleStarting();
                break;
            case GameState.Gameplay:
                HandleGameplay();
                break;
            case GameState.Pause:
                HandlePause();
                break;
            case GameState.LevelCompletion:
                HandleLevelCompletion();
                break;
            case GameState.LevelCompletionSecret:
                HandleLevelCompletionSecret();
                break;
            case GameState.LevelLoading:
                HandleLevelLoading();
                break;
        }

        OnStateChanged?.Invoke(newState);
        _isTransitioningState = false;

        Debug.Log($"Game state changed to: {newState}");
    }

    private void HandleMainMenu()
    {
        Time.timeScale = 1f; // Ensure game is not frozen in main menu
        EnablePlayerControls(); // Or enable menu navigation controls
        if (
            _previousState == GameState.LevelCompletion
            || _previousState == GameState.LevelCompletionSecret
        )
        {
            var menuController = MenuController.Instance;
            if (menuController != null)
            {
                // Using a coroutine or ExecuteAfterFrames for delayed UI operations can be safer
                // if menu setup relies on things that happen across a frame or two.
                this.ExecuteAfterFrames(
                    4, // Adjust frame count as needed
                    () =>
                    {
                        var chaptersGroup = menuController.FindNavigationGroup("Chapter Group");
                        if (chaptersGroup != null)
                        {
                            menuController.SetActiveGroup(chaptersGroup);
                        }
                    }
                );
            }
        }
    }

    private void HandleLevelLoading()
    {
        // Player controls should be disabled here.
        // Time.timeScale remains 1f to allow for loading animations if any.
        // HandleStarting will ensure Time.timeScale is 1f when the level is ready.
        StartCoroutine(DelayedLevelLoad());
    }

    private IEnumerator DelayedLevelLoad()
    {
        yield return null; // Wait a frame for any immediate screen updates or transitions
        if (_levelLoader != null)
        {
            _levelLoader.LoadNextAvailableLevel();
        }
        else
        {
            Debug.LogError("LevelLoader is null in DelayedLevelLoad.");
        }
    }

    private void HandleStarting()
    {
        Time.timeScale = 1f; // Unfreeze/ensure game is running when new level starts
        // Player controls are enabled in HandleGameplay, which is called next.
        if (FlipLevelPage.Instance != null)
        {
            FlipLevelPage.Instance.SetUpPageFlip();
            FlipLevelPage.Instance.StartPageFlip(); // This might trigger animations
        }
        // Transition to gameplay after initial setup for the level.
        ChangeState(GameState.Gameplay);
    }

    private void HandleGameplay()
    {
        EnablePlayerControls();
        Time.timeScale = 1f; // Ensure game is running
    }

    private void HandlePause()
    {
        DisablePlayerControls();
        Time.timeScale = 0f; // Freeze game on pause (this is a deliberate full stop)
    }

    private void HandleLevelCompletion()
    {
        DisablePlayerControls(); // Player cannot move
        // Time.timeScale remains 1f to allow for end-of-level animations/UI.
        Debug.Log("Handling level completion. Player controls disabled. Time.timeScale = 1f.");

        string currentLevel = SceneManager.GetActiveScene().name;
        GameSession gameSession = GameSession.Instance;
        if (gameSession != null)
        {
            gameSession.MarkLevelCompleted(currentLevel);
            gameSession.SaveGame();
        }
        else
        {
            Debug.LogError("GameSession is null in HandleLevelCompletion.");
        }

        if (_levelLoader != null)
        {
            _levelLoader.LoadNextAvailableLevel(); // This will eventually lead to HandleLevelLoaded -> HandleStarting
        }
        else
        {
            Debug.LogError("LevelLoader is null in HandleLevelCompletion.");
        }
    }

    public void CompleteSecretLevel(string specifiedSecretLevelName)
    {
        _currentSecretLevelToLoad = specifiedSecretLevelName;
        ChangeState(GameState.LevelCompletionSecret);
    }

    private void HandleLevelCompletionSecret()
    {
        DisablePlayerControls(); // Player cannot move
        // Time.timeScale remains 1f.
        Debug.Log(
            "Handling secret level completion. Player controls disabled. Time.timeScale = 1f."
        );

        string currentLevelName = SceneManager.GetActiveScene().name;
        GameSession gameSession = GameSession.Instance;
        LevelLoader levelLoader = GetLevelLoader();

        if (gameSession == null)
        {
            Debug.LogError("GameSession instance not found during secret level completion.");
            ReturnToMainMenu(); // Fallback
            return;
        }
        if (levelLoader == null)
        {
            Debug.LogError("LevelLoader instance not found during secret level completion.");
            ReturnToMainMenu(); // Fallback
            return;
        }

        gameSession.MarkLevelCompleted(currentLevelName);
        Debug.Log($"Current level '{currentLevelName}' marked as completed.");

        string secretLevelToLoad = _currentSecretLevelToLoad;

        if (string.IsNullOrEmpty(secretLevelToLoad))
        {
            secretLevelToLoad = levelLoader.GetSecretLevelForMain(currentLevelName);
            Debug.Log($"Derived secret level for '{currentLevelName}': '{secretLevelToLoad}'");
        }

        if (!string.IsNullOrEmpty(secretLevelToLoad))
        {
            gameSession.DiscoverSecretLevel(secretLevelToLoad);
            Debug.Log($"Secret level '{secretLevelToLoad}' discovered.");
            gameSession.SaveGame();
            Debug.Log($"Game saved. Loading secret level: {secretLevelToLoad}");
            levelLoader.LoadLevelByName(secretLevelToLoad); // Leads to HandleLevelLoaded -> HandleStarting
        }
        else
        {
            Debug.LogWarning(
                $"No secret level found or specified for current level '{currentLevelName}'. Returning to main menu as fallback."
            );
            gameSession.SaveGame();
            ReturnToMainMenu();
        }
        _currentSecretLevelToLoad = null; // Clear the stored name
    }

    private void HandleLevelLoaded()
    {
        // This is called by LevelLoader after a new scene is loaded.
        // Time.timeScale should be 1f by this point or will be set by the subsequent state.
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            ChangeState(GameState.MainMenu); // HandleMainMenu will ensure Time.timeScale = 1f
        }
        else
        {
            ChangeState(GameState.Starting); // HandleStarting will ensure Time.timeScale = 1f and then Gameplay
        }
    }

    public void OnLevelTriggerEntered()
    {
        if (State == GameState.LevelLoading)
            return;

        Debug.Log("Level trigger entered, changing to LevelLoading state");
        ChangeState(GameState.LevelLoading);
    }

    public void LoadNextAvailableLevel()
    {
        // This is a general utility. If called directly, ensure controls are disabled and state is appropriate.
        // Typically, HandleLevelCompletion or HandleLevelCompletionSecret will manage this.
        Debug.Log("LoadNextAvailableLevel called directly. Changing state to LevelLoading.");
        DisablePlayerControls(); // Ensure controls are off before loading
        ChangeState(GameState.LevelLoading);
    }

    public void ReturnToMainMenu()
    {
        DisablePlayerControls(); // Ensure controls are off
        if (SaveLoadSystem.Instance != null)
        {
            SaveLoadSystem.Instance.SaveGame();
        }
        // Time.timeScale will be set to 1f by HandleMainMenu after level load
        if (_levelLoader != null)
        {
            _levelLoader.LoadLevelByName("MainMenu");
        }
        else
        {
            Debug.LogError(
                "LevelLoader is null, cannot return to MainMenu. Attempting direct scene load."
            );
            SceneManager.LoadScene("MainMenu"); // Fallback direct load
            // Explicitly set Time.timeScale if going directly and bypassing normal flow.
            // However, HandleLevelLoaded -> MainMenu state should handle this.
            // For safety, if we are here due to an error, ensure time is running.
            Time.timeScale = 1f;
        }
    }

    public void ResumeGame()
    {
        if (State == GameState.Pause)
        {
            // HandleGameplay will set Time.timeScale = 1f and enable controls.
            ChangeState(GameState.Gameplay);
        }
    }

    public void PauseGame()
    {
        if (State == GameState.Gameplay)
        {
            // HandlePause will set Time.timeScale = 0f and disable controls.
            ChangeState(GameState.Pause);
        }
    }

    private void EnablePlayerControls()
    {
        if (
            Britney.BritneyManager.Instance != null
            && Britney.BritneyManager.Instance.BritneyMovement != null
        )
        {
            Britney.BritneyManager.Instance.BritneyMovement.EnableControls();
            Debug.Log("Player controls ENABLED.");
        }
        else
        {
            Debug.LogWarning(
                "BritneyManager or BritneyMovement not found when trying to enable controls."
            );
        }
    }

    private void DisablePlayerControls()
    {
        if (
            Britney.BritneyManager.Instance != null
            && Britney.BritneyManager.Instance.BritneyMovement != null
        )
        {
            Britney.BritneyManager.Instance.BritneyMovement.DisableControls();
            Debug.Log("Player controls DISABLED.");
        }
        else
        {
            Debug.LogWarning(
                "BritneyManager or BritneyMovement not found when trying to disable controls."
            );
        }
    }

    public LevelLoader GetLevelLoader()
    {
        return _levelLoader;
    }
}

[Serializable]
public enum GameState
{
    MainMenu = 0,
    Starting = 1,
    Gameplay = 2,
    Pause = 3,
    LevelCompletion = 4,
    LevelLoading = 5, // State for when a level is actively being loaded asynchronously
    LevelCompletionSecret = 6,
}
