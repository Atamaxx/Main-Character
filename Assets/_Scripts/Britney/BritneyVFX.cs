using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.VFX;

namespace Britney
{
    public class BritneyVFX : MonoBehaviour
    {
        [SerializeField]
        private List<VisualEffect> _vfx = new();

        [BoxGroup("VFX Head"), SerializeField]
        private string _scaleParameterName = "ScaleVector";

        [BoxGroup("VFX Head"), SerializeField]
        private Vector2 _headVelocityMul = new(-0.5f, 0.5f);

        [BoxGroup("VFX Head"), SerializeField]
        private float _headYVelocityOnStatic = 0.25f;

        [BoxGroup("VFX Body"), SerializeField]
        private string _colorParameterName = "Color";

        [BoxGroup("VFX Body"), SerializeField]
        private Color _filledColor = new(0.0707547f, 0, 0, 0);

        [BoxGroup("VFX Body"), SerializeField]
        private Color _unfilledColor = new(0, 0, 0, 0);

        [BoxGroup("VFX Body"), SerializeField]
        private float _colorTransitionDuration = 0.5f;

        [BoxGroup("VFX Aura"), SerializeField]
        private VisualEffect _vfxAura;

        [BoxGroup("VFX Aura"), SerializeField]
        private string _spawnRateParameterName = "SpawnRate";

        [SerializeField]
        private Vector2 _afkThreshold = new(0.05f, 0.05f);

        public Transform ObjectToTrack; // The object whose velocity you want to calculate

        private int _bodyPartsNum;

        private int _scaleParameterIdHEAD;
        private int _colorParameterID;
        private int _spawnRateAuraParameterId;

        private Color[] _currentColors;
        private Color[] _targetColors;
        private Color[] _initialColors;

        private bool _isTransitioning;
        private float _transitionStartTime;

        void Start()
        {
            _bodyPartsNum = _vfx.Count;
            _scaleParameterIdHEAD = Shader.PropertyToID(_scaleParameterName);
            _colorParameterID = Shader.PropertyToID(_colorParameterName);
            _spawnRateAuraParameterId = Shader.PropertyToID(_spawnRateParameterName);

            // Initialize color arrays
            _currentColors = new Color[_bodyPartsNum];
            _targetColors = new Color[_bodyPartsNum];
            _initialColors = new Color[_bodyPartsNum];

            // Set initial colors to filledColor as an example
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                _currentColors[i] = _filledColor;
                _targetColors[i] = _filledColor;
                ChangeVector4Parameter(_vfx[i], _colorParameterID, _filledColor);
            }
        }

        void Update()
        {
            if (_isTransitioning)
            {
                float t = (Time.time - _transitionStartTime) / _colorTransitionDuration;
                t = Mathf.Clamp01(t);

                // Interpolate colors for all VFX
                for (int i = 0; i < _bodyPartsNum; i++)
                {
                    Color blended = Color.Lerp(_initialColors[i], _targetColors[i], t);
                    ChangeVector4Parameter(_vfx[i], _colorParameterID, blended);
                }

                if (t >= 1f)
                {
                    // Transition completed, record new current colors
                    for (int i = 0; i < _bodyPartsNum; i++)
                    {
                        _currentColors[i] = _targetColors[i];
                    }
                    _isTransitioning = false;
                }
            }
        }

        #region PUBLIC
        public void Aura(int minSpawnRate, int maxAuraSpawnRate, bool isStatic)
        {
            int spawnRateValue = maxAuraSpawnRate;

            if (isStatic)
            {
                spawnRateValue = minSpawnRate;
            }

            ChangeIntParameter(_vfxAura, _spawnRateAuraParameterId, spawnRateValue);
        }

        public void RedrawBody(int inkAmountUsed, int maxInkAmount)
        {
            // Calculate how many body parts should appear 'unfilled'
            int inkUsedToBodyCount = (int)
                Mathf.Lerp(0, _bodyPartsNum, (float)inkAmountUsed / maxInkAmount);

            // Set target colors based on ink usage
            for (int i = 1; i < _bodyPartsNum; i++)
            {
                _targetColors[i] = i <= inkUsedToBodyCount ? _unfilledColor : _filledColor;
            }

            // For the first element (e.g., head):
            if (inkUsedToBodyCount == _bodyPartsNum)
                _targetColors[0] = _unfilledColor;
            else
                _targetColors[0] = _filledColor;

            // Check if any change is needed
            bool changeNeeded = false;
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                if (_targetColors[i] != _currentColors[i])
                {
                    changeNeeded = true;
                    break;
                }
            }

            if (!changeNeeded)
                return;

            // Prepare for transition
            for (int i = 0; i < _bodyPartsNum; i++)
            {
                _initialColors[i] = _currentColors[i];
            }

            _transitionStartTime = Time.time;
            _isTransitioning = true;
        }
        #endregion

        private void Head(Vector2 velocity)
        {
            Vector3 vectorValue = velocity * _headVelocityMul;
            if (velocity.y == 0)
            {
                vectorValue = new Vector2(-velocity.x, _headYVelocityOnStatic);
            }

            ChangeVector3Parameter(_vfx[0], _scaleParameterIdHEAD, vectorValue);
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
}
