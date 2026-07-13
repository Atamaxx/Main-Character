using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;

/// <summary>
/// Adjusts multiple LineRenderers' lengths based on volume levels from the AudioSystem
/// </summary>
public class VolumeLineRenderer : MonoBehaviour
{
    #region Serialized Fields
    
    [BoxGroup("Volume Settings")]
    [SerializeField] private VolumeType _volumeType;
    
    [BoxGroup("Line Settings")]
    [SerializeField, ReorderableList] private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    
    [BoxGroup("Debug")]
    [SerializeField] private bool _debugLogging = false;
    
    #endregion
    
    #region Private Fields
    
    private AudioSystem _audioSystem;
    private float _lastVolume = -1f;
    
    // Dictionary to store original positions for each line renderer
    private Dictionary<LineRenderer, Vector3[]> _originalPositionsMap = new Dictionary<LineRenderer, Vector3[]>();
    private Dictionary<LineRenderer, Vector3> _firstPositionsMap = new Dictionary<LineRenderer, Vector3>();
    
    #endregion
    
    #region Unity Lifecycle Methods
    
    private void Awake()
    {
        Initialize();
    }
    
    private void Start()
    {
        // Force initial update
        UpdateLineRenderersBasedOnVolume();
    }
    
    private void Update()
    {
        // Check if volume has changed
        float currentVolume = GetCurrentVolume();
        
        if (!Mathf.Approximately(currentVolume, _lastVolume))
        {
            _lastVolume = currentVolume;
            UpdateLineRenderersBasedOnVolume();
            
            if (_debugLogging)
            {
                Debug.Log($"[VolumeLineRenderer] Volume changed: {_volumeType} = {currentVolume}");
            }
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Initialize the component
    /// </summary>
    private void Initialize()
    {
        _audioSystem = AudioSystem.Instance;
        
        // Auto-find line renderer on this object if list is empty
        if (_lineRenderers.Count == 0)
        {
            LineRenderer lr = GetComponent<LineRenderer>();
            if (lr != null)
            {
                _lineRenderers.Add(lr);
            }
        }
        
        if (_lineRenderers.Count == 0)
        {
            Debug.LogWarning("[VolumeLineRenderer] No LineRenderers assigned!");
            enabled = false;
            return;
        }
        
        // Store original positions for each line renderer
        StoreOriginalPositions();
    }
    
    /// <summary>
    /// Stores the original positions of all line renderers
    /// </summary>
    private void StoreOriginalPositions()
    {
        _originalPositionsMap.Clear();
        _firstPositionsMap.Clear();
        
        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
            if (lineRenderer == null) continue;
            
            int pointCount = lineRenderer.positionCount;
            if (pointCount == 0) continue;
            
            Vector3[] originalPositions = new Vector3[pointCount];
            
            for (int i = 0; i < pointCount; i++)
            {
                originalPositions[i] = lineRenderer.GetPosition(i);
            }
            
            _originalPositionsMap[lineRenderer] = originalPositions;
            _firstPositionsMap[lineRenderer] = originalPositions[0];
        }
    }
    
    /// <summary>
    /// Updates all line renderers points based on the current volume level
    /// </summary>
    private void UpdateLineRenderersBasedOnVolume()
    {
        if (_audioSystem == null || _lineRenderers.Count == 0)
            return;
        
        float currentVolume = GetCurrentVolume();
        
        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
            if (lineRenderer == null) continue;
            if (!_originalPositionsMap.ContainsKey(lineRenderer)) continue;
            
            Vector3[] originalPositions = _originalPositionsMap[lineRenderer];
            Vector3 firstPosition = _firstPositionsMap[lineRenderer];
            
            // Calculate total length of the line
            float[] cumulativeDistances = new float[originalPositions.Length];
            cumulativeDistances[0] = 0;
            float totalLength = 0;
            
            for (int i = 1; i < originalPositions.Length; i++)
            {
                float segmentLength = Vector3.Distance(originalPositions[i-1], originalPositions[i]);
                totalLength += segmentLength;
                cumulativeDistances[i] = totalLength;
            }
            
            // Keep first point fixed
            lineRenderer.SetPosition(0, firstPosition);
            
            if (originalPositions.Length <= 1 || totalLength == 0) continue;
            
            // Scale positions along the line based on volume
            for (int i = 1; i < originalPositions.Length; i++)
            {
                // Calculate position along the scaled line
                float ratio = cumulativeDistances[i] / totalLength;
                float scaledRatio = ratio * currentVolume;
                
                // Find the corresponding position
                if (scaledRatio <= 0)
                {
                    lineRenderer.SetPosition(i, originalPositions[0]);
                    continue;
                }
                
                // Find the segment this scaled point falls on
                int segmentIndex = 0;
                for (int j = 1; j < originalPositions.Length; j++)
                {
                    if (cumulativeDistances[j] / totalLength >= scaledRatio)
                    {
                        segmentIndex = j - 1;
                        break;
                    }
                }
                
                // Calculate interpolation within the segment
                float segmentStart = cumulativeDistances[segmentIndex] / totalLength;
                float segmentEnd = cumulativeDistances[segmentIndex + 1] / totalLength;
                float segmentRatio = (scaledRatio - segmentStart) / (segmentEnd - segmentStart);
                
                // Interpolate between the segment's start and end points
                Vector3 newPosition = Vector3.Lerp(
                    originalPositions[segmentIndex], 
                    originalPositions[segmentIndex + 1], 
                    segmentRatio);
                
                lineRenderer.SetPosition(i, newPosition);
            }
        }
        
        if (_debugLogging)
        {
            Debug.Log($"[VolumeLineRenderer] {_volumeType} Volume: {currentVolume}, Lines scaled to {currentVolume * 100}% of original");
        }
    }
    
    /// <summary>
    /// Gets the current volume based on the selected volume type
    /// </summary>
    private float GetCurrentVolume()
    {
        if (_audioSystem == null)
        {
            Debug.LogWarning("[VolumeLineRenderer] AudioSystem is null!");
            return 0f;
        }
        
        switch (_volumeType)
        {
            case VolumeType.Master: return _audioSystem.MasterVolume;
            case VolumeType.Music: return _audioSystem.MusicVolume;
            case VolumeType.Ambience: return _audioSystem.AmbienceVolume;
            case VolumeType.SoundEffects: return _audioSystem.SFXVolume;
            default: return 0f;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Force updates all line lengths immediately
    /// </summary>
    [Button]
    public void ForceUpdate()
    {
        UpdateLineRenderersBasedOnVolume();
    }
    
    /// <summary>
    /// Reset all lines to their original positions
    /// </summary>
    [Button]
    public void ResetLines()
    {
        foreach (LineRenderer lineRenderer in _lineRenderers)
        {
            if (lineRenderer == null || !_originalPositionsMap.ContainsKey(lineRenderer)) continue;
            
            Vector3[] originalPositions = _originalPositionsMap[lineRenderer];
            
            for (int i = 0; i < originalPositions.Length; i++)
            {
                lineRenderer.SetPosition(i, originalPositions[i]);
            }
        }
    }
    
    /// <summary>
    /// Add a line renderer to be controlled
    /// </summary>
    public void AddLineRenderer(LineRenderer lineRenderer)
    {
        if (lineRenderer == null || _lineRenderers.Contains(lineRenderer)) return;
        
        _lineRenderers.Add(lineRenderer);
        
        // Store original positions for the new line renderer
        int pointCount = lineRenderer.positionCount;
        if (pointCount == 0) return;
        
        Vector3[] originalPositions = new Vector3[pointCount];
        
        for (int i = 0; i < pointCount; i++)
        {
            originalPositions[i] = lineRenderer.GetPosition(i);
        }
        
        _originalPositionsMap[lineRenderer] = originalPositions;
        _firstPositionsMap[lineRenderer] = originalPositions[0];
        
        // Update the new line renderer
        UpdateLineRenderersBasedOnVolume();
    }
    
    /// <summary>
    /// Remove a line renderer from control
    /// </summary>
    public void RemoveLineRenderer(LineRenderer lineRenderer)
    {
        if (lineRenderer == null || !_lineRenderers.Contains(lineRenderer)) return;
        
        _lineRenderers.Remove(lineRenderer);
        _originalPositionsMap.Remove(lineRenderer);
        _firstPositionsMap.Remove(lineRenderer);
    }
    
    /// <summary>
    /// Refresh original positions from all current line renderers
    /// </summary>
    [Button]
    public void RefreshOriginalPositions()
    {
        StoreOriginalPositions();
        UpdateLineRenderersBasedOnVolume();
    }
    
    /// <summary>
    /// Set the volume type to monitor
    /// </summary>
    public void SetVolumeType(VolumeType volumeType)
    {
        _volumeType = volumeType;
        UpdateLineRenderersBasedOnVolume();
    }
    
    #endregion
}