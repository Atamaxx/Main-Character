using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class CinemachineAspectRatioAdjuster : CinemachineExtension
{
    // Tolerance to determine if the aspect ratio is 16:10
    private const float TOLERANCE = 0.05f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, 
        CinemachineCore.Stage stage, 
        ref CameraState state, 
        float deltaTime)
    {
        // We adjust the lens only in the Body stage
        if (stage == CinemachineCore.Stage.Body)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            
            // Check if we are running on a 16:10 aspect ratio screen
            if (Mathf.Abs(screenAspect - (16f / 10f)) < TOLERANCE)
            {
                if (state.Lens.Orthographic)
                {
                    // For an orthographic camera, the visible width is proportional to:
                    // width = orthographicSize * 2 * aspect.
                    // To maintain width when moving from 16:9 to 16:10, scale the size by (10/9).
                    state.Lens.OrthographicSize *= (10f / 9f);
                }
                else
                {
                    // For perspective cameras, the default vertical FOV is set assuming a 16:9 view.
                    // First, calculate the horizontal FOV that the camera would have with a 16:9 aspect ratio.
                    float defaultVerticalFOV = state.Lens.FieldOfView;
                    float horizontalFOVDefault = 2f * Mathf.Atan(Mathf.Tan(defaultVerticalFOV * Mathf.Deg2Rad / 2f) * (16f / 9f)) * Mathf.Rad2Deg;
                    
                    // Now, compute the new vertical FOV that would produce the same horizontal FOV
                    // when using a 16:10 aspect ratio.
                    float newVerticalFOV = 2f * Mathf.Atan(Mathf.Tan(horizontalFOVDefault * Mathf.Deg2Rad / 2f) / (16f / 10f)) * Mathf.Rad2Deg;
                    state.Lens.FieldOfView = newVerticalFOV;
                }
            }
            // In all other cases (or if the aspect ratio isn’t approximately 16:10), we leave the lens settings unchanged.
        }
    }
}
