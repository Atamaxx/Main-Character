using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Persistence {
    [Serializable] 
    public class GameData {
        // Basic info
        public string CurrentLevelName = "MainMenu";
        public GameState LastGameState = GameState.MainMenu;
        
        // Serializable lists that work with JsonUtility
        [SerializeField] public List<string> completedLevelsList = new List<string>();
        [SerializeField] public List<string> discoveredSecretLevelsList = new List<string>();
        [SerializeField] public List<string> discoveredTextsList = new List<string>();
        [SerializeField] public List<string> discoveredSecretsList = new List<string>();
        
        // Runtime HashSets for fast lookups - not serialized
        [NonSerialized] private HashSet<string> _completedLevels;
        [NonSerialized] private HashSet<string> _discoveredSecretLevels;
        [NonSerialized] private HashSet<string> _discoveredTexts;
        [NonSerialized] private HashSet<string> _discoveredSecrets;
        
        // Properties for the HashSets
        public HashSet<string> CompletedLevels {
            get {
                if (_completedLevels == null) {
                    _completedLevels = new HashSet<string>(completedLevelsList);
                }
                return _completedLevels;
            }
        }
        
        public HashSet<string> DiscoveredSecretLevels {
            get {
                if (_discoveredSecretLevels == null) {
                    _discoveredSecretLevels = new HashSet<string>(discoveredSecretLevelsList);
                }
                return _discoveredSecretLevels;
            }
        }
        
        public HashSet<string> DiscoveredTexts {
            get {
                if (_discoveredTexts == null) {
                    _discoveredTexts = new HashSet<string>(discoveredTextsList);
                }
                return _discoveredTexts;
            }
        }
        
        public HashSet<string> DiscoveredSecrets {
            get {
                if (_discoveredSecrets == null) {
                    _discoveredSecrets = new HashSet<string>(discoveredSecretsList);
                }
                return _discoveredSecrets;
            }
        }
        
        // Initialize HashSets from lists
        public void InitializeCollections() {
            completedLevelsList ??= new List<string>();
            discoveredSecretLevelsList ??= new List<string>();
            discoveredTextsList ??= new List<string>();
            discoveredSecretsList ??= new List<string>();
            
            _completedLevels = new HashSet<string>(completedLevelsList);
            _discoveredSecretLevels = new HashSet<string>(discoveredSecretLevelsList);
            _discoveredTexts = new HashSet<string>(discoveredTextsList);
            _discoveredSecrets = new HashSet<string>(discoveredSecretsList);
        }
        
        // Sync lists from HashSets before saving
        public void SyncCollections() {
            if (_completedLevels != null) {
                completedLevelsList = new List<string>(_completedLevels);
            }
            
            if (_discoveredSecretLevels != null) {
                discoveredSecretLevelsList = new List<string>(_discoveredSecretLevels);
            }
            
            if (_discoveredTexts != null) {
                discoveredTextsList = new List<string>(_discoveredTexts);
            }
            
            if (_discoveredSecrets != null) {
                discoveredSecretsList = new List<string>(_discoveredSecrets);
            }
        }
    }
}