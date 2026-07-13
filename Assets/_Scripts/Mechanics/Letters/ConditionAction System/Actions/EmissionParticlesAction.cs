using System.Collections.Generic;
using UnityEngine;

namespace Letters
{
    public class EmissionParticlesAction : Action
    {
        [SerializeField] private float _offEmission = 0f;
        [SerializeField] private float _onEmission = 10f;
        [SerializeField] private List<ParticleSystem> _particleSystems = new();

        public override void OnConditionMet()
        {
            On_EmissionRate();
        }

        public override void OnConditionNotMet()
        {
            Off_EmissionRate();
        }


        public void On_EmissionRate()
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem != null)
                {
                    var emissionModule = particleSystem.emission;
                    emissionModule.rateOverTime = _onEmission;
                }
            }
        }
        public void Off_EmissionRate()
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem != null)
                {
                    var emissionModule = particleSystem.emission;
                    emissionModule.rateOverTime = _offEmission;
                }
            }
        }
    }
}