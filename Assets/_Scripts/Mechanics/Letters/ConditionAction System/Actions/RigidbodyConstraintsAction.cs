
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
namespace Letters
{
    public class RigidbodyConstraintsAction : Action
    {
        [SerializeField] private List<Rigidbody2D> _rigidbodies = new();

        [BoxGroup("CONDITIONS MET"), SerializeField] private bool _freezeXOnMet;
        [BoxGroup("CONDITIONS MET"), SerializeField] private bool _freezeYOnMet;
        [BoxGroup("CONDITIONS MET"), SerializeField] private bool _freezeRotationOnMet;
        [BoxGroup("CONDITIONS NOT MET"), SerializeField] private bool _freezeXOnNotMet;
        [BoxGroup("CONDITIONS NOT MET"), SerializeField] private bool _freezeYOnNotMet;
        [BoxGroup("CONDITIONS NOT MET"), SerializeField] private bool _freezeRotationOnNotMet;

        public override void OnConditionMet()
        {
            ConstraintsSet(_freezeXOnMet, _freezeYOnMet, _freezeRotationOnMet);
            
        }

        public override void OnConditionNotMet()
        {
            ConstraintsSet(_freezeXOnNotMet, _freezeYOnNotMet, _freezeRotationOnNotMet);
        }

        public void ConstraintsSet(bool freezePositionX, bool freezePositionY, bool freezeRotation)
        {
            RigidbodyConstraints2D constraints = RigidbodyConstraints2D.None;

            if (freezePositionX)
            {
                constraints |= RigidbodyConstraints2D.FreezePositionX;
            }
            if (freezePositionY)
            {
                constraints |= RigidbodyConstraints2D.FreezePositionY;
            }
            if (freezeRotation)
            {
                constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            foreach (Rigidbody2D rigidbody in _rigidbodies)
            {
                rigidbody.constraints = constraints;
            }
        }
    }
}
