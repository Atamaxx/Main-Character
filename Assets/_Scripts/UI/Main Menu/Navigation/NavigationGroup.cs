using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Unity.Cinemachine;

public abstract class NavigationGroup : MonoBehaviour, MenuControls.IMenuActions
{
    // Fixed accessor to match interface requirements in derived classes
    public abstract IButton CurrentButton { get; protected set; }
    public abstract List<IButton> GroupButtons { get; protected set; }

    [SerializeField] protected float _inputDelay = 0.2f;
    [SerializeField] protected bool _enableMouseInput = true;
    [SerializeField] protected Button _exitButton;
    [field: SerializeField] public CinemachineCamera GroupCamera { get; protected set; }
    [SerializeField] protected bool _useCustomExitActions = false;
    [SerializeField, ShowIf("_useCustomExitActions")] protected UnityEvent _onExitAction;

    // Public property to control mouse input
    public bool EnableMouseInput
    {
        get => _enableMouseInput;
        set => _enableMouseInput = value;
    }

    protected float _lastInputTime;
    protected InputSource _lastInputSource = InputSource.None;

    // Input system
    protected MenuControls _menuControls;
    protected Vector2 _currentNavigationInput;
    protected bool _selectPressed;
    protected bool _selectReleased;
    protected bool _exitPressed;

    protected abstract void ResetCurrentButton();
    protected abstract void NavigateInDirection(Vector2 direction);
    protected virtual void SetCurrentButton(IButton button)
    {
        // Skip if it's the same button
        // if (CurrentButton == button)
        //     return;

        // Deselect current button if any
        if (CurrentButton != null)
        {
            CurrentButton.OnDeselect();
        }

        CurrentButton = button;

        if (CurrentButton != null)
        {
            CurrentButton.OnSelect();
        }
    }

    protected enum InputSource
    {
        None,
        Keyboard,
        Mouse
    }

    protected virtual void Awake()
    {
        // Initialize the input system
        _menuControls = new MenuControls();
        _menuControls.Menu.AddCallbacks(this);
    }

    protected virtual void OnEnable()
    {
        this.ExecuteNextFrame(() =>
        {
            MenuController.Instance.RegisterGroup(this);

            // Enable input controls if this is the active group
            if (MenuController.Instance.ActiveGroup == this)
            {
                _menuControls.Enable();
            }

            ResetCurrentButton();
        });
    }

    protected virtual void OnDisable()
    {
        if (MenuController.Instance != null)
            MenuController.Instance.UnregisterGroup(this);

        // Disable input controls
        _menuControls.Disable();
    }

    protected virtual void Update()
    {
        // Only process input if this is the active group
        if (MenuController.Instance.ActiveGroup != this)
            return;

        // Check for input cooldown
        if (Time.unscaledTime - _lastInputTime < _inputDelay)
        {
            return;
        }

        // Process navigation if we have input
        if (_currentNavigationInput.magnitude > 0.5f)
        {
            _lastInputSource = InputSource.Keyboard;
            NavigateInDirection(_currentNavigationInput);
            _lastInputTime = Time.unscaledTime;
        }

        // Process select input
        if (_selectPressed)
        {
            _lastInputSource = InputSource.Keyboard;
            if (CurrentButton != null)
            {
                HandleButtonPress(CurrentButton);
            }
            _selectPressed = false;
            _lastInputTime = Time.unscaledTime;
        }

        // Process select release input
        if (_selectReleased)
        {
            _lastInputSource = InputSource.Keyboard;
            if (CurrentButton != null)
            {
                HandleButtonRelease(CurrentButton);
            }
            _selectReleased = false;
        }

        // Process exit input
        if (_exitPressed)
        {
            print("Exit pressed");
            _lastInputSource = InputSource.Keyboard;
            HandleExitInput();
            _exitPressed = false;
            _lastInputTime = Time.unscaledTime;
        }
    }

    // Implementation of IMenuActions interface
    public virtual void OnNavigate(InputAction.CallbackContext context)
    {
        // Only update the direction on performed to get the actual value
        if (context.performed)
        {
            _currentNavigationInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _currentNavigationInput = Vector2.zero;
        }
    }

    public virtual void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _selectPressed = true;
            _selectReleased = false;
        }
        else if (context.canceled)
        {
            _selectReleased = true;
            _selectPressed = false;
        }
    }

    // New method for the EXIT action
    public virtual void OnExit(InputAction.CallbackContext context)
    {
        // Only trigger on performed to avoid multiple presses
        if (context.performed)
        {
            _exitPressed = true;
        }
    }

    // Methods to handle activating/deactivating navigation group
    public virtual void ActivateGroup()
    {
        _menuControls.Enable();
        if (CurrentButton == null)
        {
            ResetCurrentButton();
        }
    }

    public virtual void DeactivateGroup()
    {
        _menuControls.Disable();
        if (CurrentButton != null)
        {
            // Deselect current button when this group is deactivated
            CurrentButton.ChangeState(ButtonState.Normal);
            CurrentButton = null;
        }
    }

    // Helper method to handle button press
    protected virtual void HandleButtonPress(IButton button)
    {
        button.ChangeState(ButtonState.Pressed);
    }

    // Helper method to handle button release
    protected virtual void HandleButtonRelease(IButton button)
    {
        // Check if this is a holdable button
        IHoldableButton holdableButton = button as IHoldableButton;
        if (holdableButton != null)
        {
            holdableButton.OnRelease();
        }
    }

    // New method to handle EXIT input
    protected virtual void HandleExitInput()
    {
        if (_useCustomExitActions)
        {
            print("Using custom exit actions");
            _onExitAction.Invoke();
            return;
        }

        if (_exitButton == null || !_exitButton.IsInteractable)
            return;

        // If the exit button is already selected, press it
        if ((object)CurrentButton == _exitButton)
        {
            _exitButton.OnClick();
        }
        // Otherwise, select the exit button
        else
        {
            SetCurrentButton(_exitButton);
        }
    }

    // MOUSE SUPPORT METHODS

    // Handle mouse hover over a button
    public virtual void HandleMouseHover(IButton button)
    {
        if (!_enableMouseInput)
            return;

        _lastInputSource = InputSource.Mouse;
        _lastInputTime = Time.unscaledTime;

        // Make this group active if it isn't already
        if (MenuController.Instance.ActiveGroup != this)
        {
            MenuController.Instance.SetActiveGroup(this);
        }

        // If current button is different, handle the change
        if (CurrentButton != button)
        {
            // Set the hovered button as current (this will deselect any previous button)
            SetCurrentButton(button);
        }

    }

    // Handle mouse exit from a button
    public virtual void HandleMouseExit(IButton button)
    {
        if (!_enableMouseInput)
            return;

        _lastInputTime = Time.unscaledTime;

        // // Only handle if this is the current button
        // if (CurrentButton == button)
        // {
        //     // Return to normal state
        //     button.OnDeselect();
        // }
    }

    // Handle mouse click on a button
    public virtual void HandleMouseClick(IButton button)
    {
        if (!_enableMouseInput)
            return;

        _lastInputSource = InputSource.Mouse;
        _lastInputTime = Time.unscaledTime;

        // Set as current button if it's not already
        if (CurrentButton != button)
        {
            if (CurrentButton != null)
            {
                CurrentButton.ChangeState(ButtonState.Normal);
            }
            CurrentButton = button;
        }

        // Trigger the press action
        button.OnClick();
    }

}