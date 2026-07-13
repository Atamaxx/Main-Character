using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager class for the Discoveries menu that displays all texts and secrets found by the player.
/// This is typically used in the main menu to display collectibles.
/// </summary>
public class DiscoveriesManager : MonoBehaviour
{
    [SerializeField] private GameObject _textDiscoveryPrefab;
    [SerializeField] private GameObject _secretDiscoveryPrefab;
    [SerializeField] private Transform _textsContainer;
    [SerializeField] private Transform _secretsContainer;
    [SerializeField] private GameObject _noTextsFoundMessage;
    [SerializeField] private GameObject _noSecretsFoundMessage;
    
    // Reference texts and secrets (these would be filled via the Unity Editor)
    [SerializeField] private List<DiscoverableText> _allTexts = new List<DiscoverableText>();
    [SerializeField] private List<DiscoverableSecret> _allSecrets = new List<DiscoverableSecret>();
    
    private Dictionary<string, DiscoverableText> _textsById = new Dictionary<string, DiscoverableText>();
    private Dictionary<string, DiscoverableSecret> _secretsById = new Dictionary<string, DiscoverableSecret>();
    
    private void Awake()
    {
        // Create lookup dictionaries
        foreach (var text in _allTexts)
        {
            _textsById[text.GetTextId()] = text;
        }
        
        foreach (var secret in _allSecrets)
        {
            _secretsById[secret.GetSecretId()] = secret;
        }
    }
    
    private void OnEnable()
    {
        // Refresh discoveries when the menu is opened
        PopulateDiscoveries();
    }
    
    /// <summary>
    /// Populate the discoveries UI with all discovered texts and secrets
    /// </summary>
    public void PopulateDiscoveries()
    {
        if (GameSession.Instance == null) return;
        
        // Clear existing items
        ClearContainers();
        
        // Get discovered text IDs from save data
        string[] discoveredTextIds = GameSession.Instance.GetAllDiscoveredTexts();
        
        // Get discovered secret IDs from save data
        string[] discoveredSecretIds = GameSession.Instance.GetAllDiscoveredSecrets();
        
        // Show appropriate messages if nothing has been discovered
        _noTextsFoundMessage.SetActive(discoveredTextIds.Length == 0);
        _noSecretsFoundMessage.SetActive(discoveredSecretIds.Length == 0);
        
        // Populate texts
        foreach (string textId in discoveredTextIds)
        {
            if (_textsById.TryGetValue(textId, out DiscoverableText text))
            {
                CreateTextEntry(text);
            }
        }
        
        // Populate secrets
        foreach (string secretId in discoveredSecretIds)
        {
            if (_secretsById.TryGetValue(secretId, out DiscoverableSecret secret))
            {
                CreateSecretEntry(secret);
            }
        }
    }
    
    private void ClearContainers()
    {
        // Clear text container
        foreach (Transform child in _textsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Clear secrets container
        foreach (Transform child in _secretsContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void CreateTextEntry(DiscoverableText text)
    {
        GameObject entry = Instantiate(_textDiscoveryPrefab, _textsContainer);
        TextDiscoveryUI ui = entry.GetComponent<TextDiscoveryUI>();
        
        if (ui != null)
        {
            ui.SetText(text.GetTitle(), text.GetTextContent());
        }
    }
    
    private void CreateSecretEntry(DiscoverableSecret secret)
    {
        GameObject entry = Instantiate(_secretDiscoveryPrefab, _secretsContainer);
        SecretDiscoveryUI ui = entry.GetComponent<SecretDiscoveryUI>();
        
        if (ui != null)
        {
            ui.SetSecret(secret.GetTitle(), secret.GetDescription(), secret.GetIcon());
        }
    }
    
    /// <summary>
    /// Save all discovered texts that may not have been saved immediately
    /// </summary>
    public void SaveDiscoveries()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SaveGame();
        }
    }
}

// These would be separate files in a real project
public class TextDiscoveryUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _titleText;
    [SerializeField] private TMPro.TextMeshProUGUI _contentText;
    
    public void SetText(string title, string content)
    {
        _titleText.text = title;
        _contentText.text = content;
    }
}

public class SecretDiscoveryUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _titleText;
    [SerializeField] private TMPro.TextMeshProUGUI _descriptionText;
    [SerializeField] private UnityEngine.UI.Image _iconImage;
    
    public void SetSecret(string title, string description, Sprite icon)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        
        if (icon != null)
        {
            _iconImage.sprite = icon;
            _iconImage.gameObject.SetActive(true);
        }
        else
        {
            _iconImage.gameObject.SetActive(false);
        }
    }
}