using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component for secret discoveries that can be found in levels.
/// When discovered, these are saved to GameData and can be reviewed in the game's "Discoveries" menu.
/// </summary>
public class DiscoverableSecret : MonoBehaviour
{
    [Tooltip("Unique identifier for this secret")]
    [SerializeField] private string _secretId;
    
    [Tooltip("Title of the discovered secret")]
    [SerializeField] private string _title;
    
    [TextArea(3, 10)]
    [Tooltip("Description of the secret that will be displayed in the discoveries menu")]
    [SerializeField] private string _description;
    
    [Tooltip("Optional sprite to show in the discoveries menu")]
    [SerializeField] private Sprite _icon;
    
    [Tooltip("Events triggered when this secret is discovered")]
    [SerializeField] private UnityEvent _onDiscovered;
    
    private bool _isDiscovered = false;
    
    private void Start()
    {
        // Check if this secret has already been discovered
        if (GameSession.Instance != null && !string.IsNullOrEmpty(_secretId))
        {
            _isDiscovered = GameSession.Instance.IsSecretDiscovered(_secretId);
        }
    }
    
    /// <summary>
    /// Discover this secret and record it in the game's save data
    /// </summary>
    public void Discover()
    {
        if (_isDiscovered) return;
        
        _isDiscovered = true;
        
        if (GameSession.Instance != null && !string.IsNullOrEmpty(_secretId))
        {
            GameSession.Instance.DiscoverSecret(_secretId);
            // Secrets are always saved immediately due to their importance
        }
        
        // Trigger discovery events
        _onDiscovered?.Invoke();
    }
    
    public string GetTitle()
    {
        return _title;
    }
    
    public string GetDescription()
    {
        return _description;
    }
    
    public Sprite GetIcon()
    {
        return _icon;
    }
    
    public string GetSecretId()
    {
        return _secretId;
    }
    
    public bool IsDiscovered()
    {
        return _isDiscovered;
    }
}