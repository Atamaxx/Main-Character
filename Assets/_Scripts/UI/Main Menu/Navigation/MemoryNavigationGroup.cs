using System.Collections.Generic;
using UnityEngine;

public class SimpleMemoryNavigation : NavigationGroup
{
    [SerializeField] private Button _mainButton;
    [SerializeField] private List<ButtonMapping> _buttonMappings = new List<ButtonMapping>();

    [System.Serializable]
    public class ButtonMapping
    {
        public Button button;
        public Button upButton;
        public Button downButton;
        public Button leftButton;
        public Button rightButton;
    }

    // Just track the previous button - much simpler
    private Button _previousButton;
    private Vector2 _lastDirection;

    // Fixed property implementation to match abstract base class
    public override List<IButton> GroupButtons { get; protected set; } = new List<IButton>();
    public override IButton CurrentButton { get; protected set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        InitializeButtons();
        ResetCurrentButton();
    }

    private void InitializeButtons()
    {
        // Clear and rebuild the button list
        GroupButtons.Clear();

        // Add the main button first
        if (_mainButton != null && !GroupButtons.Contains(_mainButton))
        {
            GroupButtons.Add(_mainButton);
        }

        // Add all other buttons from mappings
        foreach (var mapping in _buttonMappings)
        {
            if (mapping.button != null && !GroupButtons.Contains(mapping.button))
            {
                GroupButtons.Add(mapping.button);
            }
        }
    }

    protected override void ResetCurrentButton()
    {
        if (_mainButton != null)
        {
            SetCurrentButton(_mainButton);
        }
        else if (GroupButtons.Count > 0)
        {
            SetCurrentButton(GroupButtons[0]);
        }
    }

    // Override navigation to add simple back-step memory
    protected override void NavigateInDirection(Vector2 direction)
    {
        // Get the current button
        Button currentButton = CurrentButton as Button;

        if (currentButton == null)
        {
            ResetCurrentButton();
            return;
        }

        // Check if we're navigating in the opposite direction of last movement
        if (IsOppositeDirection(direction, _lastDirection) && _previousButton != null)
        {
            // Go back to the previous button
            Button temp = currentButton;
            SetCurrentButton(_previousButton);
            _previousButton = temp;
            _lastDirection = direction;
            return;
        }

        // Find the next button using normal navigation
        Button nextButton = FindNextButton(currentButton, direction);
        if (nextButton != null)
        {
            // Remember where we came from
            _previousButton = currentButton;
            _lastDirection = direction;

            // Navigate to next button
            SetCurrentButton(nextButton);
        }
    }

    // Check if two directions are opposite
    private bool IsOppositeDirection(Vector2 dir1, Vector2 dir2)
    {
        return (dir1.x != 0 && dir1.x == -dir2.x) || (dir1.y != 0 && dir1.y == -dir2.y);
    }

    // Find the next button in the given direction
    private Button FindNextButton(Button currentButton, Vector2 direction)
    {
        // Find the mapping for the current button
        ButtonMapping mapping = _buttonMappings.Find(m => m.button == currentButton);
        if (mapping == null) return null;

        // Return the button in the specified direction
        if (direction.x > 0 && mapping.rightButton != null)
            return mapping.rightButton;

        if (direction.x < 0 && mapping.leftButton != null)
            return mapping.leftButton;

        if (direction.y > 0 && mapping.upButton != null)
            return mapping.upButton;

        if (direction.y < 0 && mapping.downButton != null)
            return mapping.downButton;

        return null;
    }

    // Override mouse handlers for better control
    public override void HandleMouseHover(IButton button)
    {
        base.HandleMouseHover(button);

        // Make sure button state is correct
        button.OnSelect();
    }

    // public override void HandleMouseExit(IButton button)
    // {
    //     // If this is the current button, keep it in normal state when mouse exits
    //     if (CurrentButton == button)
    //     {
    //         button.OnDeselect();
    //     }
    // }

    public override void HandleMouseClick(IButton button)
    {
        // Override to ensure button is properly set as current before click
        SetCurrentButton(button);

        // Then handle the click
        button.OnClick();
    }

    // Override the HandleButtonPress method to ensure OnClick is called
    protected override void HandleButtonPress(IButton button)
    {
        // First change the state to pressed
        button.ChangeState(ButtonState.Pressed);

        // Then explicitly call OnClick to ensure the button functionality is triggered
        button.OnClick();
    }

    // Override HandleButtonRelease for better support of holdable buttons
    protected override void HandleButtonRelease(IButton button)
    {
        // Check if this is a holdable button
        IHoldableButton holdableButton = button as IHoldableButton;
        if (holdableButton != null)
        {
            holdableButton.OnRelease();
        }
        else
        {
            // For non-holdable buttons, if they're still in pressed state,
            // return them to selected state
            if (button == CurrentButton)
            {
                button.ChangeState(ButtonState.Selected);
            }
        }
    }
}