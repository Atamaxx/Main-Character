using UnityEngine;
using UnityEngine.Events;

namespace Britney
{
    public class BritneyFrozenEvents : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _onFrozen;

        [SerializeField]
        private UnityEvent _onUnfrozen;

        private IBritneyController _iBritney;

        private void Awake()
        {
            _iBritney = GetComponentInChildren<IBritneyController>();
        }

        private void OnEnable()
        {
            _iBritney.Frozen += OnFrozen;
            _iBritney.Unfrozen += OnUnfrozen;
        }

        private void OnDisable()
        {
            _iBritney.Frozen -= OnFrozen;
            _iBritney.Unfrozen -= OnUnfrozen;
        }

        private void OnFrozen()
        {
            _onFrozen.Invoke();
        }

        private void OnUnfrozen()
        {
            _onUnfrozen.Invoke();
        }
    }
}
