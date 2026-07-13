using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using NaughtyAttributes;

/// <summary>
/// Manages Cinemachine virtual cameras to be controlled through Unity Events.
/// This class provides methods to manage camera activation/deactivation, priorities,
/// and transitioning between cameras.
/// </summary>
public class CinemachineManager : MonoBehaviour
{
    [SerializeField, Required] private CinemachineBrain _cinemachineBrain;
    [Tooltip("List of Cinemachine Virtual Cameras to manage")]
    [SerializeField] private List<CinemachineCamera> virtualCameras = new List<CinemachineCamera>();

    [Tooltip("Default priority for cameras that are enabled but not actively controlling the view")]
    [SerializeField] private int defaultCameraPriority = 10;

    [Tooltip("Priority for the currently active camera")]
    [SerializeField] private int activeCameraPriority = 20;

    [Tooltip("Priority for disabled cameras")]
    [SerializeField] private int disabledCameraPriority = 0;

    // Track the currently active camera
    private CinemachineCamera currentActiveCamera;

    void Start()
    {
        _cinemachineBrain.IgnoreTimeScale = true;
    }

    /// <summary>
    /// Populate the list with all virtual cameras in the scene.
    /// Useful for automatic setup.
    /// </summary>
    public void FindAllCamerasInScene()
    {
        virtualCameras.Clear();
        CinemachineCamera[] sceneCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.InstanceID);
        virtualCameras.AddRange(sceneCameras);
    }

    /// <summary>
    /// Add a camera to the managed list.
    /// </summary>
    /// <param name="camera">The Cinemachine virtual camera to add</param>
    public void AddCamera(CinemachineCamera camera)
    {
        if (camera != null && !virtualCameras.Contains(camera))
        {
            virtualCameras.Add(camera);
            camera.Priority = defaultCameraPriority;
        }
    }

    /// <summary>
    /// Remove a camera from the managed list.
    /// </summary>
    /// <param name="camera">The Cinemachine virtual camera to remove</param>
    public void RemoveCamera(CinemachineCamera camera)
    {
        if (camera != null && virtualCameras.Contains(camera))
        {
            virtualCameras.Remove(camera);
        }
    }

    /// <summary>
    /// Switch to a camera by its index in the list.
    /// Disables all other cameras.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    public void SwitchToCamera(int index)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            SwitchToCamera(virtualCameras[index]);
        }
        else
        {
            Debug.LogWarning("Camera index out of range: " + index);
        }
    }

    /// <summary>
    /// Switch to a specific camera by reference.
    /// Disables all other cameras.
    /// </summary>
    /// <param name="targetCamera">The camera to switch to</param>
    public void SwitchToCamera(CinemachineCamera targetCamera)
    {
        if (targetCamera == null || !virtualCameras.Contains(targetCamera))
        {
            Debug.LogWarning("Camera not found in the managed list");
            return;
        }

        // Set all cameras to disabled priority
        foreach (var camera in virtualCameras)
        {
            if (camera != null)
            {
                camera.Priority = disabledCameraPriority;
            }
        }

        // Set target camera to active priority
        targetCamera.Priority = activeCameraPriority;
        currentActiveCamera = targetCamera;
    }

    /// <summary>
    /// Get the currently active camera.
    /// </summary>
    /// <returns>The currently active Cinemachine virtual camera</returns>
    public CinemachineCamera GetActiveCamera()
    {
        return currentActiveCamera;
    }

    /// <summary>
    /// Set the priority of a camera by index.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    /// <param name="priority">The new priority value</param>
    public void SetCameraPriority(int index, int priority)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            virtualCameras[index].Priority = priority;

            // Update current active camera if this becomes the highest priority
            if (priority > activeCameraPriority)
            {
                currentActiveCamera = virtualCameras[index];
                activeCameraPriority = priority;
            }
        }
    }

    /// <summary>
    /// Set the priority of a specific camera.
    /// </summary>
    /// <param name="camera">The camera to modify</param>
    /// <param name="priority">The new priority value</param>
    public void SetCameraPriority(CinemachineCamera camera, int priority)
    {
        if (camera != null && virtualCameras.Contains(camera))
        {
            camera.Priority = priority;

            // Update current active camera if this becomes the highest priority
            if (priority > activeCameraPriority && camera != currentActiveCamera)
            {
                currentActiveCamera = camera;
                activeCameraPriority = priority;
            }
        }
    }

    /// <summary>
    /// Enable all cameras by setting them to default priority.
    /// </summary>
    public void EnableAllCameras()
    {
        foreach (var camera in virtualCameras)
        {
            if (camera != null)
            {
                camera.Priority = defaultCameraPriority;
            }
        }

        // Clear current active camera since multiple may be enabled
        currentActiveCamera = null;
    }

    /// <summary>
    /// Disable all cameras by setting them to disabled priority.
    /// </summary>
    public void DisableAllCameras()
    {
        foreach (var camera in virtualCameras)
        {
            if (camera != null)
            {
                camera.Priority = disabledCameraPriority;
            }
        }

        currentActiveCamera = null;
    }

    /// <summary>
    /// Enable a specific camera by index.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    public void EnableCamera(int index)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            virtualCameras[index].Priority = defaultCameraPriority;
        }
    }

    /// <summary>
    /// Disable a specific camera by index.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    public void DisableCamera(int index)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            virtualCameras[index].Priority = disabledCameraPriority;

            // Clear current active camera reference if this was the active camera
            if (currentActiveCamera == virtualCameras[index])
            {
                currentActiveCamera = null;
            }
        }
    }

    /// <summary>
    /// Enable a specific camera.
    /// </summary>
    /// <param name="camera">The camera to enable</param>
    public void EnableCamera(CinemachineCamera camera)
    {
        if (camera != null && virtualCameras.Contains(camera))
        {
            camera.Priority = defaultCameraPriority;
        }
    }

    /// <summary>
    /// Disable a specific camera.
    /// </summary>
    /// <param name="camera">The camera to disable</param>
    public void DisableCamera(CinemachineCamera camera)
    {
        if (camera != null && virtualCameras.Contains(camera))
        {
            camera.Priority = disabledCameraPriority;

            // Clear current active camera reference if this was the active camera
            if (currentActiveCamera == camera)
            {
                currentActiveCamera = null;
            }
        }
    }

    /// <summary>
    /// Get camera by index.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    /// <returns>The requested camera or null if index is invalid</returns>
    public CinemachineCamera GetCamera(int index)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            return virtualCameras[index];
        }
        return null;
    }


    /// <summary>
    /// Get the index of a specific camera.
    /// </summary>
    /// <param name="camera">The camera to find</param>
    /// <returns>Index of the camera or -1 if not found</returns>
    public int GetCameraIndex(CinemachineCamera camera)
    {
        return virtualCameras.IndexOf(camera);
    }

    /// <summary>
    /// Get the number of cameras in the managed list.
    /// </summary>
    /// <returns>Number of cameras in the list</returns>
    public int GetCameraCount()
    {
        return virtualCameras.Count;
    }

    /// <summary>
    /// Reset all camera priorities to default.
    /// </summary>
    public void ResetAllCameraPriorities()
    {
        foreach (var camera in virtualCameras)
        {
            if (camera != null)
            {
                camera.Priority = defaultCameraPriority;
            }
        }
        currentActiveCamera = null;
    }

    /// <summary>
    /// Switch to the next camera in the list.
    /// </summary>
    public void SwitchToNextCamera()
    {
        if (virtualCameras.Count == 0) return;

        int currentIndex = -1;

        if (currentActiveCamera != null)
        {
            currentIndex = virtualCameras.IndexOf(currentActiveCamera);
        }

        int nextIndex = (currentIndex + 1) % virtualCameras.Count;
        SwitchToCamera(nextIndex);
    }

    /// <summary>
    /// Switch to the previous camera in the list.
    /// </summary>
    public void SwitchToPreviousCamera()
    {
        if (virtualCameras.Count == 0) return;

        int currentIndex = -1;

        if (currentActiveCamera != null)
        {
            currentIndex = virtualCameras.IndexOf(currentActiveCamera);
        }

        int previousIndex = (currentIndex - 1 + virtualCameras.Count) % virtualCameras.Count;
        SwitchToCamera(previousIndex);
    }

    /// <summary>
    /// Get a camera by its name.
    /// </summary>
    /// <param name="cameraName">Name of the camera to find</param>
    /// <returns>The requested camera or null if not found</returns>
    public CinemachineCamera GetCameraByName(string cameraName)
    {
        return virtualCameras.Find(cam => cam != null && cam.name == cameraName);
    }

    /// <summary>
    /// Switch to a camera by its name.
    /// </summary>
    /// <param name="cameraName">Name of the camera to switch to</param>
    public void SwitchToCameraByName(string cameraName)
    {
        CinemachineCamera camera = GetCameraByName(cameraName);
        if (camera != null)
        {
            SwitchToCamera(camera);
        }
        else
        {
            Debug.LogWarning("Camera not found with name: " + cameraName);
        }
    }

    /// <summary>
    /// Toggle a camera's enabled state by index.
    /// </summary>
    /// <param name="index">Index of the camera in the virtualCameras list</param>
    public void ToggleCamera(int index)
    {
        if (index >= 0 && index < virtualCameras.Count)
        {
            CinemachineCamera camera = virtualCameras[index];
            if (camera.Priority == disabledCameraPriority)
            {
                EnableCamera(index);
            }
            else
            {
                DisableCamera(index);
            }
        }
    }

    /// <summary>
    /// Toggle a camera's enabled state.
    /// </summary>
    /// <param name="camera">The camera to toggle</param>
    public void ToggleCamera(CinemachineCamera camera)
    {
        if (camera != null && virtualCameras.Contains(camera))
        {
            if (camera.Priority == disabledCameraPriority)
            {
                EnableCamera(camera);
            }
            else
            {
                DisableCamera(camera);
            }
        }
    }
}