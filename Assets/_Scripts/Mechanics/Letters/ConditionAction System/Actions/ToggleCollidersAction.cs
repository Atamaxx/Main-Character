using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Letters
{
    public class ToggleCollidersAction : Action
    {
        [SerializeField] private bool _toggleOn = true;
        [SerializeField] private List<Collider2D> _colliders = new();

        public override void OnConditionMet()
        {
            if (_toggleOn)
            {
                ActivateColliders();
            }
            else
            {
                DeactivateColliders();
            }
        }

        public override void OnConditionNotMet()
        {
            if (_toggleOn)
            {
                DeactivateColliders();
            }
            else
            {
                ActivateColliders();
            }
        }

        private void ActivateColliders()
        {
            foreach (Collider2D collider in _colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        private void DeactivateColliders()
        {
            foreach (Collider2D collider in _colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }
}
