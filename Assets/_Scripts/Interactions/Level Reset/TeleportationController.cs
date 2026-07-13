using UnityEngine;

public class TeleportationController : MonoBehaviour
{
    [SerializeField] private Transform _startTransform;
    // The teleportable target – typically, the player.
    private ITeleportable _teleportable;

    // The teleport input source.
    private ResetManager _resetInput;

    private void Awake()
    {
        _teleportable = GetComponent<ITeleportable>();
        if (_teleportable == null)
        {
            Debug.LogError("No ITeleportable component found on TeleportationController GameObject.");
        }
        _resetInput = ResetManager.Instance;
        if (_resetInput == null)
        {
            Debug.LogError("No ResetManager component found.");
        }
    }

    private void OnEnable()
    {
        if (_resetInput != null)
            _resetInput.OnTeleportInput += HandleTeleportInput;
    }

    private void OnDisable()
    {
        if (_resetInput != null)
            _resetInput.OnTeleportInput -= HandleTeleportInput;
    }

    /// <summary>
    /// Called when teleport input is received.
    /// </summary>
    private void HandleTeleportInput()
    {
        // Get the active checkpoint from the CheckpointsManager.
        var checkpoint = CheckpointsManager.Instance.CurrentCheckpoint;
        if (checkpoint == null)
        {
            Debug.Log("No active checkpoint available for teleportation.");
            return;
        }

        // We need the checkpoint's position. Since your Checkpoint is a MonoBehaviour,
        // we can safely cast it. (Alternatively, you could extend ICheckpoint to expose a position.)
        if (checkpoint is MonoBehaviour checkpointComponent)
        {
            Vector3 targetPosition = checkpointComponent.transform.position;
            _teleportable.TeleportTo(targetPosition);
        }
        else
        {
            Debug.LogError("The active checkpoint does not have a transform.");

        }
    }
}
