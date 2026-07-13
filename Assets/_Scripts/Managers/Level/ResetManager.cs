using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ResetManager : MonoBehaviour
{
    public static ResetManager Instance { get; private set; }
    public event Action OnTeleportInput = delegate { };

    [Header("Reset Action")]
    [SerializeField] private InputActionReference resetAction;

    [SerializeField] private float _resetDelay = 0.1f;
    [SerializeField] private List<GameObject> _visualsToToggle = new();
    public bool CanReset = true;

    [Header("Reset Timing Settings")]
    [SerializeField] private float _cooldownDuration = 10f; // If the last reset was within this time, reset immediately.
    [SerializeField] private float _holdDuration = 2f;      // Otherwise, require holding for this duration.

    private float lastResetTime = -Mathf.Infinity;
    private Coroutine holdCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple ResetManagers detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.Enable();
            resetAction.action.started += OnResetStarted;
            resetAction.action.canceled += OnResetCanceled;
        }
    }

    private void OnDisable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.started -= OnResetStarted;
            resetAction.action.canceled -= OnResetCanceled;
            resetAction.action.Disable();
        }
    }

    private void OnResetStarted(InputAction.CallbackContext context)
    {
        if (!CanReset)
            return;

        // If the reset occurred within the cooldown period, perform it immediately.
        if (Time.time - lastResetTime < _cooldownDuration)
        {
            ResetAllStates();
        }
        else
        {
            // Start a coroutine that waits for the hold duration.
            holdCoroutine = StartCoroutine(HoldResetCoroutine());
        }
    }
    public void ForceHoldReset()
    {
        lastResetTime = 0f;
    }
    public void SetCanReset(bool canReset)
    {
        CanReset = canReset;
    }

    private IEnumerator HoldResetCoroutine()
    {
        AudioSystem.Instance.PlaySFXLoop("resetHold", FMODEvents.Instance.LevelResetHoldSFX);

        yield return new WaitForSeconds(0.2f);
        _visualsToToggle[0].SetActive(false);
        yield return new WaitForSeconds(0.32f);
        _visualsToToggle[1].SetActive(false);
        yield return new WaitForSeconds(0.33f);
        _visualsToToggle[2].SetActive(false);
        yield return new WaitForSeconds(0.32f);
        _visualsToToggle[3].SetActive(false);
        yield return new WaitForSeconds(0.1f);
        _visualsToToggle[4].SetActive(false);
        yield return new WaitForSeconds(0.015f);


        AudioSystem.Instance.StopSFXLoop("resetHold");
        ResetAllStates();
        foreach (var visual in _visualsToToggle)
        {
            visual.SetActive(true);
        }
    }

    private void OnResetCanceled(InputAction.CallbackContext context)
    {
        // If the button is released before the hold duration is met, cancel the pending reset.
        if (holdCoroutine != null)
        {
            AudioSystem.Instance.StopSFXLoop("resetHold");

            foreach (var visual in _visualsToToggle)
            {
                visual.SetActive(true);
            }
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    public void ResetAllStates()
    {
        // Update the last reset time.
        lastResetTime = Time.time;

        var currentCheckpoint = CheckpointsManager.Instance.CurrentCheckpoint;
        if (currentCheckpoint != null)
        {
            AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.LevelResetSFX, transform.position);
            CheckpointSaveManager.Instance.LoadCheckpoint(currentCheckpoint.CheckpointKey);
        }
        else
        {
            Debug.LogWarning("No active checkpoint to reset to.");
        }

        if (currentCheckpoint != null)
        {
            currentCheckpoint.OnReset();
        }
        else
        {
            RestartLevel();
        }

        // Freeze the player through BritneyManager
        if (Britney.BritneyManager.Instance != null && Britney.BritneyManager.Instance.BritneyMovement != null)
        {
            Britney.BritneyManager.Instance.BritneyMovement.FreezeRigidBody();
        }

        StartCoroutine(DoAfterFrame());
        StartCoroutine(ResetDelay());
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DoAfterFrame()
    {
        yield return null;
        OnTeleportInput.Invoke();
    }

    private IEnumerator ResetDelay()
    {
        CanReset = false;
        yield return new WaitForSeconds(_resetDelay);
        CanReset = true;
    }
}

public interface ITeleportable
{
    void TeleportTo(Vector3 destination);
}