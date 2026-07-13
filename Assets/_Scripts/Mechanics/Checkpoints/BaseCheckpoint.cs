using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class BaseCheckpoint : StatefulMonoBehaviour, ICheckpoint
{
    private bool _isActive;

    public bool IsActive => _isActive;

    public string CheckpointKey => gameObject.name;

    public UnityEvent OnResetEvent;

    private void Start()
    {
        CheckpointsManager.Instance.RegisterCheckpoint(this);
        CheckpointsManager.Instance.ActivateCheckpoint(this);
    }



    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
    }

    public void OnReset()
    {
        OnResetEvent?.Invoke();
    }


    #region Save

    // A serializable struct to hold the checkpoint's state.
    [System.Serializable]
    private struct CheckpointSaveData
    {
        public bool isActive;
        public bool isFilled;
    }

    /// <summary>
    /// Capture the current state of the Checkpoint.
    /// </summary>
    public override object CaptureState()
    {
        CheckpointSaveData data = new CheckpointSaveData
        {
            isActive = _isActive,
        };

        return data;
    }

    /// <summary>
    /// Restore the Checkpoint's state from the provided data.
    /// </summary>
    public override void RestoreState(object state)
    {
        if (state is CheckpointSaveData data)
        {
            _isActive = data.isActive;        }
        else
        {
            Debug.LogError("Invalid state provided for Base_Checkpoint restore.");
        }
    }

    #endregion
}
