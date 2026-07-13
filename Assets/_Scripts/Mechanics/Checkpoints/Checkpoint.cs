using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : StatefulMonoBehaviour, ICheckpoint
{
    [SerializeField, BoxGroup("SETTINGS")] private bool _canBeFilled = true;
    [SerializeField, BoxGroup("SETTINGS")] private bool _canBeUnfilled = true;
    [SerializeField, BoxGroup("SETTINGS")] private int _useInkAmount = 1;
    [SerializeField, BoxGroup("SETTINGS")] private int _restoreInkAmount = 1;
    [SerializeField] private MaterialActions _materialChanges;


    // Whether this checkpoint is active or not
    private bool _isActive;
    private bool _isFilled;

    public bool IsActive => _isActive;

    public string CheckpointKey => gameObject.name;

    public UnityEvent OnResetEvent;

    private void Start()
    {
        CheckpointsManager.Instance.RegisterCheckpoint(this);
        _isFilled = false;

        UpdateFilledVisual();
        UpdateActiveVisual();
    }



    public void Activate()
    {
        _isActive = true;
        UpdateActiveVisual();
    }

    public void Deactivate()
    {
        _isActive = false;
        UpdateActiveVisual();
    }

    public void OnReset()
    {
        OnResetEvent?.Invoke();
    }

    public void OnTouched(InkManager inkManager)
    {
        if (_isFilled)
        {
            if (_canBeUnfilled && inkManager.CanUnfill && inkManager.CanRestoreInk(_restoreInkAmount))
            {
                inkManager.RestoreInk(_restoreInkAmount);
                _isFilled = false;
                UpdateFilledVisual();

                CheckpointsManager.Instance.DeactivateCheckpoint(this);
                AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.UnfillCheckpointSFX, transform.position);
            }
        }
        else
        {
            if (_canBeFilled && inkManager.CanFill && inkManager.CanUseInk(_useInkAmount))
            {
                inkManager.UseInk(_useInkAmount);
                _isFilled = true;
                UpdateFilledVisual();

                CheckpointsManager.Instance.ActivateCheckpoint(this);
                AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.FillCheckpointSFX, transform.position);
            }
        }
    }

    private void UpdateActiveVisual()
    {
        if (_isActive)
        {
            _materialChanges.ChangeFloatsToEnd();
            _materialChanges.ChangeVector2sToEnd();
        }
        else
        {
            _materialChanges.ChangeFloatsToStart();
            _materialChanges.ChangeVector2sToStart();
        }
    }

    private void UpdateFilledVisual()
    {
        if (_isFilled)
        {
            _materialChanges.ChangeColorsToEnd();
        }
        else
        {
            _materialChanges.ChangeColorsToStart();
        }
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
            isFilled = _isFilled
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
            _isActive = data.isActive;
            _isFilled = data.isFilled;

            // Update the visuals to reflect the restored state.
            UpdateFilledVisual();
            UpdateActiveVisual();

            // If needed, update the color or other visual indicators immediately.
            // For example, you might want to stop running coroutines before updating the material.
        }
        else
        {
            Debug.LogError("Invalid state provided for Checkpoint restore.");
        }
    }

    #endregion
}
