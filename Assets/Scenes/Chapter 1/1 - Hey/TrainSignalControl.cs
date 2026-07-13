using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Added for UnityEvents

public class TrainSignalControl : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField]
    private Vector2 triggerBoxSize = new Vector2(5f, 3f);

    [SerializeField]
    private Transform playerTransform; // Player must be assigned in the Inspector

    [Header("Shader Light Control (_WH_Lights)")]
    [SerializeField]
    private Renderer signalRenderer;
    private const string LIGHTS_PROPERTY_NAME = "_WH_Lights";

    [SerializeField]
    private Vector2 lightsValuePlayerOutside = Vector2.zero;

    [SerializeField]
    private Vector2 lightsValuePlayerInside = Vector2.one;

    [SerializeField]
    private float lightTransitionDuration = 1.0f;

    [Header("Shader State Keywords (_STATE)")]
    private const string STATE_KEYWORD_BASE = "_STATE";

    public enum SignalState
    {
        NONE,
        RED,
        YELLOW,
        GREEN,
    }

    [Header("State Change Transparency Animation (_LightTransparency)")]
    [SerializeField]
    private float transparencyAnimationDuration = 0.25f; // Duration for fade in/out
    private const string TRANSPARENCY_PROPERTY_NAME = "_LightTransparency";

    [Header("State Change Events")]
    public UnityEvent onStateSetToNone;
    public UnityEvent onStateSetToRed;
    public UnityEvent onStateSetToYellow;
    public UnityEvent onStateSetToGreen;

    private Material _signalMaterialInstance;
    private bool _isPlayerCurrentlyInside = false;
    private Coroutine _playerLightTransitionCoroutine; // Renamed for clarity
    private Coroutine _stateChangeAnimationCoroutine; // Coroutine for state change with transparency

    // _currentVisualShaderState tracks the keywords active on the shader
    private SignalState _currentVisualShaderState = SignalState.NONE;

    // _targetVisualState tracks the state we are animating towards or have achieved via RequestVisualStateChange
    private SignalState _targetVisualState = SignalState.NONE;

    void Awake()
    {
        if (signalRenderer != null)
        {
            _signalMaterialInstance = signalRenderer.material;
        }
        else
        {
            Debug.LogError(
                "Signal Renderer not assigned on " + gameObject.name + ". Script will be disabled."
            );
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError(
                "Player Transform not assigned on "
                    + gameObject.name
                    + ". Player detection will not work. Script will be disabled."
            );
            enabled = false;
            return;
        }
    }

    void Start()
    {
        // Initialize shader properties
        if (_signalMaterialInstance != null)
        {
            // Set initial keywords without invoking events, as it's the starting state
            SetShaderKeywordsOnly(SignalState.NONE, false);
            _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, 1f); // Start fully visible
            _targetVisualState = SignalState.NONE; // Initial target state
        }
    }

    void Update()
    {
        if (_signalMaterialInstance == null || playerTransform == null)
            return;

        Vector3 selfPos = transform.position;
        Vector3 playerPos = playerTransform.position;

        bool isInsideHorizontally = Mathf.Abs(playerPos.x - selfPos.x) < triggerBoxSize.x / 2f;
        bool isInsideVertically = Mathf.Abs(playerPos.y - selfPos.y) < triggerBoxSize.y / 2f;

        if (isInsideHorizontally && isInsideVertically)
        {
            if (!_isPlayerCurrentlyInside)
            {
                HandlePlayerEnter();
            }
        }
        else
        {
            if (_isPlayerCurrentlyInside)
            {
                HandlePlayerExit();
            }
        }
    }

    private void HandlePlayerEnter()
    {
        _isPlayerCurrentlyInside = true;
        if (_playerLightTransitionCoroutine != null)
        {
            StopCoroutine(_playerLightTransitionCoroutine);
        }
        _playerLightTransitionCoroutine = StartCoroutine(
            AnimatePlayerLights(lightsValuePlayerInside)
        );
    }

    private void HandlePlayerExit()
    {
        _isPlayerCurrentlyInside = false;
        if (_playerLightTransitionCoroutine != null)
        {
            StopCoroutine(_playerLightTransitionCoroutine);
        }
        _playerLightTransitionCoroutine = StartCoroutine(
            AnimatePlayerLights(lightsValuePlayerOutside)
        );
    }

    private IEnumerator AnimatePlayerLights(Vector2 targetValue)
    {
        if (_signalMaterialInstance == null)
            yield break;

        Vector2 startValue = _signalMaterialInstance.GetVector(LIGHTS_PROPERTY_NAME);
        float elapsedTime = 0f;

        while (elapsedTime < lightTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / lightTransitionDuration);
            Vector2 currentValue = Vector2.Lerp(startValue, targetValue, progress);
            _signalMaterialInstance.SetVector(LIGHTS_PROPERTY_NAME, currentValue);
            yield return null;
        }
        _signalMaterialInstance.SetVector(LIGHTS_PROPERTY_NAME, targetValue);
    }

    // Main method to request a visual state change
    private void RequestVisualStateChange(SignalState newState)
    {
        // If already settled in the target state and no animation is running, do nothing.
        if (_targetVisualState == newState && _stateChangeAnimationCoroutine == null)
        {
            //Debug.Log($"Already in state {newState} and animation finished. No change.");
            return;
        }

        // If we are already animating towards this state, or if the keywords are already set to this state
        // and we are just re-triggering (e.g. to ensure visibility), we might still want to run the animation.
        // The crucial check is if the _targetVisualState is already what we want.
        // If the new state is the same as the one we are already targeting or have set,
        // and an animation is NOT running, we don't need to do anything.
        // If an animation IS running, stopping and restarting it for the same target state is fine.

        if (_stateChangeAnimationCoroutine != null)
        {
            StopCoroutine(_stateChangeAnimationCoroutine);
        }

        _targetVisualState = newState;
        _stateChangeAnimationCoroutine = StartCoroutine(
            AnimateStateChangeWithTransparencySequence(newState)
        );
    }

    private IEnumerator AnimateStateChangeWithTransparencySequence(
        SignalState stateToSetKeywordsFor
    )
    {
        if (_signalMaterialInstance == null)
            yield break;

        bool actuallyChangedKeywords = (_currentVisualShaderState != stateToSetKeywordsFor);

        if (transparencyAnimationDuration <= 0.001f)
        {
            _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, 0f);
            SetShaderKeywordsOnly(stateToSetKeywordsFor, actuallyChangedKeywords); // Pass if keywords changed
            _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, 1f);
            _stateChangeAnimationCoroutine = null;
            yield break;
        }

        float currentTransparency = _signalMaterialInstance.GetFloat(TRANSPARENCY_PROPERTY_NAME);
        float elapsedTime = 0f;
        float actualFadeOutDuration = transparencyAnimationDuration * currentTransparency;

        if (currentTransparency > 0.001f)
        {
            while (elapsedTime < actualFadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float newTransparency = Mathf.Lerp(
                    currentTransparency,
                    0f,
                    elapsedTime / actualFadeOutDuration
                );
                _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, newTransparency);
                yield return null;
            }
        }
        _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, 0f);

        SetShaderKeywordsOnly(stateToSetKeywordsFor, actuallyChangedKeywords); // Pass if keywords changed

        elapsedTime = 0f;
        while (elapsedTime < transparencyAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float newTransparency = Mathf.Lerp(0f, 1f, elapsedTime / transparencyAnimationDuration);
            _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, newTransparency);
            yield return null;
        }
        _signalMaterialInstance.SetFloat(TRANSPARENCY_PROPERTY_NAME, 1f);

        _stateChangeAnimationCoroutine = null;
    }

    // Helper to set shader keywords and update the internal tracking state
    private void SetShaderKeywordsOnly(SignalState newState, bool invokeEvents = true)
    {
        if (_signalMaterialInstance == null)
            return;

        bool stateActuallyChanged = _currentVisualShaderState != newState;

        _signalMaterialInstance.DisableKeyword(
            STATE_KEYWORD_BASE + "_" + SignalState.RED.ToString()
        );
        _signalMaterialInstance.DisableKeyword(
            STATE_KEYWORD_BASE + "_" + SignalState.YELLOW.ToString()
        );
        _signalMaterialInstance.DisableKeyword(
            STATE_KEYWORD_BASE + "_" + SignalState.GREEN.ToString()
        );

        if (newState != SignalState.NONE)
        {
            string keywordToEnable = STATE_KEYWORD_BASE + "_" + newState.ToString();
            _signalMaterialInstance.EnableKeyword(keywordToEnable);
        }
        _currentVisualShaderState = newState;

        if (invokeEvents && stateActuallyChanged) // Only invoke if the state truly changed
        {
            switch (newState)
            {
                case SignalState.NONE:
                    onStateSetToNone?.Invoke();
                    break;
                case SignalState.RED:
                    onStateSetToRed?.Invoke();
                    break;
                case SignalState.YELLOW:
                    onStateSetToYellow?.Invoke();
                    break;
                case SignalState.GREEN:
                    onStateSetToGreen?.Invoke();
                    break;
            }
        }
    }

    // --- Public Methods to Control Signal State ---
    public void SetNoLight()
    {
        RequestVisualStateChange(SignalState.NONE);
    }

    public void SetRedLight()
    {
        RequestVisualStateChange(SignalState.RED);
    }

    public void SetYellowLight()
    {
        RequestVisualStateChange(SignalState.YELLOW);
    }

    public void SetGreenLight()
    {
        RequestVisualStateChange(SignalState.GREEN);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 boxCenter = transform.position;
        Vector3 boxGizmoSize = new Vector3(triggerBoxSize.x, triggerBoxSize.y, 0.1f);
        Gizmos.DrawWireCube(boxCenter, boxGizmoSize);
    }

    void OnDestroy()
    {
        if (_signalMaterialInstance != null)
        {
            Destroy(_signalMaterialInstance);
        }
    }
}
