using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class InkVisibilityManager : MonoBehaviour
{
    [Header("Stencil Settings")]
    [SerializeField, Range(0, 255)]
    private int stencilReference = 1;

    [SerializeField]
    private Material inkStencilMaterial;

    [Header("Hidden Objects")]
    [SerializeField]
    private List<GameObject> hiddenObjects = new List<GameObject>();

    [Header("Ink Particle System")]
    [SerializeField]
    private ParticleSystem[] inkParticleSystems;

    private Material instancedInkMaterial;

    private void Awake()
    {
        // Initialize particle systems if not set
        if (inkParticleSystems == null || inkParticleSystems.Length == 0)
        {
            inkParticleSystems = new ParticleSystem[1];
            inkParticleSystems[0] = GetComponent<ParticleSystem>();
        }

        // Create material instance to avoid affecting other objects
        if (inkStencilMaterial != null)
        {
            instancedInkMaterial = new Material(inkStencilMaterial);
            instancedInkMaterial.SetInt("_StencilRef", stencilReference);

            // Apply to all particle systems
            foreach (var ps in inkParticleSystems)
            {
                if (ps != null)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = instancedInkMaterial;
                    }
                }
            }
        }

        // Set up hidden objects
        foreach (var obj in hiddenObjects)
        {
            MakeVisibleOnlyInInk(obj);
        }
    }

    // Call this to add objects that should only be visible within ink
    public void MakeVisibleOnlyInInk(GameObject obj)
    {
        if (obj == null)
            return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Custom/VisibleInInk"));
            mat.SetInt("_StencilRef", stencilReference);
            renderer.material = mat;
        }
    }

    // Integration with BritneyInkManager
    public void RegisterInkParticleSystems(params ParticleSystem[] particleSystems)
    {
        if (particleSystems == null || particleSystems.Length == 0)
            return;

        // Create a new array with combined size
        int currentSize = inkParticleSystems?.Length ?? 0;
        int newSize = currentSize + particleSystems.Length;
        ParticleSystem[] newArray = new ParticleSystem[newSize];

        // Copy existing systems
        if (inkParticleSystems != null)
        {
            System.Array.Copy(inkParticleSystems, newArray, currentSize);
        }

        // Add new systems
        System.Array.Copy(particleSystems, 0, newArray, currentSize, particleSystems.Length);
        inkParticleSystems = newArray;

        // Apply material to newly added systems
        if (instancedInkMaterial != null)
        {
            for (int i = currentSize; i < newSize; i++)
            {
                var renderer = inkParticleSystems[i].GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.material = instancedInkMaterial;
                }
            }
        }
    }

    // Example of integrating with HittableLetter
    public void RegisterExplosionParticles(ParticleSystem explosion)
    {
        if (explosion != null)
        {
            RegisterInkParticleSystems(explosion);
        }
    }
}
