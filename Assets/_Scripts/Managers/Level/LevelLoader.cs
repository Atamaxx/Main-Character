using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public event Action<float> OnLoadProgress;
    public event Action OnLevelLoaded;

    [SerializeField]
    private float _minimumLoadingTime = 1.0f;

    [SerializeField]
    private LevelConfiguration _levelConfiguration;

    private AsyncOperation _asyncOperation;
    private bool _isLoading = false;
    private string _selectedLevelOverride = null;

    /// <summary>
    /// Sets a level to override the normal level progression
    /// </summary>
    public void SetSelectedLevel(string levelName)
    {
        _selectedLevelOverride = levelName;
        Debug.Log($"Level override set to: {levelName}");
    }

    /// <summary>
    /// Loads the next available level
    /// </summary>
    public void LoadNextAvailableLevel()
    {
        if (_isLoading)
            return;

        string levelToLoad;

        // Check for level override first
        if (!string.IsNullOrEmpty(_selectedLevelOverride))
        {
            levelToLoad = _selectedLevelOverride;
            _selectedLevelOverride = null; // Clear after use
            Debug.Log($"Loading override level: {levelToLoad}");
            StartCoroutine(LoadSceneAsync(levelToLoad));
            return;
        }

        string currentLevel = SceneManager.GetActiveScene().name;
        levelToLoad = DetermineNextLevelToLoad();

        // Prevent loading the same scene again
        if (levelToLoad == currentLevel)
        {
            Debug.LogWarning($"Attempted to load current scene ({currentLevel})");
            if (_levelConfiguration != null)
            {
                int currentIndex = FindLevelIndex(currentLevel);
                if (currentIndex >= 0)
                {
                    int nextIndex = (currentIndex + 1) % _levelConfiguration.LevelPairs.Count;
                    levelToLoad = _levelConfiguration.GetMainLevelAtIndex(nextIndex);
                }
            }

            if (levelToLoad == currentLevel)
            {
                Debug.LogError("Cannot determine next level to load");
                return;
            }
        }

        Debug.Log($"Loading next level: {levelToLoad}");
        StartCoroutine(LoadSceneAsync(levelToLoad));
    }

    /// <summary>
    /// Determine which level to load
    /// </summary>
    private string DetermineNextLevelToLoad()
    {
        string currentLevel = SceneManager.GetActiveScene().name;

        // Find next uncompleted level
        GameSession gameSession = GameSession.Instance;
        if (gameSession == null)
        {
            return GetDefaultLevel(currentLevel);
        }

        List<string> allMainLevels = GetAllMainLevels();

        // Find first uncompleted level that isn't the current level
        foreach (string levelName in allMainLevels)
        {
            if (levelName == "MainMenu" || levelName == currentLevel)
                continue;
            if (!gameSession.IsLevelCompleted(levelName))
            {
                return levelName;
            }
        }

        // All levels are completed: load the next level from the configuration order.
        if (_levelConfiguration != null)
        {
            int currentIndex = FindLevelIndex(currentLevel);
            if (currentIndex >= 0)
            {
                int nextIndex = (currentIndex + 1) % _levelConfiguration.LevelPairs.Count;
                return _levelConfiguration.GetMainLevelAtIndex(nextIndex);
            }
        }

        // Fallback: return the first level that's not MainMenu or the current level
        foreach (string levelName in allMainLevels)
        {
            if (levelName != "MainMenu" && levelName != currentLevel)
            {
                return levelName;
            }
        }

        return "MainMenu";
    }

    /// <summary>
    /// Get default level if GameSession isn't available
    /// </summary>
    private string GetDefaultLevel(string currentLevel)
    {
        List<string> allMainLevels = GetAllMainLevels();

        // Find first level that isn't MainMenu or current level
        foreach (string levelName in allMainLevels)
        {
            if (levelName != "MainMenu" && levelName != currentLevel)
            {
                return levelName;
            }
        }

        return allMainLevels.Count > 0 ? allMainLevels[0] : "Level_1";
    }

    /// <summary>
    /// Load level by name
    /// </summary>
    public void LoadLevelByName(string sceneName)
    {
        if (_isLoading || string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Cannot load level: already loading or invalid name");
            return;
        }

        Debug.Log($"Loading level by name: {sceneName}");
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Cancel any in-progress level loading
    /// </summary>
    public void CancelLevelLoading()
    {
        if (!_isLoading || _asyncOperation == null)
            return;

        Debug.Log("Cancelling level loading");
        StopAllCoroutines();
        _asyncOperation.allowSceneActivation = true;
        _isLoading = false;
    }

    /// <summary>
    /// Coroutine to load a scene asynchronously
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        _isLoading = true;

        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;

        float startTime = Time.time;
        while (!_asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            OnLoadProgress?.Invoke(progress);

            if (_asyncOperation.progress >= 0.9f && (Time.time - startTime) >= _minimumLoadingTime)
            {
                _asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        _isLoading = false;
        _asyncOperation = null;

        OnLevelLoaded?.Invoke();
    }

    /// <summary>
    /// Find the index of a level in the configuration
    /// </summary>
    private int FindLevelIndex(string levelName)
    {
        if (_levelConfiguration == null)
            return -1;

        for (int i = 0; i < _levelConfiguration.LevelPairs.Count; i++)
        {
            if (_levelConfiguration.LevelPairs[i].MainLevel == levelName)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Get all main level names
    /// </summary>
    public List<string> GetAllMainLevels()
    {
        List<string> mainLevels = new List<string>();

        if (_levelConfiguration == null)
            return mainLevels;

        foreach (var pair in _levelConfiguration.LevelPairs)
        {
            mainLevels.Add(pair.MainLevel);
        }

        return mainLevels;
    }

    /// <summary>
    /// Get secret level for main level
    /// </summary>
    public string GetSecretLevelForMain(string mainLevelName)
    {
        if (_levelConfiguration == null)
            return string.Empty;
        return _levelConfiguration.GetSecretLevelForMain(mainLevelName);
    }
}
