using UnityEngine;
using System.Collections.Generic;

public class CheckpointSaveManager : MonoBehaviour
{
    public static CheckpointSaveManager Instance { get; private set; }

    // List of all objects that implement IStateful.
    private readonly List<IStateful> _statefulObjects = new List<IStateful>();

    // Dictionary to hold save data for each checkpoint.
    // You can use string IDs, ints, or even references to a Checkpoint object.
    private readonly Dictionary<string, Dictionary<IStateful, object>> _checkpointSaves = new Dictionary<string, Dictionary<IStateful, object>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple CheckpointSaveManagers detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterStateful(IStateful stateful)
    {
        if (!_statefulObjects.Contains(stateful))
        {
            _statefulObjects.Add(stateful);
        }
    }

    public void UnregisterStateful(IStateful stateful)
    {
        if (_statefulObjects.Contains(stateful))
            _statefulObjects.Remove(stateful);
    }

    /// <summary>
    /// Save the state of all registered objects under a checkpoint key.
    /// </summary>
    public void SaveCheckpoint(string checkpointID)
    {
        var saveData = new Dictionary<IStateful, object>();
        foreach (var stateful in _statefulObjects)
        {
            saveData[stateful] = stateful.CaptureState();
        }
        _checkpointSaves[checkpointID] = saveData;
        Debug.Log($"Checkpoint '{checkpointID}' saved.");
    }

    /// <summary>
    /// Restore all registered objects from a previously saved checkpoint.
    /// </summary>
    public void LoadCheckpoint(string checkpointID)
    {
        if (!_checkpointSaves.ContainsKey(checkpointID))
        {
            Debug.LogWarning("No save data for checkpoint " + checkpointID);
            return;
        }

        var saveData = _checkpointSaves[checkpointID];
        foreach (var stateful in _statefulObjects)
        {
            if (saveData.TryGetValue(stateful, out var state))
            {
                stateful.RestoreState(state);
            }
        }
        Debug.Log($"Checkpoint '{checkpointID}' loaded.");
    }
}
