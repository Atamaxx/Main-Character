using UnityEngine;

public abstract class StatefulMonoBehaviour : MonoBehaviour, IStateful
{
    protected virtual void Awake()
    {
        // Automatically register this stateful object with the save manager.
        CheckpointSaveManager.Instance.RegisterStateful(this);
        OnStatefulAwake();
    }

    /// <summary>
    /// Optional override point for inheriting classes to execute logic in Awake after registration.
    /// </summary>
    protected virtual void OnStatefulAwake() { }

    protected virtual void OnDestroy()
    {
        if (CheckpointSaveManager.Instance != null)
            CheckpointSaveManager.Instance.UnregisterStateful(this);
    }

    // IStateful interface methods must be implemented by the derived classes.
    public abstract object CaptureState();
    public abstract void RestoreState(object state);
}

/// <summary>
/// A generic interface that allows a component to capture and restore its state.
/// </summary>
public interface IStateful
{
    /// <summary>
    /// Capture the current state of the component.
    /// </summary>
    object CaptureState();

    /// <summary>
    /// Restore the component's state from the provided data.
    /// </summary>
    void RestoreState(object state);
}
