using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.Persistence {
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> {
        public GameData gameData;
        [SerializeField] private string _saveName = "save.json";
        private string _savePath;
        
        protected override void Awake() {
            base.Awake();
            _savePath = Path.Combine(Application.persistentDataPath, _saveName);
            Debug.Log($"Save path: {_savePath}");
            
            // Initialize with a new game, then try to load
            NewGame();
            LoadGame();
        }
        
        public void NewGame() {
            gameData = new GameData {
                CurrentLevelName = "MainMenu",
                LastGameState = GameState.MainMenu
            };
        }
        
        public void SaveGame() {
            try {
                // Try to load existing save to merge data
                GameData existingSave = null;
                
                if (File.Exists(_savePath)) {
                    try {
                        string existingJson = File.ReadAllText(_savePath);
                        existingSave = JsonUtility.FromJson<GameData>(existingJson);
                    } catch (Exception e) {
                        Debug.LogWarning($"Could not read existing save: {e.Message}");
                    }
                }
                
                // Update basic info
                gameData.CurrentLevelName = SceneManager.GetActiveScene().name;
                if (GameManager.Instance != null) {
                    gameData.LastGameState = GameManager.Instance.State;
                }
                
                // Synchronize collections before saving
                gameData.SyncCollections();
                
                // If existing save exists, merge collections
                if (existingSave != null) {
                    // Keep current level/state from gameData
                    existingSave.CurrentLevelName = gameData.CurrentLevelName;
                    existingSave.LastGameState = gameData.LastGameState;
                    
                    // Add all items from current collections to existing save
                    foreach (string level in gameData.completedLevelsList)
                        existingSave.completedLevelsList.Add(level);
                        
                    foreach (string secretLevel in gameData.discoveredSecretLevelsList)
                        existingSave.discoveredSecretLevelsList.Add(secretLevel);
                        
                    foreach (string text in gameData.discoveredTextsList)
                        existingSave.discoveredTextsList.Add(text);
                        
                    foreach (string secret in gameData.discoveredSecretsList)
                        existingSave.discoveredSecretsList.Add(secret);
                    
                    // Remove duplicates
                    existingSave.completedLevelsList = new List<string>(new HashSet<string>(existingSave.completedLevelsList));
                    existingSave.discoveredSecretLevelsList = new List<string>(new HashSet<string>(existingSave.discoveredSecretLevelsList));
                    existingSave.discoveredTextsList = new List<string>(new HashSet<string>(existingSave.discoveredTextsList));
                    existingSave.discoveredSecretsList = new List<string>(new HashSet<string>(existingSave.discoveredSecretsList));
                    
                    // Use merged data
                    gameData = existingSave;
                }
                
                // Write to file
                string json = JsonUtility.ToJson(gameData, true);
                File.WriteAllText(_savePath, json);
                Debug.Log($"Game saved successfully to {_savePath}");
            } catch (Exception e) {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }
        
        public void LoadGame() {
            try {
                if (File.Exists(_savePath)) {
                    string json = File.ReadAllText(_savePath);
                    gameData = JsonUtility.FromJson<GameData>(json);
                    
                    // Initialize HashSets from lists
                    gameData.InitializeCollections();
                    
                    Debug.Log($"Game loaded successfully from {_savePath}");
                    return;
                }
            } catch (Exception e) {
                Debug.LogError($"Failed to load game: {e.Message}");
            }
            
            // If load fails or no save exists, keep the new game created in Awake
            Debug.Log("No save file found or load failed. Starting new game.");
        }
        
        public void DeleteSave() {
            try {
                if (File.Exists(_savePath)) {
                    File.Delete(_savePath);
                    Debug.Log("Save file deleted");
                    NewGame();
                }
            } catch (Exception e) {
                Debug.LogError($"Failed to delete save: {e.Message}");
            }
        }
    }
}