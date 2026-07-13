using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using NaughtyAttributes;

public class VFXController : MonoBehaviour
{
    [SerializeField] private List<VisualEffect> vfx = new();
    [BoxGroup("VFX Head"), SerializeField] private string scaleParameterName = "ScaleVector";
    [BoxGroup("VFX Head"), SerializeField] private Vector2 _headVelocityMul = new(-0.5f, 0.5f);
    [BoxGroup("VFX Head"), SerializeField] private float _headYVelocityOnStatic = 0.25f;

    [BoxGroup("VFX Body"), SerializeField] private string colorParameterName = "Color";
    [BoxGroup("VFX Body"), SerializeField] private Color _filledColor = new(0.0707547f, 0, 0, 0);
    [BoxGroup("VFX Body"), SerializeField] private Color _unfilledColor = new(0, 0, 0, 0);
    [BoxGroup("VFX Body"), SerializeField] private float colorTransitionDuration = 0.5f;

    [BoxGroup("VFX Aura"), SerializeField] private VisualEffect vfxAura;
    [BoxGroup("VFX Aura"), SerializeField] private string _spawnRateParameterName = "SpawnRate";

    [SerializeField] private Vector2 _afkThreshold = new(0.05f, 0.05f);

    public Transform ObjectToTrack; // The object whose velocity you want to calculate

    private Vector2 _velocity;
    private Vector2 _lastPosition;
    private float _lastTime;
    private int _bodyPartsNum;

    private int scaleParameterIdHEAD;
    private int colorParameterID;
    private int _spawnRateAuraParameterId;

    private Color[] currentColors;
    private Color[] targetColors;
    private Color[] initialColors;

    private bool isTransitioning;
    private float transitionStartTime;

    void Start()
    {
        _bodyPartsNum = vfx.Count;
        scaleParameterIdHEAD = Shader.PropertyToID(scaleParameterName);
        colorParameterID = Shader.PropertyToID(colorParameterName);
        _spawnRateAuraParameterId = Shader.PropertyToID(_spawnRateParameterName);

        _lastPosition = ObjectToTrack.position;
        _lastTime = Time.time;

        // Initialize color arrays
        currentColors = new Color[_bodyPartsNum];
        targetColors = new Color[_bodyPartsNum];
        initialColors = new Color[_bodyPartsNum];

        // Set initial colors to filledColor as an example
        for (int i = 0; i < _bodyPartsNum; i++)
        {
            currentColors[i] = _filledColor;
            targetColors[i] = _filledColor;
            ChangeVector4Parameter(vfx[i], colorParameterID, _filledColor);
        }
    }

    void Update()
    {
        if (isTransitioning)
        {
            float t = (Time.time - transitionStartTime) / colorTransitionDuration;
            t = Mathf.Clamp01(t);

            // Interpolate colors for all VFX
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                Color blended = Color.Lerp(initialColors[i], targetColors[i], t);
                ChangeVector4Parameter(vfx[i], colorParameterID, blended);
            }

            if (t >= 1f)
            {
                // Transition completed, record new current colors
                for (int i = 0; i < _bodyPartsNum; i++)
                {
                    currentColors[i] = targetColors[i];
                }
                isTransitioning = false;
            }
        }
    }

    void FixedUpdate()
    {
        CalculateObjectVelocity();
        //Head();
        AfkCheck();

    }

    private void CalculateObjectVelocity()
    {
        Vector2 currentPosition = ObjectToTrack.position;
        Vector2 deltaPosition = currentPosition - _lastPosition;

        float currentTime = Time.time;
        float deltaTime = currentTime - _lastTime;

        _velocity = deltaPosition / deltaTime;

        _lastPosition = currentPosition;
        _lastTime = currentTime;
    }


    public bool _isAFK;
    private void AfkCheck()
    {
        _isAFK = false;

        if (Mathf.Abs(_velocity.x) < _afkThreshold.x && Mathf.Abs(_velocity.y) < _afkThreshold.y)
        {
            _isAFK = true;
        }
    }

    #region PUBLIC
    public void Aura(int minSpawnRate, int maxAuraSpawnRate)
    {
        int spawnRateValue = maxAuraSpawnRate;

        if (_isAFK)
        {
            spawnRateValue = minSpawnRate;
        }

        ChangeIntParameter(vfxAura, _spawnRateAuraParameterId, spawnRateValue);
    }

    public void RedrawBody(int inkAmountUsed, int maxInkAmount)
    {
        // Calculate how many body parts should appear 'unfilled'
        int inkUsedToBodyCount = (int)Mathf.Lerp(0, _bodyPartsNum, (float)inkAmountUsed / maxInkAmount);

        // Set target colors based on ink usage
        for (int i = 1; i < _bodyPartsNum; i++)
        {
            targetColors[i] = i <= inkUsedToBodyCount ? _unfilledColor : _filledColor;
        }

        // For the first element (e.g., head):
        if (inkUsedToBodyCount == _bodyPartsNum)
            targetColors[0] = _unfilledColor;
        else
            targetColors[0] = _filledColor;

        // Check if any change is needed
        bool changeNeeded = false;
        for (int i = 0; i < _bodyPartsNum; i++)
        {
            if (targetColors[i] != currentColors[i])
            {
                changeNeeded = true;
                break;
            }
        }

        if (!changeNeeded) return;

        // Prepare for transition
        for (int i = 0; i < _bodyPartsNum; i++)
        {
            initialColors[i] = currentColors[i];
        }

        transitionStartTime = Time.time;
        isTransitioning = true;
    }
    #endregion

    private void Head()
    {
        Vector3 vectorValue = _velocity * _headVelocityMul;
        if (_velocity.y == 0)
        {
            vectorValue = new Vector2(-_velocity.x, _headYVelocityOnStatic);
        }

        ChangeVector3Parameter(vfx[0], scaleParameterIdHEAD, vectorValue);
    }

    void ChangeVector3Parameter(VisualEffect vfxComp, int parameterId, Vector3 newValue)
    {
        if (vfxComp != null && vfxComp.HasVector3(parameterId))
        {
            vfxComp.SetVector3(parameterId, newValue);
        }
    }

    void ChangeVector4Parameter(VisualEffect vfxComp, int parameterId, Color newColor)
    {
        if (vfxComp != null && vfxComp.HasVector4(parameterId))
        {
            vfxComp.SetVector4(parameterId, newColor);
        }
    }

    void ChangeIntParameter(VisualEffect vfxComp, int parameterId, int newValue)
    {
        if (vfxComp != null && vfxComp.HasInt(parameterId))
        {
            vfxComp.SetInt(parameterId, newValue);
        }
    }
}
