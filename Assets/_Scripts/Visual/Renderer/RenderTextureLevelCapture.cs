using System.Collections;
using UnityEngine;
using NaughtyAttributes;

public class RenderTextureLevelCapture : MonoBehaviour
{
    [Header("Capture Settings")]
    [Tooltip("Camera used to capture the current level view. Defaults to main camera if left empty.")]
    [SerializeField, Required] private RenderTexture _renderTexture;
    [SerializeField, Required] private Camera _captureCamera;
    [Button("Capture Level")]
    public void CaptureLevel()
    {
        if (_captureCamera == null)
        {
            Debug.LogError("Cannot capture level - no camera assigned!");
            return;
        }
        if (_renderTexture == null)
        {
            Debug.LogError("Cannot capture level - no render texture assigned!");
            return;
        }
        print("Level capture started...");
        // Update the render texture's resolution to match the screen (or camera) resolution.
        int width = _captureCamera.pixelWidth;
        int height = _captureCamera.pixelHeight;
        _renderTexture.Release(); // Release the render texture so we can change its dimensions.
        _renderTexture.width = width;
        _renderTexture.height = height;
        _renderTexture.Create();  // Recreate the render texture with new settings.

        // Store original target
        RenderTexture originalTarget = _captureCamera.targetTexture;

        // Assign the RenderTexture to the camera and force a render.
        _captureCamera.targetTexture = _renderTexture;
        _captureCamera.Render();

        // Make the RenderTexture active so we can read from it.
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = _renderTexture;

        // Reset the camera and active RenderTexture.
        _captureCamera.targetTexture = originalTarget;
        RenderTexture.active = currentActiveRT;

        Debug.Log("Level capture complete!");
    }

    /// <summary>
    /// Get the render texture used for capturing
    /// </summary>
    public RenderTexture GetRenderTexture()
    {
        return _renderTexture;
    }
}