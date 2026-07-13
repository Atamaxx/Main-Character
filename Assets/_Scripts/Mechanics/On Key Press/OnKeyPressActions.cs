using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;
using NaughtyAttributes;
using UnityEngine.Events;

public class OnKeyPressActions : MonoBehaviour
{
    [Tooltip("Reference to the Input Action to listen for.")]
    [BoxGroup("INPUT ACTION"), SerializeField] private InputActionReference _inputActionReference;

    [Tooltip("Specify if the action is a button or a value type.")]
    [BoxGroup("INPUT ACTION"), SerializeField] private ActionType _actionType = ActionType.Button;

    public UnityEvent KeyPress;
    public UnityEvent KeyRelease;

    // Enum to define action types
    public enum ActionType
    {
        Button,     // For actions like button presses
        Value       // For actions like movement vectors
    }

    // Enum for axis selection
    public enum Axis
    {
        X,
        Y,
        Both
    }

    // Enum for comparison types
    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        EqualTo
    }

    [Header("Value Action Conditions")]
    [Tooltip("Conditions to evaluate for value actions.")]
    [SerializeField] private ValueCondition[] _valueConditions;

    [Serializable]
    public class ValueCondition
    {
        [Tooltip("Select the axis to monitor.")]
        public Axis axis = Axis.Y;

        [Tooltip("Select the type of comparison.")]
        public ComparisonType comparison = ComparisonType.GreaterThan;

        [Tooltip("Threshold value for comparison.")]
        public float threshold = 0f;

        [Tooltip("Direction to consider for the condition.")]
        public Direction direction = Direction.Any;

        public enum Direction
        {
            Positive,
            Negative,
            Any
        }
    }

    private void OnEnable()
    {
        if (_inputActionReference == null)
        {
            Debug.LogError("InputActionReference is not assigned.");
            return;
        }

        if (_actionType == ActionType.Button)
        {
            _inputActionReference.action.performed += OnActionPerformed;
            _inputActionReference.action.canceled += OnActionCanceled;
        }
        else if (_actionType == ActionType.Value)
        {
            _inputActionReference.action.performed += OnValueActionPerformed;
            _inputActionReference.action.canceled += OnValueActionCanceled;
        }

        _inputActionReference.action.Enable();

    }

    private void OnDisable()
    {
        if (_inputActionReference == null)
            return;

        if (_actionType == ActionType.Button)
        {
            _inputActionReference.action.performed -= OnActionPerformed;
            _inputActionReference.action.canceled -= OnActionCanceled;
        }
        else if (_actionType == ActionType.Value)
        {
            _inputActionReference.action.performed -= OnValueActionPerformed;
            _inputActionReference.action.canceled -= OnValueActionCanceled;
        }

       // _inputActionReference.action.Disable();
    }

    // For Button-type actions
    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        KeyPress.Invoke();
    }

    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        KeyRelease.Invoke();
    }

    // For Value-type actions (e.g., Vector2)
    private void OnValueActionPerformed(InputAction.CallbackContext context)
    {
        bool conditionMet = false;

        // Iterate through all conditions
        foreach (var condition in _valueConditions)
        {
            float valueToCheck = 0f;

            // Determine which axis to monitor
            if (condition.axis == Axis.X || condition.axis == Axis.Both)
            {
                Vector2 vector = context.ReadValue<Vector2>();
                valueToCheck = vector.x;
                conditionMet |= EvaluateCondition(valueToCheck, condition);
            }

            if (condition.axis == Axis.Y || condition.axis == Axis.Both)
            {
                Vector2 vector = context.ReadValue<Vector2>();
                valueToCheck = vector.y;
                conditionMet |= EvaluateCondition(valueToCheck, condition);
            }
        }

        if (conditionMet)
        {
            KeyPress.Invoke();
        }
        else
        {
           KeyRelease.Invoke();
        }
    }

    private void OnValueActionCanceled(InputAction.CallbackContext context)
    {
       KeyRelease.Invoke();
    }

    /// <summary>
    /// Evaluates a single condition based on the input value and condition settings.
    /// </summary>
    /// <param name="value">The input value to evaluate.</param>
    /// <param name="condition">The condition settings.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private bool EvaluateCondition(float value, ValueCondition condition)
    {
        // Handle direction
        if (condition.direction == ValueCondition.Direction.Positive && value < 0)
            return false;
        if (condition.direction == ValueCondition.Direction.Negative && value > 0)
            return false;
        // If direction is Any, no need to check

        switch (condition.comparison)
        {
            case ComparisonType.GreaterThan:
                return value > condition.threshold;
            case ComparisonType.LessThan:
                return value < condition.threshold;
            case ComparisonType.EqualTo:
                return Mathf.Approximately(value, condition.threshold);
            default:
                return false;
        }
    }
}
