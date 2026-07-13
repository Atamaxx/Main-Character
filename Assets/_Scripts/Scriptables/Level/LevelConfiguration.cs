using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfiguration", menuName = "Level/Level Configuration", order = 1)]
public class LevelConfiguration : ScriptableObject
{
    [Serializable]
    public class LevelPair
    {
        [Tooltip("Name of the main level scene. Make sure this scene is added in Build Settings.")]
        public string MainLevel;
        public Texture2D BackTexture;

        [Tooltip("Name of the secret level scene corresponding to the main level.")]
        public string SecretLevel;
        public Texture2D SecretBackTexture;

    }

    [Tooltip("List of level pairs. Each pair contains a main level and its corresponding secret level.")]
    public List<LevelPair> LevelPairs = new List<LevelPair>();

    /// <summary>
    /// Returns the secret level name associated with the given main level.
    /// If not found, returns an empty string.
    /// </summary>
    public string GetSecretLevelForMain(string mainLevelName)
    {
        foreach (LevelPair pair in LevelPairs)
        {
            if (pair.MainLevel.Equals(mainLevelName, StringComparison.Ordinal))
            {
                return pair.SecretLevel;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Optionally, returns the main level name at the given index.
    /// </summary>
    public string GetMainLevelAtIndex(int index)
    {
        if (index < 0 || index >= LevelPairs.Count)
        {
            Debug.LogError($"Index {index} is out of range.");
            return string.Empty;
        }
        return LevelPairs[index].MainLevel;
    }
}
