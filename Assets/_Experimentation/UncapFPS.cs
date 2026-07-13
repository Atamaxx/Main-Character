using UnityEngine;

[ExecuteInEditMode]
public class UncapFPS : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        // Disable VSync to prevent it from capping the FPS.
        QualitySettings.vSyncCount = 0;
        // Remove any target frame rate limit.
        Application.targetFrameRate = 300;
#endif
    }
}
