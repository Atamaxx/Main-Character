using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component for text discoveries that can be found in levels.
/// When discovered, these are saved to GameData and can be reviewed in the game's "Discoveries" menu.
/// </summary>
public class DiscoverableText : MonoBehaviour
{
    [Tooltip("Unique identifier for this text discovery")]
    [SerializeField] private string _textId;
    
    [Tooltip("Title of the discovered text")]
    [SerializeField] private string _title;
    
    [TextArea(3, 10)]
    [Tooltip("The actual text content that will be displayed in the discoveries menu")]
    [SerializeField] private string _textContent;
    
    [Tooltip("Events triggered when this text is discovered")]
    [SerializeField] private UnityEvent _onDiscovered;
    
    [Tooltip("Should this be saved immediately when discovered? Set to false for performance if many texts can be discovered in quick succession.")]
    [SerializeField] private bool _saveImmediately = false;
    
    private bool _isDiscovered = false;
    
    private void Start()
    {
        // Check if this text has already been discovered
        if (GameSession.Instance != null && !string.IsNullOrEmpty(_textId))
        {
            _isDiscovered = GameSession.Instance.IsTextDiscovered(_textId);
        }
    }
    
    /// <summary>
    /// Discover this text and record it in the game's save data
    /// </summary>
    public void Discover()
    {
        if (_isDiscovered) return;
        
        _isDiscovered = true;
        
        if (GameSession.Instance != null && !string.IsNullOrEmpty(_textId))
        {
            GameSession.Instance.DiscoverText(_textId);
            
            // Save immediately only if requested
            if (_saveImmediately)
            {
                GameSession.Instance.SaveGame();
            }
        }
        
        // Trigger discovery events
        _onDiscovered?.Invoke();
    }
    
    public string GetTitle()
    {
        return _title;
    }
    
    public string GetTextContent()
    {
        return _textContent;
    }
    
    public string GetTextId()
    {
        return _textId;
    }
    
    public bool IsDiscovered()
    {
        return _isDiscovered;
    }
}