using System.Collections.Generic;
using UnityEngine;

namespace Britney
{
    public class BritneyManager : StaticInstance<BritneyManager>
    {
        [SerializeField]
        private List<ManagerSettings> _managerSettings = new();

        [SerializeField]
        private float _moveMultiplierMax = 2;

        [SerializeField]
        private float _animMultiplierMax = 2;

        [SerializeField]
        private Letters.BritneyInkManager _inkManager;

        [SerializeField]
        private BritneyMovement _britneyMovement;

        [SerializeField]
        private BritneyToLife _britneyToLife;

        private enum PlayerState
        {
            Empty,
            One,
            Two,
            Three,
            Four,
            Full,
        }

        private PlayerState _currentState;

        // Public access properties to Britney components
        public BritneyMovement BritneyMovement => _britneyMovement;
        public BritneyToLife BritneyToLife => _britneyToLife;
        public Letters.BritneyInkManager InkManager => _inkManager;

        private void OnEnable()
        {
            UpdateState(_currentState);
            _inkManager.OnInkChanged += HandleInkChanged;
        }

        private void OnDisable()
        {
            _inkManager.OnInkChanged -= HandleInkChanged;
        }

        private void HandleInkChanged(int newInk)
        {
            // 1. Determine state from ink
            var newState = DetermineStateFromInk(newInk);

            // 2. Only update if it differs from current
            if (newState != _currentState)
            {
                _currentState = newState;
                UpdateState(_currentState);
            }
        }

        private PlayerState DetermineStateFromInk(int ink)
        {
            if (ink == 0)
                return PlayerState.Empty;
            if (ink == 1)
                return PlayerState.One;
            if (ink == 2)
                return PlayerState.Two;
            if (ink == 3)
                return PlayerState.Three;
            if (ink == 4)
                return PlayerState.Four;

            return PlayerState.Full;
        }

        private void UpdateState(PlayerState state)
        {
            // Switch or direct index
            switch (state)
            {
                case PlayerState.Empty:
                    ChangeSettings(0);
                    break;
                case PlayerState.One:
                    ChangeSettings(1);
                    break;
                case PlayerState.Two:
                    ChangeSettings(2);
                    break;
                case PlayerState.Three:
                    ChangeSettings(3);
                    break;
                case PlayerState.Four:
                    ChangeSettings(4);
                    break;
                case PlayerState.Full:
                    ChangeSettings(5);
                    break;
            }
        }

        private void ChangeSettings(int inkAmount)
        {
            _britneyMovement.Stats.MoveMultiplier =
                _moveMultiplierMax - inkAmount * (_moveMultiplierMax / 6);
            _britneyToLife.AnimationSO.SpeedBaseMultiplier =
                _animMultiplierMax - inkAmount * (_moveMultiplierMax / 6);
            // _britneyToLife.SoundSO = _managerSettings[inkAmount].SoundParams;
            // _britneyToLife.AnimationSO = _managerSettings[inkAmount].AnimationParams;
            // _britneyToLife.VfxSO = _managerSettings[inkAmount].VFXParams;
        }
    }
}
