using UnityEngine;

public class PositionReset : StatefulMonoBehaviour
{
    // Store the initial position and rotation.
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    /// <summary>
    /// Called from the base Awake method after registration.
    /// Records the initial state of the transform.
    /// </summary>
    protected override void OnStatefulAwake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Optionally, this method can be called to manually reset the transform to its initial state.
    /// </summary>
    public void ResetToInitialState()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    /// <summary>
    /// Captures the current transform state (position and rotation).
    /// This is used by the CheckpointSaveManager when saving state.
    /// </summary>
    public override object CaptureState()
    {
        return new TransformState
        {
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    /// <summary>
    /// Restores the transform state from the given saved state.
    /// This is used by the CheckpointSaveManager when loading state.
    /// </summary>
    public override void RestoreState(object state)
    {
        if (state is TransformState transformState)
        {
            transform.position = transformState.Position;
            transform.rotation = transformState.Rotation;
        }
        else
        {
            Debug.LogError("Invalid state object passed to PositionReset");
        }
    }

    /// <summary>
    /// A serializable struct used to hold the transform state.
    /// </summary>
    [System.Serializable]
    private struct TransformState
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}
