using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension for LevelLoader to handle back textures for page flipping
/// </summary>
public class BackTextureHelper : MonoBehaviour
{
    [SerializeField] private LevelConfiguration _levelConfiguration;
    
    /// <summary>
    /// Get the back texture for a level
    /// </summary>
    public Texture2D GetBackTextureForLevel(string levelName)
    {
        if (_levelConfiguration == null) return null;

        foreach (var pair in _levelConfiguration.LevelPairs)
        {
            if (pair.MainLevel == levelName)
            {
                return pair.BackTexture;
            }
            if (pair.SecretLevel == levelName)
            {
                return pair.SecretBackTexture;
            }
        }

        return null;
    }
}