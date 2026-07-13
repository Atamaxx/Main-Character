using System;
using Britney;
using UnityEngine;

namespace Letters
{
    public class BritneyInkManager : InkManager
    {
        [field: SerializeField]
        public override int CurrentInkAmount { get; protected set; } = 5;

        [field: SerializeField]
        public override int MaxInkAmount { get; protected set; } = 5;

        [field: SerializeField]
        public override int MinInkAmount { get; protected set; } = 0;

        [field: SerializeField]
        public override bool CanFill { get; protected set; } = true;

        [field: SerializeField]
        public override bool CanUnfill { get; protected set; } = true;

        [SerializeField]
        private BritneyToLife _visuals;
        public event Action<int> OnInkChanged;

        // Called from InkManager.OnStatefulAwake via OnStart.
        protected override void OnAwake()
        {
            Invoke(nameof(UpdateOnInkChange), 0.1f);
        }

        public void UpdateOnInkChange()
        {
            OnInkChanged?.Invoke(CurrentInkAmount);
            _visuals.RedrawBody(MaxInkAmount - CurrentInkAmount, MaxInkAmount);
        }

        public override bool CanUseInk(int amount)
        {
            return CurrentInkAmount - amount >= MinInkAmount;
        }

        public override bool CanRestoreInk(int amount)
        {
            return CurrentInkAmount + amount <= MaxInkAmount;
        }

        public override void UseInk(int amount)
        {
            int changes = CurrentInkAmount;
            CurrentInkAmount -= amount;

            if (CurrentInkAmount < MinInkAmount)
            {
                Debug.LogError("Як це можливо!?");
                CurrentInkAmount = changes;
            }

            if (changes != CurrentInkAmount)
                UpdateOnInkChange();
        }

        public override void RestoreInk(int amount)
        {
            int changes = CurrentInkAmount;
            CurrentInkAmount += amount;
            CurrentInkAmount = Mathf.Clamp(CurrentInkAmount, MinInkAmount, MaxInkAmount);

            if (changes != CurrentInkAmount)
                UpdateOnInkChange();
        }

        public void SetInk(int amount)
        {
            CurrentInkAmount = Mathf.Clamp(amount, MinInkAmount, MaxInkAmount);
            UpdateOnInkChange();
        }

        #region RESET

        public override void RestoreState(object state)
        {
            // Restore the base state first.
            base.RestoreState(state);
            // Update visuals after state restoration.
            UpdateOnInkChange();
        }

        #endregion
    }
}
