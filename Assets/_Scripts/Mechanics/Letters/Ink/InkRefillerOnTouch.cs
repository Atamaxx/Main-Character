using System.Collections;
using UnityEngine;

namespace Letters
{
    public class InkRefillerOnTouch : MonoBehaviour
    {
        [SerializeField] private bool _isTrigger = false;
        [SerializeField] private int _maxInkAmount = 3;
        [SerializeField] private int _currentInkAmount = 3;
        [SerializeField] private float _inkRefillDelay = 0.4f;
        [SerializeField] private InkManager _inkManager;
        private bool _isRefilling = false;

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (_isTrigger && collision.collider.CompareTag("Player") && _currentInkAmount > 0 && _inkManager.CurrentInkAmount < _inkManager.MaxInkAmount)
                if (!_isRefilling)
                {
                    StartCoroutine(RefillInkWithDelay());
                }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_isTrigger && other.CompareTag("Player") && _currentInkAmount > 0 && _inkManager.CurrentInkAmount < _inkManager.MaxInkAmount)
                if (!_isRefilling)
                {
                    StartCoroutine(RefillInkWithDelay());
                }
        }

        private IEnumerator RefillInkWithDelay()
        {
            _isRefilling = true;

            if (_currentInkAmount > 0 && _inkManager.CurrentInkAmount < _inkManager.MaxInkAmount)
            {
                _inkManager.RestoreInk(1);
                _currentInkAmount--;
            }
            yield return new WaitForSeconds(_inkRefillDelay);

            _isRefilling = false;
        }

    }
}