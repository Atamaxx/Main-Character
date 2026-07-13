using System;
using UnityEngine;

namespace Letters
{
    public class ObjectInkManager : InkManager
    {
        [field: SerializeField] public override int CurrentInkAmount { get; protected set; } = 5;
        [field: SerializeField] public override int MaxInkAmount { get; protected set; } = 5;
        [field: SerializeField] public override int MinInkAmount { get; protected set; } = 0;
        [field: SerializeField] public override bool CanFill { get; protected set; } = true;
        [field: SerializeField] public override bool CanUnfill { get; protected set; } = true;
        public event Action<int> OnInkChanged;


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
                print("Як це можливо!?");
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


        public void UpdateOnInkChange()
        {
            OnInkChanged?.Invoke(CurrentInkAmount);
        }

        #region RESET
        public override void RestoreState(object state)
        {
            base.RestoreState(state);
            UpdateOnInkChange();
        }

        #endregion
    }
}