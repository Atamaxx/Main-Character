using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class InkFiller : MonoBehaviour
{
    [BoxGroup("General Settings"), SerializeField] private bool _pause;
    [BoxGroup("General Settings"), SerializeField] private InkManager _inkManager;
    [BoxGroup("General Settings"), SerializeField] private CheckIfUnderCollider _underCheck;
    [BoxGroup("General Settings"), SerializeField] private Transform _playerTransform;
    [BoxGroup("General Settings"), SerializeField] private float _rangeThreshold = 5f;
    [BoxGroup("General Settings"), SerializeField] private Transform _inkfillObject;

    [BoxGroup("Ink Settings"), SerializeField] private int _maxInkAmount = 3;
    [BoxGroup("Ink Settings"), SerializeField] private int _currentInkAmount = 3;
    [BoxGroup("Ink Settings"), SerializeField] private int _restoreAmount = 1;
    [BoxGroup("Ink Settings"), SerializeField] private int _useAmount = 1;
    [BoxGroup("Ink Settings"), SerializeField] private float _inkRefillDelay = 0.4f;
    [BoxGroup("Ink Settings"), SerializeField] private float _inkUnfillDelay = 0.4f;

    [BoxGroup("Refill & Unfill Settings"), SerializeField] private bool _canRefill = true;
    [BoxGroup("Refill & Unfill Settings"), SerializeField] private bool _canUnfill = true;
    [BoxGroup("Refill & Unfill Settings"), SerializeField, EnumFlags] private ConditionType _refillConditions;
    [BoxGroup("Refill & Unfill Settings"), SerializeField, EnumFlags] private ConditionType _unfillConditions;
    [BoxGroup("Refill & Unfill Settings"), SerializeField] private LogicOperator _refillLogicOperator = LogicOperator.Or;
    [BoxGroup("Refill & Unfill Settings"), SerializeField] private LogicOperator _unfillLogicOperator = LogicOperator.Or;
    [BoxGroup("Events")]
    public UnityEvent<float, float> OnInkAmountChanged;

    private bool _isRefilling = false;
    private bool _isUnfilling = false;

    public enum ConditionType
    {
        None = 0,
        Always = 1 << 0,
        UnderCollider = 1 << 1,
        NotUnderCollider = 1 << 2,
        InRange = 1 << 3,
        NotInRange = 1 << 4,
        Custom = 1 << 5
    }

    public enum LogicOperator
    {
        Or,
        And
    }

    private void Update()
    {
        bool canRefill = _canRefill && !_pause && CheckConditions(_refillConditions, _refillLogicOperator) && !_isRefilling;
        bool canUnfill = _canUnfill && !_pause && CheckConditions(_unfillConditions, _unfillLogicOperator) && !_isUnfilling;

        if (canRefill)
        {
            StartCoroutine(RefillInkWithDelay());
        }
        if (canUnfill)
        {
            StartCoroutine(UnfillInkWithDelay());
        }
    }

    private bool CheckConditions(ConditionType conditions, LogicOperator logicOperator)
    {
        if (conditions == ConditionType.None)
            return false;

        if (logicOperator == LogicOperator.Or)
        {
            if ((conditions & ConditionType.Always) != 0)
                return true;
            if ((conditions & ConditionType.UnderCollider) != 0 && (_underCheck == null || _underCheck.IsUnder))
                return true;
            if ((conditions & ConditionType.NotUnderCollider) != 0 && (_underCheck == null || !_underCheck.IsUnder))
                return true;
            if ((conditions & ConditionType.InRange) != 0 && IsInRange())
                return true;
            if ((conditions & ConditionType.NotInRange) != 0 && !IsInRange())
                return true;
            if ((conditions & ConditionType.Custom) != 0 && CustomConditionCheck())
                return true;

            return false;
        }
        else // And logic
        {
            // The Always flag is inherently true so we can ignore it in an AND check.
            if ((conditions & ConditionType.UnderCollider) != 0 && !(_underCheck == null || _underCheck.IsUnder))
                return false;
            if ((conditions & ConditionType.NotUnderCollider) != 0 && !(_underCheck == null || !_underCheck.IsUnder))
                return false;
            if ((conditions & ConditionType.InRange) != 0 && !IsInRange())
                return false;
            if ((conditions & ConditionType.NotInRange) != 0 && IsInRange())
                return false;
            if ((conditions & ConditionType.Custom) != 0 && !CustomConditionCheck())
                return false;

            return true;
        }
    }

    private bool IsInRange()
    {
        if (_playerTransform == null)
            return false;

        return Vector3.Distance(_inkfillObject.position, _playerTransform.position) <= _rangeThreshold;
    }

    private bool CustomConditionCheck()
    {
        // Implement custom logic here
        return true;
    }

    private IEnumerator RefillInkWithDelay()
    {
        _isRefilling = true;

        if (_currentInkAmount > 0 && _inkManager.CurrentInkAmount < _inkManager.MaxInkAmount)
        {
            _inkManager.RestoreInk(_restoreAmount);
            _currentInkAmount--;
            OnInkAmountChanged?.Invoke(_currentInkAmount, _maxInkAmount);
        }
        yield return new WaitForSeconds(_inkRefillDelay);

        _isRefilling = false;
    }

    private IEnumerator UnfillInkWithDelay()
    {
        _isUnfilling = true;

        if (_inkManager.CurrentInkAmount > 0 && _currentInkAmount < _maxInkAmount)
        {
            _inkManager.UseInk(_useAmount);
            _currentInkAmount++;
            OnInkAmountChanged?.Invoke(_currentInkAmount, _maxInkAmount);
        }
        yield return new WaitForSeconds(_inkUnfillDelay);

        _isUnfilling = false;
    }

    public void Stop() => _pause = true;

    public void Resume() => _pause = false;

    private void OnDrawGizmos()
    {
        if (_playerTransform != null && _inkfillObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_inkfillObject.position, _rangeThreshold);
        }
    }
}
