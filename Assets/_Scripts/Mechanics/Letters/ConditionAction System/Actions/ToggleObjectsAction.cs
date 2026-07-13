using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
namespace Letters
{
    public class ToggleObjectsAction : Action
    {
        [SerializeField] private bool _toggleOn = true;
        [SerializeField] private List<GameObject> _objectsToToggle = new();

        public override void OnConditionMet()
        {
            if (_toggleOn)
            {
                ActivateObjects();
            }
            else
            {
                DeactivateObjects();
            }
        }

        public override void OnConditionNotMet()
        {
            if (_toggleOn)
            {
                DeactivateObjects();
            }
            else
            {
                ActivateObjects();
            }
        }

        private void ActivateObjects()
        {
            foreach (GameObject obj in _objectsToToggle)
            {
                obj.SetActive(true);
            }
        }

        private void DeactivateObjects()
        {
            foreach (GameObject obj in _objectsToToggle)
            {
                obj.SetActive(false);
            }
        }



    }
}