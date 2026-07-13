using System.Collections.Generic;
using UnityEngine;

public class ManualNavigation : NavigationGroup
{
    [SerializeField] private Button _mainButton;

    [SerializeField] private List<ButtonMapping> _buttonMappings = new List<ButtonMapping>();

    // Fixed property implementation to match abstract base class
    public override List<IButton> GroupButtons { get; protected set; } = new List<IButton>();
    public override IButton CurrentButton { get; protected set; }

    [System.Serializable]
    public class ButtonMapping
    {
        public Button button;
        public Button upButton;
        public Button downButton;
        public Button leftButton;
        public Button rightButton;
    }

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



    protected override void NavigateInDirection(Vector2 direction)
    {
        if (CurrentButton == null || !(CurrentButton is Button currentBtn))
        {
            ResetCurrentButton();
            return;
        }

        // Find the mapping for the current button
        ButtonMapping mapping = _buttonMappings.Find(m => m.button == currentBtn);
        if (mapping == null) return;

        Button nextButton = null;

        // Determine which direction has priority if diagonal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement takes priority
            if (direction.x > 0 && mapping.rightButton != null)
            {
                nextButton = mapping.rightButton;
            }
            else if (direction.x < 0 && mapping.leftButton != null)
            {
                nextButton = mapping.leftButton;
            }
        }
        else
        {
            // Vertical movement takes priority
            if (direction.y > 0 && mapping.upButton != null)
            {
                nextButton = mapping.upButton;
            }
            else if (direction.y < 0 && mapping.downButton != null)
            {
                nextButton = mapping.downButton;
            }
        }

        // If we found a next button, select it
        if (nextButton != null)
        {
            SetCurrentButton(nextButton);
        }
    }

    // Public method to manually navigate to a specific button
    public void NavigateTo(Button targetButton)
    {
        if (GroupButtons.Contains(targetButton))
        {
            SetCurrentButton(targetButton);
        }
    }



    // Optional: Method to handle when this group is shown/activated by a menu
    public void Show()
    {
        gameObject.SetActive(true);
        MenuController.Instance.SetActiveGroup(this);
    }

    // Optional: Method to handle when this group is hidden/deactivated
    public void Hide()
    {
        gameObject.SetActive(false);
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
}