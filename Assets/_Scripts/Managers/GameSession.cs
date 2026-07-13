using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : PersistentSingleton<GameSession>
{
    private SaveLoadSystem _saveSystem;

    protected override void Awake()
    {
        base.Awake();
        _saveSystem = SaveLoadSystem.Instance;
    }

    private void OnEnable()
    {
        // Try to get reference again in case order of initialization was wrong
        if (_saveSystem == null)
        {
            _saveSystem = SaveLoadSystem.Instance;
        }
    }

    public void SaveGame()
    {
        if (_saveSystem == null)
        {
            _saveSystem = SaveLoadSystem.Instance;
            if (_saveSystem == null)
            {
                Debug.LogError(
                    "SaveLoadSystem instance is missing. Make sure it exists in the scene."
                );
                return;
            }
        }

        // SaveLoadSystem now handles CurrentLevelName and LastGameState internally
        _saveSystem.SaveGame();
    }

    public void LoadGame()
    {
        if (_saveSystem == null)
        {
            _saveSystem = SaveLoadSystem.Instance;
            if (_saveSystem == null)
            {
                Debug.LogError(
                    "SaveLoadSystem instance is missing. Make sure it exists in the scene."
                );
                return;
            }
        }

        _saveSystem.LoadGame();
    }

    public void MarkLevelCompleted(string levelName)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(levelName))
            return;

        _saveSystem.gameData.CompletedLevels.Add(levelName);
        SaveGame();
        Debug.Log($"Marked level as completed: {levelName}");
    }

    public void DiscoverSecretLevel(string secretLevelName)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(secretLevelName))
            return;

        _saveSystem.gameData.DiscoveredSecretLevels.Add(secretLevelName);
        SaveGame();
        Debug.Log($"Discovered secret level: {secretLevelName}");
    }

    public void DiscoverText(string textId)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(textId))
            return;

        _saveSystem.gameData.DiscoveredTexts.Add(textId);
        // No immediate save to avoid performance issues when finding multiple texts
    }

    public void DiscoverSecret(string secretId)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(secretId))
            return;

        _saveSystem.gameData.DiscoveredSecrets.Add(secretId);
        SaveGame(); // Saving immediately for important discoveries
        Debug.Log($"Discovered secret: {secretId}");
    }

    public bool IsLevelCompleted(string levelName)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(levelName))
            return false;
        return _saveSystem.gameData.CompletedLevels.Contains(levelName);
    }

    public bool IsSecretLevelDiscovered(string secretLevelName)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(secretLevelName))
            return false;
        return _saveSystem.gameData.DiscoveredSecretLevels.Contains(secretLevelName);
    }

    public bool IsTextDiscovered(string textId)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(textId))
            return false;
        return _saveSystem.gameData.DiscoveredTexts.Contains(textId);
    }

    public bool IsSecretDiscovered(string secretId)
    {
        if (_saveSystem == null || string.IsNullOrEmpty(secretId))
            return false;
        return _saveSystem.gameData.DiscoveredSecrets.Contains(secretId);
    }

    public string[] GetAllDiscoveredTexts()
    {
        if (_saveSystem == null)
            return new string[0];

        string[] texts = new string[_saveSystem.gameData.DiscoveredTexts.Count];
        _saveSystem.gameData.DiscoveredTexts.CopyTo(texts);
        return texts;
    }

    public string[] GetAllDiscoveredSecrets()
    {
        if (_saveSystem == null)
            return new string[0];

        string[] secrets = new string[_saveSystem.gameData.DiscoveredSecrets.Count];
        _saveSystem.gameData.DiscoveredSecrets.CopyTo(secrets);
        return secrets;
    }

    public string[] GetAllDiscoveredSecretLevels()
    {
        if (_saveSystem == null)
            return new string[0];

        string[] secretLevels = new string[_saveSystem.gameData.DiscoveredSecretLevels.Count];
        _saveSystem.gameData.DiscoveredSecretLevels.CopyTo(secretLevels);
        return secretLevels;
    }
}
