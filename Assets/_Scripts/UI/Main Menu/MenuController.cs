using System.Collections.Generic;
using UnityEngine;

public class MenuController : StaticInstance<MenuController>
{
    private List<IButton> _buttons = new();
    private List<NavigationGroup> _groups = new();

    // Track the currently active navigation group
    private NavigationGroup _activeGroup;
    public NavigationGroup ActiveGroup => _activeGroup;

    // Event callbacks for button state changes (optional)
    public delegate void ButtonStateChangedHandler(IButton button, ButtonState state);
    public event ButtonStateChangedHandler OnButtonStateChanged;

    [SerializeField] private CinemachineManager _cinemachineManager;
    [SerializeField] private bool _enableMouseSupport = true;
    [SerializeField] private bool _debugMode = false;

    public bool EnableMouseSupport
    {
        get => _enableMouseSupport;
        set
        {
            if (_enableMouseSupport != value)
            {
                _enableMouseSupport = value;
                OnMouseSupportChanged();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        // Initialize the MenuControls input system
    }

    public void RegisterGroup(NavigationGroup group)
    {
        if (!_groups.Contains(group))
        {
            if (_debugMode) Debug.Log($"Registering group: {group.name}");
            _groups.Add(group);

            // If this is the first group added, make it active
            if (_groups.Count == 1 && _activeGroup == null)
            {
                SetActiveGroup(group);
            }
            else
            {
                // Force all buttons in this non-active group to Normal state
                ForceGroupButtonsToNormalState(group);
            }
        }
    }

    public void UnregisterGroup(NavigationGroup group)
    {
        if (_groups.Contains(group))
        {
            if (_debugMode) Debug.Log($"Unregistering group: {group.name}");

            // If removing the active group, try to find a new active group
            if (_activeGroup == group)
            {
                _activeGroup = null;

                // Find another group to make active
                foreach (var g in _groups)
                {
                    if (g != group && g.isActiveAndEnabled)
                    {
                        SetActiveGroup(g);
                        break;
                    }
                }
            }

            _groups.Remove(group);
        }
    }

    public void RegisterButton(IButton button)
    {
        if (!_buttons.Contains(button))
        {
            _buttons.Add(button);

            // Ensure button starts in correct state based on group status
            NavigationGroup buttonGroup = FindButtonGroup(button);
            if (buttonGroup != null && buttonGroup != _activeGroup)
            {
                // Force buttons in non-active groups to Normal state
                button.ChangeState(ButtonState.Normal);
            }
        }

        // Ensure button has necessary components for mouse support
        if (_enableMouseSupport)
        {
            EnsureButtonHasMouseSupport(button);
        }
    }

    public void UnregisterButton(IButton button)
    {
        if (_buttons.Contains(button))
            _buttons.Remove(button);
    }

    public void SetActiveGroup(NavigationGroup group)
    {
        if (group == null || group == _activeGroup)
            return;

        if (_debugMode) Debug.Log($"Setting active group from {(_activeGroup != null ? _activeGroup.name : "none")} to {group.name}");

        // Deactivate the current group
        if (_activeGroup != null)
        {
            // Force all buttons in the current active group to Normal state
            ForceGroupButtonsToNormalState(_activeGroup);

            _activeGroup.DeactivateGroup();
        }

        // Force all buttons in all OTHER groups to Normal state
        foreach (var otherGroup in _groups)
        {
            if (otherGroup != group)
            {
                ForceGroupButtonsToNormalState(otherGroup);
            }
        }

        // Set and activate the new group
        _activeGroup = group;

        if (_activeGroup != null)
        {
            _activeGroup.ActivateGroup();
            _cinemachineManager.SwitchToCamera(_activeGroup.GroupCamera);
            _activeGroup.EnableMouseInput = _enableMouseSupport;
        }
    }

    // Helper method to force all buttons in a group to Normal state
    private void ForceGroupButtonsToNormalState(NavigationGroup group)
    {
        if (group == null) return;

        if (_debugMode) Debug.Log($"Forcing Normal state for all buttons in group: {group.name}");

        foreach (var button in group.GroupButtons)
        {
            // Force button to Normal state
            button.ChangeState(ButtonState.Normal);

            // For holdable buttons, ensure they release any held state
            IHoldableButton holdableButton = button as IHoldableButton;
            if (holdableButton != null && holdableButton.IsHeld)
            {
                holdableButton.OnRelease();
            }
        }
    }

    // Method to notify when a button state changes (called by buttons)
    public void NotifyButtonStateChanged(IButton button, ButtonState state)
    {
        // Always allow the notification to pass through
        OnButtonStateChanged?.Invoke(button, state);
    }

    // Helper method to find a button by name
    public IButton FindButton(string buttonName)
    {
        foreach (var button in _buttons)
        {
            MonoBehaviour mb = button as MonoBehaviour;
            if (mb != null && mb.name == buttonName)
            {
                return button;
            }
        }
        return null;
    }

    // Helper method to find a navigation group by name
    public NavigationGroup FindNavigationGroup(string groupName)
    {
        foreach (var group in _groups)
        {
            if (group.name == groupName)
            {
                return group;
            }
        }
        return null;
    }

    // Helper method to find which group a button belongs to
    public NavigationGroup FindButtonGroup(IButton button)
    {
        foreach (var group in _groups)
        {
            if (group.GroupButtons.Contains(button))
            {
                return group;
            }
        }
        return null;
    }

    // Check if a button is in the active group
    public bool IsButtonInActiveGroup(IButton button)
    {
        if (_activeGroup == null)
            return false;

        return _activeGroup.GroupButtons.Contains(button);
    }

    // Get all registered groups (used by MouseButtonHandler)
    public List<NavigationGroup> GetAllGroups()
    {
        return new List<NavigationGroup>(_groups);
    }

    // Handle changes to mouse support
    private void OnMouseSupportChanged()
    {
        // Update all navigation groups with the new mouse support setting
        foreach (var group in _groups)
        {
            group.EnableMouseInput = _enableMouseSupport;
        }

        // Add or remove MouseButtonHandler components
        if (_enableMouseSupport)
        {
            // Add handlers to any buttons that don't have them
            foreach (var button in _buttons)
            {
                EnsureButtonHasMouseSupport(button);
            }
        }
    }

    private void EnsureButtonHasMouseSupport(IButton button)
    {
        MonoBehaviour buttonMono = button as MonoBehaviour;
        if (buttonMono == null)
            return;

        // Add MouseButtonHandler if missing
        if (buttonMono.GetComponent<MouseButtonHandler>() == null)
        {
            // Check/add collider for mouse interaction
            Collider2D collider = buttonMono.GetComponent<Collider2D>();
            if (collider == null)
            {
                // Add a BoxCollider2D sized to match the TextMeshPro bounds
                BoxCollider2D boxCollider = buttonMono.gameObject.AddComponent<BoxCollider2D>();

                // Try to get the TextMeshPro component to size the collider
                TMPro.TextMeshPro textMesh = buttonMono.GetComponent<TMPro.TextMeshPro>();
                if (textMesh != null)
                {
                    // Size the collider to match the text bounds
                    boxCollider.size = new Vector2(
                        textMesh.textBounds.size.x,
                        textMesh.textBounds.size.y);

                    // Center the collider on the text (local space)
                    boxCollider.offset = textMesh.textBounds.center;
                }
            }

            // Now add the MouseButtonHandler component
            buttonMono.gameObject.AddComponent<MouseButtonHandler>();
        }
    }
}
public interface IButton
{
    void ChangeState(ButtonState newState);

    // Optional additions to the interface
    bool IsInteractable { get; set; }

    // Events for button lifecycle
    // These could be called through ChangeState instead of direct methods
    void OnSelect();
    void OnDeselect();
    void OnClick();
}

public enum ButtonState
{
    Normal,
    Selected,
    Pressed,
    Disabled
}