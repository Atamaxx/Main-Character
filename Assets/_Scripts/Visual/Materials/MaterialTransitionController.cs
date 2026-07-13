using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Controls material property transitions with support for animation curves, progress tracking, and state persistence.
/// </summary>
public class MaterialTransitionController : MonoBehaviour
{
    #region Property Transition Interfaces

    /// <summary>
    /// Base interface for all material property transitions
    /// </summary>
    public interface IPropertyTransition
    {
        string PropertyName { get; }
        int PropertyID { get; }
        float Duration { get; }
        float RevertSpeedMultiplier { get; }
        AnimationCurve EasingCurve { get; }
        float Progress { get; set; }

        void Initialize();
        void UpdateTransition(Material material, float progress);
        void SetToStartValue(Material material);
        void SetToEndValue(Material material);
    }

    /// <summary>
    /// Base implementation for property transitions
    /// </summary>
    [Serializable]
    public abstract class BasePropertyTransition<T> : IPropertyTransition
    {
        public string PropertyName => propertyName;
        public int PropertyID { get; private set; }
        public float Duration => duration;
        public float RevertSpeedMultiplier => revertSpeedMultiplier;
        public AnimationCurve EasingCurve => easingCurve;
        public float Progress { get => progress; set => progress = value; }

        [SerializeField] protected string propertyName;
        [SerializeField] protected T startValue;
        [SerializeField] protected T endValue;
        [SerializeField] protected float duration = 1f;
        [SerializeField] protected float revertSpeedMultiplier = 1f;
        [SerializeField] protected AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Inspector-visible progress value (0-1)
        [SerializeField, Range(0f, 1f)] protected float progress = 0f;

        public virtual void Initialize()
        {
            PropertyID = Shader.PropertyToID(propertyName);
        }

        public abstract void UpdateTransition(Material material, float progress);
        public abstract void SetToStartValue(Material material);
        public abstract void SetToEndValue(Material material);
    }

    /// <summary>
    /// Implementation for Color property transitions
    /// </summary>
    [Serializable]
    public class ColorPropertyTransition : BasePropertyTransition<Color>
    {
        public override void UpdateTransition(Material material, float progress)
        {
            if (material == null) return;
            float curveTime = EasingCurve.Evaluate(progress);
            Color targetValue = Color.Lerp(startValue, endValue, curveTime);
            material.SetColor(PropertyID, targetValue);

            // Update progress
            this.progress = progress;
        }

        public override void SetToStartValue(Material material)
        {
            if (material == null) return;
            material.SetColor(PropertyID, startValue);
            progress = 0f;
        }

        public override void SetToEndValue(Material material)
        {
            if (material == null) return;
            material.SetColor(PropertyID, endValue);
            progress = 1f;
        }
    }

    /// <summary>
    /// Implementation for Float property transitions
    /// </summary>
    [Serializable]
    public class FloatPropertyTransition : BasePropertyTransition<float>
    {
        public override void UpdateTransition(Material material, float progress)
        {
            if (material == null) return;
            float curveTime = EasingCurve.Evaluate(progress);
            float targetValue = Mathf.Lerp(startValue, endValue, curveTime);
            material.SetFloat(PropertyID, targetValue);

            // Update progress
            this.progress = progress;
        }

        public override void SetToStartValue(Material material)
        {
            if (material == null) return;
            material.SetFloat(PropertyID, startValue);
            progress = 0f;
        }

        public override void SetToEndValue(Material material)
        {
            if (material == null) return;
            material.SetFloat(PropertyID, endValue);
            progress = 1f;
        }
    }

    /// <summary>
    /// Implementation for Vector2 property transitions
    /// </summary>
    [Serializable]
    public class Vector2PropertyTransition : BasePropertyTransition<Vector2>
    {
        public override void UpdateTransition(Material material, float progress)
        {
            if (material == null) return;
            float curveTime = EasingCurve.Evaluate(progress);
            Vector2 targetValue = Vector2.Lerp(startValue, endValue, curveTime);

            // Some shaders expect Vector2 as Vector4, others as Vector2
            if (material.HasVector(PropertyID))
            {
                material.SetVector(PropertyID, new Vector4(targetValue.x, targetValue.y, 0f, 0f));
            }
            else
            {
                material.SetTextureOffset(PropertyID, targetValue); // For texture offsets
            }

            // Update progress
            this.progress = progress;
        }

        public override void SetToStartValue(Material material)
        {
            if (material == null) return;

            if (material.HasVector(PropertyID))
            {
                material.SetVector(PropertyID, new Vector4(startValue.x, startValue.y, 0f, 0f));
            }
            else
            {
                material.SetTextureOffset(PropertyID, startValue);
            }

            progress = 0f;
        }

        public override void SetToEndValue(Material material)
        {
            if (material == null) return;

            if (material.HasVector(PropertyID))
            {
                material.SetVector(PropertyID, new Vector4(endValue.x, endValue.y, 0f, 0f));
            }
            else
            {
                material.SetTextureOffset(PropertyID, endValue);
            }

            progress = 1f;
        }
    }

    #endregion

    #region Progress Tracking and State Persistence

    /// <summary>
    /// Structure to hold serializable transition state
    /// </summary>
    [Serializable]
    public struct TransitionState
    {
        public float overallProgress;
        public Dictionary<string, float> propertyProgress;

        public TransitionState(float overall)
        {
            overallProgress = overall;
            propertyProgress = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// Normalized overall progress of the current transition (0-1)
    /// 0 = all properties at start values, 1 = all properties at end values
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    private float _overallProgress = 0f;

    public float OverallTransitionProgress
    {
        get => _overallProgress;
        set => _overallProgress = Mathf.Clamp01(value);
    }

    /// <summary>
    /// Progress tracking for color transitions
    /// </summary>
    public float ColorTransitionProgress
    {
        get
        {
            if (_colorTransitions.Count == 0) return 0f;
            float sum = 0f;
            foreach (var transition in _colorTransitions)
            {
                sum += transition.Progress;
            }
            return sum / _colorTransitions.Count;
        }
    }

    /// <summary>
    /// Progress tracking for float transitions
    /// </summary>
    public float FloatTransitionProgress
    {
        get
        {
            if (_floatTransitions.Count == 0) return 0f;
            float sum = 0f;
            foreach (var transition in _floatTransitions)
            {
                sum += transition.Progress;
            }
            return sum / _floatTransitions.Count;
        }
    }

    /// <summary>
    /// Progress tracking for vector2 transitions
    /// </summary>
    public float Vector2TransitionProgress
    {
        get
        {
            if (_vector2Transitions.Count == 0) return 0f;
            float sum = 0f;
            foreach (var transition in _vector2Transitions)
            {
                sum += transition.Progress;
            }
            return sum / _vector2Transitions.Count;
        }
    }

    /// <summary>
    /// Gets the current transition state that can be saved or transferred
    /// </summary>
    public TransitionState GetCurrentState()
    {
        var state = new TransitionState(OverallTransitionProgress);

        // Store progress for each property
        foreach (var transition in _allTransitions)
        {
            state.propertyProgress[transition.PropertyName] = transition.Progress;
        }

        return state;
    }

    /// <summary>
    /// Restores a previously saved transition state
    /// </summary>
    public void RestoreState(TransitionState state)
    {
        if (!_initialized)
            Initialize();

        OverallTransitionProgress = state.overallProgress;

        // Restore progress for each property
        foreach (var transition in _allTransitions)
        {
            if (state.propertyProgress.TryGetValue(transition.PropertyName, out float progress))
            {
                transition.Progress = progress;

                // Update material with the restored progress
                UpdateTransitionFromProgress(transition, progress);
            }
        }
    }

    /// <summary>
    /// Updates a transition based on the stored progress value
    /// </summary>
    private void UpdateTransitionFromProgress(IPropertyTransition transition, float progress)
    {
        if (MaterialInstance == null) return;

        // Always use UpdateTransition for exact value control, including at 0 and 1
        // This prevents "jumps" when starting from specific progress values
        transition.UpdateTransition(MaterialInstance, Mathf.Clamp01(progress));
    }

    /// <summary>
    /// Set all material properties based on the current progress values
    /// </summary>
    public void ApplyProgressValues()
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        foreach (var transition in _allTransitions)
        {
            UpdateTransitionFromProgress(transition, transition.Progress);
        }
    }

    #endregion

    #region Transition Manager

    /// <summary>
    /// Manages the state and execution of a set of property transitions
    /// </summary>
    private class TransitionManager
    {
        private readonly Material _material;
        private readonly IPropertyTransition[] _transitions;
        private readonly bool _toEndValue;
        private readonly float _speedMultiplier;
        private readonly Action<float> _onProgressUpdate;
        private readonly Action _onComplete;

        private float _startProgress;
        private float _targetProgress;
        private float _elapsedTime;
        private float _duration;
        private bool _isRunning;

        /// <summary>
        /// Current normalized progress of the transition (0-1)
        /// </summary>
        public float CurrentProgress { get; private set; }

        public TransitionManager(
            Material material,
            IPropertyTransition[] transitions,
            bool toEndValue,
            float speedMultiplier,
            Action<float> onProgressUpdate,
            Action onComplete,
            float startProgress)
        {
            _material = material;
            _transitions = transitions;
            _toEndValue = toEndValue;
            _speedMultiplier = speedMultiplier;
            _onProgressUpdate = onProgressUpdate;
            _onComplete = onComplete;

            // Determine start and target progress values
            _startProgress = startProgress;
            _targetProgress = _toEndValue ? 1f : 0f;

            // Calculate the duration based on start and target progress
            _duration = CalculateDuration();
            _elapsedTime = 0f;
            CurrentProgress = _startProgress;
            _isRunning = true;

            // Report initial progress
            _onProgressUpdate?.Invoke(CurrentProgress);
        }

        /// <summary>
        /// Calculate the appropriate duration based on progress and direction
        /// </summary>
        private float CalculateDuration()
        {
            // Find the longest base duration and appropriate speed multiplier
            float longestDuration = 0f;
            float speedMultiplier = 1f;

            foreach (var transition in _transitions)
            {
                longestDuration = Mathf.Max(longestDuration, transition.Duration);

                // If we're reverting, use the revert speed multiplier
                if (!_toEndValue)
                {
                    speedMultiplier = Mathf.Max(speedMultiplier, transition.RevertSpeedMultiplier);
                }
            }

            // Adjust duration based on progress distance
            float progressDistance = Mathf.Abs(_targetProgress - _startProgress);
            float adjustedDuration = longestDuration * progressDistance;

            // Apply speed multiplier if going back to start
            if (!_toEndValue)
            {
                adjustedDuration /= speedMultiplier;
            }

            // Ensure we always have a non-zero duration to prevent instant completion
            // unless start and target are identical
            return Mathf.Approximately(_startProgress, _targetProgress) ? 0f : Mathf.Max(adjustedDuration, 0.001f);
        }

        public bool Update(float deltaTime, bool isPaused)
        {
            if (!_isRunning || isPaused)
                return true; // Return true to keep the coroutine running even when paused

            if (_material == null)
            {
                _isRunning = false;
                _onComplete?.Invoke();
                return false;
            }

            if (Mathf.Approximately(_startProgress, _targetProgress))
            {
                // If start and target are the same, complete immediately
                ApplyTargetValues();
                _isRunning = false;
                _onComplete?.Invoke();
                return false;
            }

            _elapsedTime += deltaTime * _speedMultiplier;

            // Calculate the normalized progress (0 to 1) for the transition
            float transitionProgress = _duration > 0 ?
                Mathf.Clamp01(_elapsedTime / _duration) : 1f;

            // Convert to the actual progress value between start and target
            CurrentProgress = Mathf.Lerp(_startProgress, _targetProgress, transitionProgress);

            // Update all transitions
            foreach (var transition in _transitions)
            {
                transition.UpdateTransition(_material, CurrentProgress);
            }

            // Report progress
            _onProgressUpdate?.Invoke(CurrentProgress);

            if (transitionProgress >= 1f)
            {
                // Transition complete - set all properties to final values
                ApplyTargetValues();
                _isRunning = false;
                _onComplete?.Invoke();
                return false;
            }

            return true;
        }

        private void ApplyTargetValues()
        {
            if (_material == null) return;

            foreach (var transition in _transitions)
            {
                if (_toEndValue)
                    transition.SetToEndValue(_material);
                else
                    transition.SetToStartValue(_material);
            }

            CurrentProgress = _targetProgress;
            _onProgressUpdate?.Invoke(CurrentProgress);
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }

    #endregion

    #region Public Properties and Fields

    [Tooltip("The material to transition. Will be instanced if createInstance is true.")]
    [SerializeField] private Material _targetMaterial;

    [Tooltip("If true, creates a unique instance of the material to avoid affecting other objects.")]
    [SerializeField] private bool _createInstance = true;

    [Tooltip("If true, applies the material to all renderers in the list on Start.")]
    [SerializeField] private bool _changeMaterialOnStart = true;

    [Tooltip("Renderers that should use this material.")]
    [SerializeField, ShowIf("_changeMaterialOnStart")]
    private List<Renderer> _renderers = new();

    [Tooltip("If true, category-specific transitions will update the overall progress. If false, they remain independent.")]
    [SerializeField] private bool _syncCategoryTransitionsToOverall = false;

    [Space(10)]
    [Header("Color Properties")]
    [SerializeField] private List<ColorPropertyTransition> _colorTransitions = new();

    [Header("Float Properties")]
    [SerializeField] private List<FloatPropertyTransition> _floatTransitions = new();

    [Header("Vector2 Properties")]
    [SerializeField] private List<Vector2PropertyTransition> _vector2Transitions = new();

    /// <summary>
    /// The material instance used for transitions
    /// </summary>
    public Material MaterialInstance { get; private set; }

    /// <summary>
    /// Indicates whether any transition is currently active
    /// </summary>
    public bool IsTransitioning => _transitionCoroutine != null;

    /// <summary>
    /// Indicates whether transitions are currently paused
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// Gets a dictionary of all property names and their current progress values
    /// </summary>
    public Dictionary<string, float> PropertyProgress
    {
        get
        {
            Dictionary<string, float> progress = new Dictionary<string, float>();
            foreach (var transition in _allTransitions)
            {
                progress[transition.PropertyName] = transition.Progress;
            }
            return progress;
        }
    }

    #endregion

    #region Private Fields

    private List<IPropertyTransition> _allTransitions = new();
    private TransitionManager _activeTransitionManager;
    private Coroutine _transitionCoroutine;
    private bool _initialized = false;

    // Events for transition completion
    public event Action OnTransitionToStartComplete;
    public event Action OnTransitionToEndComplete;
    public event Action<float> OnTransitionProgressUpdated;

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (_changeMaterialOnStart && MaterialInstance != null)
        {
            ApplyMaterialToRenderers();

            // Apply initial progress values from inspector
            ApplyProgressValues();
        }
    }

    private void OnEnable()
    {
        // Ensure material is still valid when re-enabled
        if (_initialized && MaterialInstance == null)
        {
            RecreateMaterial();
            ApplyMaterialToRenderers();
            ApplyProgressValues();
        }
    }

    private void OnDestroy()
    {
        CleanupMaterial();
    }

    private void OnValidate()
    {
        // In edit mode, update properties to reflect inspector changes
        if (!Application.isPlaying && _targetMaterial != null)
        {
            bool wasInitialized = _initialized;

            // Create temporary instance if needed
            Material tempMaterial = null;
            if (_createInstance)
            {
                tempMaterial = new Material(_targetMaterial);
            }
            else
            {
                tempMaterial = _targetMaterial;
            }

            // Initialize transitions to get property IDs
            foreach (var transition in _colorTransitions)
            {
                transition.Initialize();
                UpdateTransitionFromProgress(transition, transition.Progress);
            }

            foreach (var transition in _floatTransitions)
            {
                transition.Initialize();
                UpdateTransitionFromProgress(transition, transition.Progress);
            }

            foreach (var transition in _vector2Transitions)
            {
                transition.Initialize();
                UpdateTransitionFromProgress(transition, transition.Progress);
            }

            if (_createInstance && tempMaterial != null)
            {
                DestroyImmediate(tempMaterial);
            }

            _initialized = wasInitialized;
        }
    }

    #endregion

    #region Initialization Methods

    /// <summary>
    /// Initializes all transitions and material instance
    /// </summary>
    private void Initialize()
    {
        if (_initialized)
            return;

        // Create material instance
        RecreateMaterial();

        if (MaterialInstance == null)
            return; // Can't initialize without a material

        // Initialize all transitions
        _allTransitions.Clear();

        foreach (var transition in _colorTransitions)
        {
            transition.Initialize();
            _allTransitions.Add(transition);
        }

        foreach (var transition in _floatTransitions)
        {
            transition.Initialize();
            _allTransitions.Add(transition);
        }

        foreach (var transition in _vector2Transitions)
        {
            transition.Initialize();
            _allTransitions.Add(transition);
        }

        _initialized = true;
    }

    /// <summary>
    /// Creates or assigns the material instance
    /// </summary>
    private void RecreateMaterial()
    {
        // Clean up existing material if needed
        CleanupMaterial();

        // Create or assign a new material
        if (_targetMaterial != null)
        {
            if (_createInstance)
            {
                MaterialInstance = new Material(_targetMaterial);
                MaterialInstance.name = $"{_targetMaterial.name}_Instance";
            }
            else
            {
                MaterialInstance = _targetMaterial;
            }
        }
    }

    /// <summary>
    /// Cleans up the material instance if it was created by this component
    /// </summary>
    private void CleanupMaterial()
    {
        if (_createInstance && MaterialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(MaterialInstance);
            }
            else
            {
                DestroyImmediate(MaterialInstance);
            }
            MaterialInstance = null;
        }
    }

    /// <summary>
    /// Applies the material instance to all assigned renderers
    /// </summary>
    private void ApplyMaterialToRenderers()
    {
        if (MaterialInstance == null) return;

        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                // Use sharedMaterial for editor, material for play mode
                if (Application.isPlaying)
                {
                    renderer.material = MaterialInstance;
                }
                else
                {
                    renderer.sharedMaterial = MaterialInstance;
                }
            }
        }
    }

    #endregion

    #region Public Transition Methods

    /// <summary>
    /// Transitions all properties to their end values (progress = 1)
    /// </summary>
    public void TransitionToEnd()
    {
        TransitionToEnd(OverallTransitionProgress);
    }

    /// <summary>
    /// Transitions all properties to their end values starting from a specific progress point
    /// </summary>
    public void TransitionToEnd(float fromProgress)
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        _activeTransitionManager = new TransitionManager(
            MaterialInstance,
            _allTransitions.ToArray(),
            true, // to end values
            1.0f,
            UpdateTransitionProgress,
            () => { OnTransitionToEndComplete?.Invoke(); },
            fromProgress
        );

        _transitionCoroutine = StartCoroutine(RunTransition(_activeTransitionManager));
    }

    /// <summary>
    /// Transitions all properties to their start values (progress = 0)
    /// </summary>
    public void TransitionToStart()
    {
        TransitionToStart(OverallTransitionProgress);
    }

    /// <summary>
    /// Transitions all properties to their start values starting from a specific progress point
    /// </summary>
    public void TransitionToStart(float fromProgress)
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        _activeTransitionManager = new TransitionManager(
            MaterialInstance,
            _allTransitions.ToArray(),
            false, // to start values
            1.0f,
            UpdateTransitionProgress,
            () => { OnTransitionToStartComplete?.Invoke(); },
            fromProgress
        );

        _transitionCoroutine = StartCoroutine(RunTransition(_activeTransitionManager));
    }

    /// <summary>
    /// Transitions to a specific progress value (0-1), starting from the current progress
    /// </summary>
    public void TransitionToProgress(float targetProgress)
    {
        TransitionFromToProgress(OverallTransitionProgress, targetProgress);
    }

    /// <summary>
    /// Transitions from a specific start progress to a specific end progress (0-1)
    /// </summary>
    public void TransitionFromToProgress(float startProgress, float targetProgress)
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        float clampedStart = Mathf.Clamp01(startProgress);
        float clampedTarget = Mathf.Clamp01(targetProgress);

        // Determine if we're moving toward end or start
        bool toEnd = clampedTarget > clampedStart;
        Action completeAction = toEnd ?
            () => { OnTransitionToEndComplete?.Invoke(); }
        :
            () => { OnTransitionToStartComplete?.Invoke(); };

        // Create a custom TransitionManager with specific start and target values
        _activeTransitionManager = new TransitionManager(
            MaterialInstance,
            _allTransitions.ToArray(),
            toEnd,
            1.0f,
            UpdateTransitionProgress,
            completeAction,
            clampedStart // Use the provided start progress instead of current progress
        );

        _transitionCoroutine = StartCoroutine(RunTransition(_activeTransitionManager));
    }

    /// <summary>
    /// Pauses any active transition
    /// </summary>
    public void PauseTransition()
    {
        IsPaused = true;
    }

    /// <summary>
    /// Resumes a paused transition
    /// </summary>
    public void ResumeTransition()
    {
        IsPaused = false;
    }

    /// <summary>
    /// Updates the overall transition progress tracking
    /// </summary>
    private void UpdateTransitionProgress(float progress)
    {
        OverallTransitionProgress = progress;
        OnTransitionProgressUpdated?.Invoke(progress);
    }

    /// <summary>
    /// Transitions a specific property by name
    /// </summary>
    public void TransitionProperty(string propertyName, float targetProgress, float duration = -1f)
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        // Find the property transition
        IPropertyTransition targetTransition = null;
        foreach (var transition in _allTransitions)
        {
            if (transition.PropertyName == propertyName)
            {
                targetTransition = transition;
                break;
            }
        }

        if (targetTransition == null)
        {
            Debug.LogWarning($"Property '{propertyName}' not found in transition controller.");
            return;
        }

        // Get current progress of that specific property
        float currentProgress = targetTransition.Progress;
        float actualDuration = duration > 0 ? duration : targetTransition.Duration;

        // Create temporary coroutine for just this property
        StartCoroutine(TransitionSingleProperty(targetTransition, currentProgress, targetProgress, actualDuration));
    }

    /// <summary>
    /// Coroutine to transition a single property
    /// </summary>
    private IEnumerator TransitionSingleProperty(IPropertyTransition transition, float startProgress, float targetProgress, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = transition.EasingCurve.Evaluate(t);
            float currentProgress = Mathf.Lerp(startProgress, targetProgress, curvedT);

            // Update just this property
            transition.UpdateTransition(MaterialInstance, currentProgress);

            yield return null;
        }

        // Ensure we end at exact target value
        transition.UpdateTransition(MaterialInstance, targetProgress);
    }

    /// <summary>
    /// Immediately sets all properties to their end values
    /// </summary>
    public void SetToEndImmediate()
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        foreach (var transition in _allTransitions)
        {
            transition.SetToEndValue(MaterialInstance);
        }

        OverallTransitionProgress = 1f;
        OnTransitionProgressUpdated?.Invoke(1f);
        OnTransitionToEndComplete?.Invoke();
    }

    /// <summary>
    /// Immediately sets all properties to their start values
    /// </summary>
    public void SetToStartImmediate()
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        foreach (var transition in _allTransitions)
        {
            transition.SetToStartValue(MaterialInstance);
        }

        OverallTransitionProgress = 0f;
        OnTransitionProgressUpdated?.Invoke(0f);
        OnTransitionToStartComplete?.Invoke();
    }

    /// <summary>
    /// Immediately sets properties to reflect a specific progress value
    /// </summary>
    public void SetToProgressImmediate(float progress)
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        StopActiveTransition();

        float clampedProgress = Mathf.Clamp01(progress);
        OverallTransitionProgress = clampedProgress;

        foreach (var transition in _allTransitions)
        {
            transition.UpdateTransition(MaterialInstance, clampedProgress);
        }

        OnTransitionProgressUpdated?.Invoke(clampedProgress);
    }

    #endregion

    #region Transition Utilities

    /// <summary>
    /// Stops any active transition
    /// </summary>
    private void StopActiveTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        if (_activeTransitionManager != null)
        {
            _activeTransitionManager.Stop();
            _activeTransitionManager = null;
        }

        IsPaused = false;
    }

    /// <summary>
    /// Coroutine that runs a transition to completion
    /// </summary>
    private IEnumerator RunTransition(TransitionManager manager)
    {
        if (manager == null)
        {
            _transitionCoroutine = null;
            yield break;
        }

        while (manager.Update(Time.deltaTime, IsPaused))
        {
            yield return null;
        }

        _transitionCoroutine = null;
    }

    #endregion

    #region Type-Specific Transition Methods

    /// <summary>
    /// Transitions color properties to their end values
    /// </summary>
    public void TransitionColorsToEnd()
    {
        TransitionColorsToEnd(ColorTransitionProgress);
    }

    /// <summary>
    /// Transitions color properties to their end values from a specific progress point
    /// </summary>
    public void TransitionColorsToEnd(float fromProgress)
    {
        TransitionPropertiesToEnd<ColorPropertyTransition>(_colorTransitions, fromProgress);
    }

    /// <summary>
    /// Transitions color properties to their start values
    /// </summary>
    public void TransitionColorsToStart()
    {
        TransitionColorsToStart(ColorTransitionProgress);
    }

    /// <summary>
    /// Transitions color properties to their start values from a specific progress point
    /// </summary>
    public void TransitionColorsToStart(float fromProgress)
    {
        TransitionPropertiesToStart<ColorPropertyTransition>(_colorTransitions, fromProgress);
    }

    /// <summary>
    /// Transitions float properties to their end values
    /// </summary>
    public void TransitionFloatsToEnd()
    {
        TransitionFloatsToEnd(FloatTransitionProgress);
    }

    /// <summary>
    /// Transitions float properties to their end values from a specific progress point
    /// </summary>
    public void TransitionFloatsToEnd(float fromProgress)
    {
        TransitionPropertiesToEnd<FloatPropertyTransition>(_floatTransitions, fromProgress);
    }

    /// <summary>
    /// Transitions float properties to their start values
    /// </summary>
    public void TransitionFloatsToStart()
    {
        TransitionFloatsToStart(FloatTransitionProgress);
    }

    /// <summary>
    /// Transitions float properties to their start values from a specific progress point
    /// </summary>
    public void TransitionFloatsToStart(float fromProgress)
    {
        TransitionPropertiesToStart<FloatPropertyTransition>(_floatTransitions, fromProgress);
    }

    /// <summary>
    /// Transitions vector2 properties to their end values
    /// </summary>
    public void TransitionVector2sToEnd()
    {
        TransitionVector2sToEnd(Vector2TransitionProgress);
    }

    /// <summary>
    /// Transitions vector2 properties to their end values from a specific progress point
    /// </summary>
    public void TransitionVector2sToEnd(float fromProgress)
    {
        TransitionPropertiesToEnd<Vector2PropertyTransition>(_vector2Transitions, fromProgress);
    }

    /// <summary>
    /// Transitions vector2 properties to their start values
    /// </summary>
    public void TransitionVector2sToStart()
    {
        TransitionVector2sToStart(Vector2TransitionProgress);
    }

    /// <summary>
    /// Transitions vector2 properties to their start values from a specific progress point
    /// </summary>
    public void TransitionVector2sToStart(float fromProgress)
    {
        TransitionPropertiesToStart<Vector2PropertyTransition>(_vector2Transitions, fromProgress);
    }

    /// <summary>
    /// Helper method to transition specific property types to end values
    /// </summary>
    private void TransitionPropertiesToEnd<T>(List<T> transitions, float fromProgress) where T : IPropertyTransition
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null || transitions.Count == 0) return;

        StopActiveTransition();

        _activeTransitionManager = new TransitionManager(
            MaterialInstance,
            transitions.ConvertAll(t => t as IPropertyTransition).ToArray(),
            true, // to end values
            1.0f,
            (progress) =>
            {
                // Only update overall progress if sync option is enabled
                if (_syncCategoryTransitionsToOverall)
                {
                    UpdateTransitionProgress(progress);
                }
            },
            () => { OnTransitionToEndComplete?.Invoke(); },
            fromProgress
        );

        _transitionCoroutine = StartCoroutine(RunTransition(_activeTransitionManager));
    }

    /// <summary>
    /// Helper method to transition specific property types to start values
    /// </summary>
    private void TransitionPropertiesToStart<T>(List<T> transitions, float fromProgress) where T : IPropertyTransition
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null || transitions.Count == 0) return;

        StopActiveTransition();

        _activeTransitionManager = new TransitionManager(
            MaterialInstance,
            transitions.ConvertAll(t => t as IPropertyTransition).ToArray(),
            false, // to start values 
            1.0f,
            (progress) =>
            {
                // Only update overall progress if sync option is enabled
                if (_syncCategoryTransitionsToOverall)
                {
                    UpdateTransitionProgress(progress);
                }
            },
            () => { OnTransitionToStartComplete?.Invoke(); },
            fromProgress
        );

        _transitionCoroutine = StartCoroutine(RunTransition(_activeTransitionManager));
    }

    #endregion

    #region Immediate Setting Methods

    /// <summary>
    /// Sets color properties to their end values immediately
    /// </summary>
    public void SetColorsToEndImmediate()
    {
        SetPropertiesToEndImmediate<ColorPropertyTransition>(_colorTransitions);
    }

    /// <summary>
    /// Sets color properties to their start values immediately
    /// </summary>
    public void SetColorsToStartImmediate()
    {
        SetPropertiesToStartImmediate<ColorPropertyTransition>(_colorTransitions);
    }

    /// <summary>
    /// Sets float properties to their end values immediately
    /// </summary>
    public void SetFloatsToEndImmediate()
    {
        SetPropertiesToEndImmediate<FloatPropertyTransition>(_floatTransitions);
    }

    /// <summary>
    /// Sets float properties to their start values immediately
    /// </summary>
    public void SetFloatsToStartImmediate()
    {
        SetPropertiesToStartImmediate<FloatPropertyTransition>(_floatTransitions);
    }

    /// <summary>
    /// Sets vector2 properties to their end values immediately
    /// </summary>
    public void SetVector2sToEndImmediate()
    {
        SetPropertiesToEndImmediate<Vector2PropertyTransition>(_vector2Transitions);
    }

    /// <summary>
    /// Sets vector2 properties to their start values immediately
    /// </summary>
    public void SetVector2sToStartImmediate()
    {
        SetPropertiesToStartImmediate<Vector2PropertyTransition>(_vector2Transitions);
    }

    /// <summary>
    /// Helper method to set specific property types to end values immediately
    /// </summary>
    private void SetPropertiesToEndImmediate<T>(List<T> transitions) where T : IPropertyTransition
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        foreach (var transition in transitions)
        {
            transition.SetToEndValue(MaterialInstance);
        }
    }

    /// <summary>
    /// Helper method to set specific property types to start values immediately
    /// </summary>
    private void SetPropertiesToStartImmediate<T>(List<T> transitions) where T : IPropertyTransition
    {
        if (!_initialized)
            Initialize();

        if (MaterialInstance == null) return;

        foreach (var transition in transitions)
        {
            transition.SetToStartValue(MaterialInstance);
        }
    }

    #endregion

    #region Unity Event-Compatible Methods

    // Methods for triggering transitions from Unity Events

    // All properties methods
    [Tooltip("Transition all properties to their end values")]
    public void AllToEnd() => TransitionToEnd();

    [Tooltip("Transition all properties to their start values")]
    public void AllToStart() => TransitionToStart();

    [Tooltip("Immediately set all properties to their end values")]
    public void AllToEndImmediate() => SetToEndImmediate();

    [Tooltip("Immediately set all properties to their start values")]
    public void AllToStartImmediate() => SetToStartImmediate();

    [Tooltip("Transition all properties from 0 to 1")]
    public void AllToEndFromProgress(float resetProgress) => TransitionToEnd(resetProgress);

    [Tooltip("Transition all properties from 1 to 0")]
    public void AllToStartFromProgress(float resetProgress) => TransitionToStart(resetProgress);

    [Tooltip("Transition all properties to progress")]
    public void AllToProgress(float progress) => TransitionToProgress(progress);

    // Color properties methods
    [Tooltip("Transition color properties to their end values")]
    public void ChangeColorsToEnd() => TransitionColorsToEnd();

    [Tooltip("Transition color properties to their start values")]
    public void ChangeColorsToStart() => TransitionColorsToStart();

    [Tooltip("Immediately set color properties to their end values")]
    public void ColorsToEndImmediate() => SetColorsToEndImmediate();

    [Tooltip("Immediately set color properties to their start values")]
    public void ColorsToStartImmediate() => SetColorsToStartImmediate();

    [Tooltip("Transition color properties from progress to end")]
    public void ChangeColorsFromProgressToEnd(float progress) => TransitionColorsToEnd(progress);

    [Tooltip("Transition color properties from progress to start")]
    public void ChangeColorsFromProgressToStart(float progress) => TransitionColorsToStart(progress);

    [Tooltip("Transition color properties to progress")]
    public void ColorsToProgress(float progress) => TransitionColorsToEnd(progress);

    // Float properties methods
    [Tooltip("Transition float properties to their end values")]
    public void ChangeFloatsToEnd() => TransitionFloatsToEnd();

    [Tooltip("Transition float properties to their start values")]
    public void ChangeFloatsToStart() => TransitionFloatsToStart();

    [Tooltip("Immediately set float properties to their end values")]
    public void FloatsToEndImmediate() => SetFloatsToEndImmediate();

    [Tooltip("Immediately set float properties to their start values")]
    public void FloatsToStartImmediate() => SetFloatsToStartImmediate();

    [Tooltip("Transition float properties from progress to end")]
    public void ChangeFloatsFromProgressToEnd(float progress) => TransitionFloatsToEnd(progress);

    [Tooltip("Transition float properties from progress to start")]
    public void ChangeFloatsFromProgressToStart(float progress) => TransitionFloatsToStart(progress);

    [Tooltip("Transition float properties to progress")]
    public void FloatsToProgress(float progress) => TransitionFloatsToEnd(progress);

    // Vector2 properties methods
    [Tooltip("Transition vector2 properties to their end values")]
    public void ChangeVector2sToEnd() => TransitionVector2sToEnd();

    [Tooltip("Transition vector2 properties to their start values")]
    public void ChangeVector2sToStart() => TransitionVector2sToStart();

    [Tooltip("Immediately set vector2 properties to their end values")]
    public void Vector2sToEndImmediate() => SetVector2sToEndImmediate();

    [Tooltip("Immediately set vector2 properties to their start values")]
    public void Vector2sToStartImmediate() => SetVector2sToStartImmediate();

    [Tooltip("Transition vector2 properties from progress to end")]
    public void ChangeVector2sFromProgressToEnd(float progress) => TransitionVector2sToEnd(progress);

    [Tooltip("Transition vector2 properties from progress to start")]
    public void ChangeVector2sFromProgressToStart(float progress) => TransitionVector2sToStart(progress);

    [Tooltip("Transition vector2 properties to progress")]
    public void Vector2sToProgress(float progress) => TransitionVector2sToEnd(progress);

    // Control methods
    [Tooltip("Pause any active transition")]
    public void Pause() => PauseTransition();

    [Tooltip("Resume a paused transition")]
    public void Resume() => ResumeTransition();

    [Tooltip("Toggle between pause and resume")]
    public void TogglePause()
    {
        if (IsPaused)
            ResumeTransition();
        else
            PauseTransition();
    }

    // Progress presets
    [Tooltip("Set all properties to progress immediately")]
    public void SetToProgress(float progress) => SetToProgressImmediate(progress);

    #endregion
}