using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class RiverLevelChecker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _target;

    [Header("Events")]
    [SerializeField] private UnityEvent<Vector2> _onSubmerged;
    [SerializeField] private UnityEvent<Vector2> _onSubmergedAndCondition;

    [Header("Shader Settings")]
    [Tooltip("The name of the shader property controlling the fill level.")]
    [SerializeField] private float _startingWaterLevel = 0.128f;
    [SerializeField] private float _waterLevelOffsetY = 0f;
    [SerializeField] private string _riverLevelPropertyName = "_River_Level";
    [SerializeField] private SpriteRenderer _spriteRenderer;

    // Used to ensure the event is only invoked once on the transition.
    private bool _wasUnder = false;
    // To be set from outside to trigger the special _onSubmergedAndCondition.
    private bool _isSpecialConditionMet = false;


    public void SetSpecialConditionMet(bool isMet)
    {
        _isSpecialConditionMet = isMet;
    }

    private void Start()
    {
        _spriteRenderer.sharedMaterial.SetFloat(_riverLevelPropertyName, _startingWaterLevel);
    }
    private void Update()
    {
        if (_spriteRenderer == null || _target == null)
        {
            return;
        }

        // In play mode we want the runtime instance of the material.
        float shaderValue = _spriteRenderer.sharedMaterial.GetFloat(_riverLevelPropertyName);
        float fillFraction = Mathf.Clamp01(shaderValue);

        float waterLevelY = CalculateWaterLevelY(fillFraction) + _waterLevelOffsetY;

        bool isUnder = _target.position.y < waterLevelY;

        if (isUnder && !_wasUnder)
        {
            if (_isSpecialConditionMet)
                _onSubmergedAndCondition.Invoke(new(_target.position.x, waterLevelY));
            else
                _onSubmerged.Invoke(new(_target.position.x, waterLevelY));
        }

        _wasUnder = isUnder;
    }

    /// <summary>
    /// Computes the world-space Y position of the water level based on the sprite bounds.
    /// </summary>
    /// <param name="fillFraction">The fraction (0-1) of the sprite's height that is filled.</param>
    /// <returns>The Y coordinate of the water line.</returns>
    private float CalculateWaterLevelY(float fillFraction)
    {
        Bounds bounds = _spriteRenderer.bounds;
        return bounds.min.y + fillFraction * bounds.size.y;
    }

    /// <summary>
    /// Draws a gizmo line in the scene view indicating the current water level.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        // Use sharedMaterial to avoid instantiating a new material in edit mode.
        Material sharedMat = _spriteRenderer.sharedMaterial;
        if (sharedMat == null)
        {
            return;
        }

        float shaderValue = sharedMat.GetFloat(_riverLevelPropertyName);
        float fillFraction = Mathf.Clamp01(shaderValue);

        float waterLevelY = CalculateWaterLevelY(fillFraction) + _waterLevelOffsetY;
        Bounds bounds = _spriteRenderer.bounds;

        // Set the Gizmos color and draw a horizontal line across the sprite.
        Gizmos.color = Color.blue;
        Vector3 leftPoint = new Vector3(bounds.min.x, waterLevelY, bounds.min.z);
        Vector3 rightPoint = new Vector3(bounds.max.x, waterLevelY, bounds.min.z);
        Gizmos.DrawLine(leftPoint, rightPoint);
    }
}
