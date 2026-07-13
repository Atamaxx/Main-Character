using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    // Target aspect ratio: 16:9
    private const float targetAspect = 16f / 9f;
    private Camera _camera;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        UpdateCameraViewport();
    }

    void Update()
    {
        // Check if the screen resolution has changed
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            UpdateCameraViewport();
        }
    }

    void UpdateCameraViewport()
    {
        // Calculate the current window aspect ratio
        float windowAspect = (float)Screen.width / Screen.height;
        // Calculate scale height relative to the target aspect ratio
        float scaleHeight = windowAspect / targetAspect;
        Rect rect = _camera.rect;

        if (scaleHeight < 1.0f)
        {
            // Screen is too tall; add letterboxing (black bars on top and bottom)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // Screen is too wide; add pillarboxing (black bars on the sides)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        _camera.rect = rect;
    }
}
