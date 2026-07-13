using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StatefulParticleSystem : StatefulMonoBehaviour
{
    [SerializeField]
    private ParticleSystem _particleSystem;

    // Simple data structure that only stores what we absolutely need
    [Serializable]
    private class ParticleState
    {
        // We only store the raw particle data and nothing else
        public byte[] serializedParticles;
    }

    private void Reset()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    protected override void OnStatefulAwake()
    {
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();
    }

    public override object CaptureState()
    {
        // Get the live particles from the system
        var particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int count = _particleSystem.GetParticles(particles, particles.Length);

        if (count == 0)
            return new ParticleState { serializedParticles = new byte[0] };

        // Serialize the particles to a byte array
        int particleSize = GetParticleSize();
        byte[] serializedParticles = new byte[count * particleSize];

        // Copy memory from particles array to byte array
        System.Runtime.InteropServices.GCHandle handle =
            System.Runtime.InteropServices.GCHandle.Alloc(
                particles,
                System.Runtime.InteropServices.GCHandleType.Pinned
            );
        System.Runtime.InteropServices.Marshal.Copy(
            handle.AddrOfPinnedObject(),
            serializedParticles,
            0,
            count * particleSize
        );
        handle.Free();

        return new ParticleState { serializedParticles = serializedParticles };
    }

    public override void RestoreState(object state)
    {
        if (!(state is ParticleState savedState) || savedState.serializedParticles.Length == 0)
        {
            _particleSystem.Clear();
            return;
        }

        // First, clear existing particles
        _particleSystem.Clear();

        // Get particle size and calculate count
        int particleSize = GetParticleSize();
        int count = savedState.serializedParticles.Length / particleSize;

        // Create a new array of particles
        var particles = new ParticleSystem.Particle[count];

        // Deserialize the byte array back to particles
        System.Runtime.InteropServices.GCHandle handle =
            System.Runtime.InteropServices.GCHandle.Alloc(
                particles,
                System.Runtime.InteropServices.GCHandleType.Pinned
            );
        System.Runtime.InteropServices.Marshal.Copy(
            savedState.serializedParticles,
            0,
            handle.AddrOfPinnedObject(),
            savedState.serializedParticles.Length
        );
        handle.Free();

        // Set the particles back to the system
        _particleSystem.SetParticles(particles, count);
    }

    // Helper method to calculate the size of a particle structure
    private int GetParticleSize()
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(ParticleSystem.Particle));
    }
}
