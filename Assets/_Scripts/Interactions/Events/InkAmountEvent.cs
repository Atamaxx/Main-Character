using UnityEngine;
using UnityEngine.Events;

public class InkAmountEvent : MonoBehaviour
{
    public enum ComparisonType
    {
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    [SerializeField] private InkManager _inkManager;
    [SerializeField] private int _inkNeeded = 4;
    [SerializeField] private ComparisonType _comparison = ComparisonType.Equal;

    [SerializeField] private UnityEvent _onConditionMet;
    [SerializeField] private UnityEvent _onConditionNotMet;

    private bool _isActive = false;

    void Update()
    {
        bool shouldActivate = EvaluateCondition(_inkManager.CurrentInkAmount);
        if (shouldActivate != _isActive)
        {
            _isActive = shouldActivate;
            if (shouldActivate)
            {
                _onConditionMet.Invoke();
            }
            else
            {
                _onConditionNotMet.Invoke();
            }
        }
    }

    private bool EvaluateCondition(int currentInk)
    {
        switch (_comparison)
        {
            case ComparisonType.Equal:
                return currentInk == _inkNeeded;
            case ComparisonType.LessThan:
                return currentInk < _inkNeeded;
            case ComparisonType.LessThanOrEqual:
                return currentInk <= _inkNeeded;
            case ComparisonType.GreaterThan:
                return currentInk > _inkNeeded;
            case ComparisonType.GreaterThanOrEqual:
                return currentInk >= _inkNeeded;
            default:
                return false;
        }
    }
}
