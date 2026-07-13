using UnityEngine;

public class CameraFreezeController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    private bool _isFrozen = false;
    private Vector3 _frozenPosition;
    private Quaternion _frozenRotation;
    
    // Call this to freeze the camera
    public void FreezeCamera()
    {
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera not assigned!");
            return;
        }
        
        // Cache the current position and rotation
        _frozenPosition = _mainCamera.transform.position;
        _frozenRotation = _mainCamera.transform.rotation;
        _isFrozen = true;
    }
    
    // Call this to unfreeze the camera
    public void UnfreezeCamera()
    {
        _isFrozen = false;
    }
    
    // Make sure this runs after Cinemachine updates (typically in LateUpdate)
    private void LateUpdate()
    {
        if (_isFrozen)
        {
            _mainCamera.transform.position = _frozenPosition;
            _mainCamera.transform.rotation = _frozenRotation;
        }
    }
}
