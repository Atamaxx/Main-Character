using UnityEngine;

[RequireComponent(typeof(Button), typeof(Collider2D))]
public class MouseButtonHandler : MonoBehaviour
{
    private Button _button;
    private Collider2D _collider;
    private bool _isHovering = false;
    private bool _isHolding = false;
    private Camera _mainCamera;
    private NavigationGroup _cachedGroup;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;

        if (_button == null)
        {
            Debug.LogError("MouseButtonHandler requires a Button component", this);
            enabled = false;
        }

        if (_collider == null)
        {
            Debug.LogError("MouseButtonHandler requires a Collider2D component", this);
            enabled = false;
        }
    }

    private void Start()
    {
        // Cache the button's group for performance
        _cachedGroup = FindButtonGroup();
    }

    private void Update()
    {
        if (_mainCamera == null || !MenuController.Instance.EnableMouseSupport)
            return;

        // Check if this button belongs to the active group
        if (!IsInActiveGroup())
        {
            // If we were hovering or holding, clean up the state
            if (_isHovering)
            {
                _isHovering = false;
                _button.ChangeState(ButtonState.Normal);
            }
            
            if (_isHolding)
            {
                _isHolding = false;
                IHoldableButton holdableButton = _button as IHoldableButton;
                if (holdableButton != null)
                {
                    holdableButton.OnRelease();
                }
            }
            
            return; // Skip processing for inactive groups
        }

        // Get mouse position in world space
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Check if mouse is over this button
        bool isOverButton = _collider.OverlapPoint(mousePosition);

        // Handle hover state changes
        if (isOverButton && !_isHovering)
        {
            // Mouse entered
            _isHovering = true;
            HandleMouseEnter();
        }
        else if (!isOverButton && _isHovering)
        {
            // Mouse exited
            _isHovering = false;
            HandleMouseExit();
        }

        // Handle mouse button events
        if (isOverButton)
        {
            // Handle press start
            if (Input.GetMouseButtonDown(0) && !_isHolding)
            {
                _isHolding = true;
                HandleMouseDown();
            }
        }

        // Handle release (anywhere on screen)
        if (_isHolding && Input.GetMouseButtonUp(0))
        {
            _isHolding = false;
            HandleMouseUp();
        }
    }

    private bool IsInActiveGroup()
    {
        // Refresh cached group if null
        if (_cachedGroup == null)
        {
            _cachedGroup = FindButtonGroup();
        }
        
        // Check if this button's group is the active one
        return _cachedGroup != null && _cachedGroup == MenuController.Instance.ActiveGroup;
    }

    private void HandleMouseEnter()
    {
        if (!_button.IsInteractable)
            return;

        NavigationGroup group = _cachedGroup;
        if (group != null && group.EnableMouseInput)
        {
            // Make group active if it isn't already
            if (MenuController.Instance.ActiveGroup != group)
            {
                MenuController.Instance.SetActiveGroup(group);
            }

            group.HandleMouseHover(_button);
        }
        else
        {
            _button.ChangeState(ButtonState.Selected);
        }
    }

    private void HandleMouseExit()
    {
        if (!_button.IsInteractable)
            return;

        NavigationGroup group = _cachedGroup;
        if (group != null && group.EnableMouseInput)
        {
            group.HandleMouseExit(_button);
        }
        else
        {
            _button.ChangeState(ButtonState.Normal);
        }
    }

    private void HandleMouseDown()
    {
        if (!_button.IsInteractable)
            return;

        NavigationGroup group = _cachedGroup;
        if (group != null && group.EnableMouseInput)
        {
            group.HandleMouseClick(_button);
        }
        else
        {
            _button.OnClick();
        }
    }

    private void HandleMouseUp()
    {
        // Check if button implements IHoldableButton
        IHoldableButton holdableButton = _button as IHoldableButton;
        if (holdableButton != null)
        {
            // Release the hold state
            holdableButton.OnRelease();
        }

        // For other buttons, standard behavior is handled by the Button's ReturnToSelectedState
    }

    private NavigationGroup FindButtonGroup()
    {
        foreach (var group in MenuController.Instance.GetAllGroups())
        {
            if (group.GroupButtons.Contains(_button))
            {
                return group;
            }
        }
        return null;
    }

    private void OnDisable()
    {
        // Reset hover and hold state when disabled
        if (_isHovering)
        {
            _isHovering = false;
            HandleMouseExit();
        }

        if (_isHolding)
        {
            _isHolding = false;
            HandleMouseUp();
        }
    }
}