using System.Collections.Generic;
using UnityEngine;

namespace Letters
{
    public class PlayParticlesAction : Action
    {
        [SerializeField] private bool _playParticles = true;
        [SerializeField] private List<ParticleSystem> _particleSystems = new();

        public override void OnConditionMet()
        {
            if (_playParticles)
            {
                PlayParticleEffect();
            }
            else
                StopParticleEffect();
        }

        public override void OnConditionNotMet()
        {
            if (_playParticles)
            {
                StopParticleEffect();
            }
            else
                PlayParticleEffect();
        }



        public void PlayParticleEffect()
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem != null && !particleSystem.isPlaying)
                {
                    particleSystem.Play();
                }
            }
        }
        public void StopParticleEffect()
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem != null && !particleSystem.isPlaying)
                {
                    particleSystem.Stop();
                }
            }
        }
    }
}