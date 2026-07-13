using UnityEngine;
using System.Collections.Generic;


public class CheckpointsManager : StatefulMonoBehaviour
{
    // This list tracks the order in which checkpoints were activated.
    // The last element is considered the current active checkpoint.
    private List<ICheckpoint> _activationHistory = new List<ICheckpoint>();

    // Returns the current active checkpoint (if any)
    public ICheckpoint CurrentCheckpoint => _activationHistory.Count > 0 ? _activationHistory[_activationHistory.Count - 1] : null;

    public static CheckpointsManager Instance { get; private set; }

    private List<ICheckpoint> _allCheckpoints = new List<ICheckpoint>();

    protected override void OnStatefulAwake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one CheckpointsManager instance in the scene.");
        }
        Instance = this;

        CheckpointSaveManager.Instance.RegisterStateful(this);
    }


    public void RegisterCheckpoint(ICheckpoint checkpoint)
    {
        if (!_allCheckpoints.Contains(checkpoint))
        {
            _allCheckpoints.Add(checkpoint);
        }
    }

    public void ActivateCheckpoint(ICheckpoint checkpoint)
    {
        // If this checkpoint was activated earlier, remove it so we can update its order.
        if (_activationHistory.Contains(checkpoint))
        {
            // If it's already the current one, no need to update anything.
            if (CurrentCheckpoint == checkpoint)
                return;
            _activationHistory.Remove(checkpoint);
        }
        else
        {
            // Optionally: you can check here if the checkpoint is allowed to be activated
            // (for example, if it is “filled”) before proceeding.
        }

        // If there is a checkpoint currently active, suspend it.
        if (CurrentCheckpoint != null)
        {
            CurrentCheckpoint.Deactivate();
        }

        // Add the new checkpoint as the current active one.
        _activationHistory.Add(checkpoint);
        checkpoint.Activate();

        // Save checkpoint (if you have a save system) 
        CheckpointSaveManager.Instance.SaveCheckpoint(checkpoint.CheckpointKey);
    }

    public void DeactivateCheckpoint(ICheckpoint checkpoint)
    {
        // If the checkpoint isn’t tracked, nothing to do.
        if (!_activationHistory.Contains(checkpoint))
            return;

        // Determine if the checkpoint being deactivated is the current active one.
        bool wasCurrent = (CurrentCheckpoint == checkpoint);

        // Remove it from our activation history.
        _activationHistory.Remove(checkpoint);
        checkpoint.Deactivate();
        ResetManager.Instance.ForceHoldReset();

        // If the checkpoint was the active one and a previous checkpoint exists,
        // re-activate the most recently active checkpoint.
        if (wasCurrent && _activationHistory.Count > 0)
        {
            ICheckpoint previous = _activationHistory[_activationHistory.Count - 1];
            previous.Activate();
        }
    }

    #region IStateful

    // Serializable struct to hold CheckpointsManager state.
    [System.Serializable]
    private struct CheckpointsManagerSaveData
    {
        // List of checkpoint keys representing the activation order.
        public List<string> activationHistoryKeys;
    }

    /// <summary>
    /// Capture the current state of the CheckpointsManager.
    /// </summary>
    public override object CaptureState()
    {
        CheckpointsManagerSaveData data = new CheckpointsManagerSaveData
        {
            activationHistoryKeys = new List<string>()
        };

        // Save the activation history using checkpoint keys.
        foreach (ICheckpoint cp in _activationHistory)
        {
            data.activationHistoryKeys.Add(cp.CheckpointKey);
        }

        return data;
    }

    /// <summary>
    /// Restore the CheckpointsManager's state from the provided data.
    /// </summary>
    public override void RestoreState(object state)
    {
        if (state is CheckpointsManagerSaveData data)
        {
            // Clear the current activation history.
            _activationHistory.Clear();

            // Rebuild the activation history using the stored checkpoint keys.
            foreach (string cpKey in data.activationHistoryKeys)
            {
                // Look for the checkpoint with the matching key.
                ICheckpoint cp = _allCheckpoints.Find(x => x.CheckpointKey == cpKey);
                if (cp != null)
                {
                    _activationHistory.Add(cp);
                }
                else
                {
                    Debug.LogWarning($"Checkpoint with key {cpKey} not found during state restore.");
                }
            }

            // Update the visual state:
            // Deactivate all checkpoints first.
            foreach (ICheckpoint checkpoint in _allCheckpoints)
            {
                checkpoint.Deactivate();
            }

            // Activate the current checkpoint (if any).
            if (CurrentCheckpoint != null)
            {
                CurrentCheckpoint.Activate();
            }
        }
        else
        {
            Debug.LogError("Failed to restore CheckpointsManager state: invalid state object.");
        }
    }

    #endregion
}


public interface ICheckpoint
{
    string CheckpointKey { get; }
    bool IsActive { get; }
    void Activate();
    void Deactivate();
    void OnReset();
}
