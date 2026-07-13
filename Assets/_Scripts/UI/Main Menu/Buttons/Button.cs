using UnityEngine;
using UnityEngine.Events;

public abstract class Button : MonoBehaviour, IButton
{
    [SerializeField] protected bool _interactable = true;
    [SerializeField] protected UnityEvent _onButtonPressed;
    [SerializeField] protected UnityEvent _onButtonSelect;
    [SerializeField] protected UnityEvent _onButtonDeselect;

    protected ButtonState _currentState = ButtonState.Normal;
    protected NavigationGroup _cachedGroup; // Cache the group this button belongs to

    public bool IsInteractable
    {
        get => _interactable;
        set
        {
            if (_interactable != value)
            {
                _interactable = value;
                if (!_interactable)
                {
                    ChangeState(ButtonState.Disabled);
                }
                else if (_currentState == ButtonState.Disabled)
                {
                    ChangeState(ButtonState.Normal);
                }
            }
        }
    }

    public UnityEvent OnButtonPressed => _onButtonPressed;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        // Cache the navigation group this button belongs to
        _cachedGroup = FindNavigationGroup();
    }

    protected virtual void OnEnable()
    {
        this.ExecuteNextFrame(() =>
        {
            if (MenuController.Instance != null)
            {
                MenuController.Instance.RegisterButton(this);
            }
            else
            {
                // Still null after one frame, log warning
                Debug.LogWarning("MenuController.Instance still null after waiting one frame");
            }
        });
    }

    protected virtual void OnDisable()
    {
        if (MenuController.Instance != null)
        {
            MenuController.Instance.UnregisterButton(this);
        }
    }

    public virtual void ChangeState(ButtonState newState)
    {
        // Don't change state if button is not interactable (except to Disabled)
        if (!_interactable && newState != ButtonState.Normal && newState != ButtonState.Disabled)
            return;
        // Check if this button belongs to an inactive group
        // Only allow Normal and Disabled states for buttons in inactive groups
        if (IsInInactiveGroup() && newState != ButtonState.Normal && newState != ButtonState.Disabled)
        {
            // Reset to normal state to ensure button stays visually inactive
            if (_currentState != ButtonState.Normal && _currentState != ButtonState.Disabled)
            {
                _currentState = ButtonState.Normal;
                UpdateVisuals();
            }
            return;
        }

        ButtonState oldState = _currentState;

        _currentState = newState;

        // Update visuals based on state
        UpdateVisuals();

        // Notify MenuController about the state change
        if (MenuController.Instance != null)
        {
            MenuController.Instance.NotifyButtonStateChanged(this, newState);
        }


        PlayStateChangeSound(oldState, newState);


        // Handle special states
        if (newState == ButtonState.Pressed)
        {
            _onButtonPressed?.Invoke();
        }
    }

    private bool IsInInactiveGroup()
    {
        // Update cached group if needed
        if (_cachedGroup == null)
        {
            _cachedGroup = FindNavigationGroup();
        }

        // Check if this button's group exists and is not active
        return _cachedGroup != null &&
               MenuController.Instance != null &&
               MenuController.Instance.ActiveGroup != _cachedGroup;
    }

    private NavigationGroup FindNavigationGroup()
    {
        if (MenuController.Instance == null)
            return null;

        foreach (var group in MenuController.Instance.GetAllGroups())
        {
            if (group.GroupButtons.Contains(this))
            {
                return group;
            }
        }
        return null;
    }

    // Methods required by IButton interface
    public virtual void OnSelect()
    {
        if (!IsInInactiveGroup())
        {
            ChangeState(ButtonState.Selected);
            _onButtonSelect?.Invoke();
        }
    }

    public virtual void OnDeselect()
    {
        //FMODUnity.RuntimeManager.PlayOneShot(FMODEventsMenu.Instance.ButtonDeselectSFX);
        ChangeState(ButtonState.Normal);
        _onButtonDeselect?.Invoke();
    }

    public virtual void OnClick()
    {
        // Only allow clicking if in active group
        if (!IsInInactiveGroup())
        {
            ChangeState(ButtonState.Pressed);

            // Only schedule the automatic return if this button should auto-return
            if (ShouldAutoReturnToSelectedState())
            {
                // Automatically return to Selected state after a short delay
                Invoke(nameof(ReturnToSelectedState), 0.1f);
            }
        }
    }

    // New virtual method to determine if a button should automatically return to selected state
    protected virtual bool ShouldAutoReturnToSelectedState()
    {
        return true; // Default behavior is to auto-return
    }

    protected virtual void ReturnToSelectedState()
    {
        if (_currentState == ButtonState.Pressed)
        {
            ChangeState(ButtonState.Selected);
        }
    }

    // Abstract method that concrete button types must implement
    protected abstract void UpdateVisuals();

    protected virtual void PlayStateChangeSound(ButtonState oldState, ButtonState newState)
    {
        if(oldState == newState) return;
        // Check if FMOD events system is available
        if (FMODEvents.Instance == null)
            return;

        if (oldState != ButtonState.Selected && newState == ButtonState.Selected)
        {
            FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.Instance.ButtonSelectSFX);
        }
        // else if (newState == ButtonState.Pressed)
        // {
        //     FMODUnity.RuntimeManager.PlayOneShot(FMODEventsMenu.Instance.ButtonClickSFX);
        // }
    }
}


public interface IHoldableButton : IButton
{
    // Method to handle when a button is released after being held
    void OnRelease();

    // Property to check if the button is currently being held
    bool IsHeld { get; }
}