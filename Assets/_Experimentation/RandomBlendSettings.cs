using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
public class RandomBlendSettings : MonoBehaviour
{
    /// <summary>
    /// Optional: Assign a specific material via the Inspector.
    /// If left null, the script will use the Renderer’s material.
    /// </summary>
    [Tooltip("Assign the material to modify. If left empty, the Renderer’s material will be used.")]
    public Material targetMaterial;
    private BlendMode[] blendModes;
    private BlendOp[] blendOps;
    /// <summary>
    /// Initializes the script by setting random blend modes.
    /// </summary>


    /// <summary>
    /// Randomly selects and assigns values to _SrcFactor, _DstFactor, and _Operation.
    /// </summary>
    [Button("SetRandomBlendModes")]
    public void SetRandomBlendModes()
    {


        // Get all possible values for BlendMode and BlendOp enums
        BlendMode[] blendModes = (BlendMode[])System.Enum.GetValues(typeof(BlendMode));
        BlendOp[] blendOps = (BlendOp[])System.Enum.GetValues(typeof(BlendOp));

        // Select random BlendMode values for source and destination factors
        BlendMode srcFactor = blendModes[Random.Range(0, blendModes.Length)];
        BlendMode dstFactor = blendModes[Random.Range(0, blendModes.Length)];

        // Select a random BlendOp value for the operation
        BlendOp operation = blendOps[Random.Range(0, blendOps.Length)];

        // Assign the selected values to the shader properties
        targetMaterial.SetFloat("_SrcFactor", (float)srcFactor);
        targetMaterial.SetFloat("_DstFactor", (float)dstFactor);
        targetMaterial.SetFloat("_Operation", (float)operation);

        // Optional: Log the selected values for debugging purposes
        Debug.Log($"[RandomBlendSettings] Set _SrcFactor to {srcFactor}, _DstFactor to {dstFactor}, _Operation to {operation}");
    }

    /// <summary>
    /// Cycles to the next BlendMode for _SrcFactor.
    /// </summary>
    /// 
    private void Start()
    {
        blendModes = (BlendMode[])System.Enum.GetValues(typeof(BlendMode));
        blendOps = (BlendOp[])System.Enum.GetValues(typeof(BlendOp));
    }
    [Button]
    public void NextSrcFactor()
    {


        // Get current _SrcFactor value
        BlendMode current = (BlendMode)(int)targetMaterial.GetFloat("_SrcFactor");

        // Find the index of the current value
        int currentIndex = System.Array.IndexOf(blendModes, current);
        if (currentIndex == -1)
        {
            currentIndex = 0;
        }

        // Calculate the next index with wrap-around
        int nextIndex = (currentIndex + 1) % blendModes.Length;
        BlendMode next = blendModes[nextIndex];

        // Assign the next value to the shader property
        targetMaterial.SetFloat("_SrcFactor", (float)next);

        Debug.Log($"[RandomBlendSettings] _SrcFactor changed to {next}");
    }

    /// <summary>
    /// Cycles to the next BlendMode for _DstFactor.
    /// </summary>
    [Button]
    public void NextDstFactor()
    {


        // Get current _DstFactor value
        BlendMode current = (BlendMode)(int)targetMaterial.GetFloat("_DstFactor");

        // Find the index of the current value
        int currentIndex = System.Array.IndexOf(blendModes, current);
        if (currentIndex == -1)
        {
            currentIndex = 0;
        }

        // Calculate the next index with wrap-around
        int nextIndex = (currentIndex + 1) % blendModes.Length;
        BlendMode next = blendModes[nextIndex];

        // Assign the next value to the shader property
        targetMaterial.SetFloat("_DstFactor", (float)next);

        Debug.Log($"[RandomBlendSettings] _DstFactor changed to {next}");
    }

    /// <summary>
    /// Cycles to the next BlendOp for _Operation.
    /// </summary>
    [Button]
    public void NextOperation()
    {


        // Get current _Operation value
        BlendOp current = (BlendOp)(int)targetMaterial.GetFloat("_Operation");

        // Find the index of the current value
        int currentIndex = System.Array.IndexOf(blendOps, current);
        if (currentIndex == -1)
        {
            currentIndex = 0;
        }

        // Calculate the next index with wrap-around
        int nextIndex = (currentIndex + 1) % blendOps.Length;
        BlendOp next = blendOps[nextIndex];

        // Assign the next value to the shader property
        targetMaterial.SetFloat("_Operation", (float)next);

        Debug.Log($"[RandomBlendSettings] _Operation changed to {next}");
    }
}
