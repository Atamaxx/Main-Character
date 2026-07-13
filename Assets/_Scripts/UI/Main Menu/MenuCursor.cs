using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCursor : MonoBehaviour
{
    [SerializeField, Required]
    private Transform _headTransform;

    [SerializeField, Required]
    private LineRenderer _lineRenderer;

    [SerializeField, Tooltip("The sprite to use as the menu cursor.")]
    private Sprite _cursorSprite;

    [SerializeField, Tooltip("Optional offset for the cursor position.")]
    private Vector3 _offset = Vector3.zero;

    [Header("Custom Trail Renderer Settings")]
    [SerializeField]
    private CustomTrailRenderer _customTrailRenderer;

    [SerializeField, Tooltip("How long the trail lasts in seconds.")]
    private float _trailTime = 0.2f;

    [SerializeField, Tooltip("Trail lifetime when holding down the mouse.")]
    private float _trailTimeOnHold = 0.4f;

    [SerializeField]
    private float _trailMinVertexDistance = 0.05f;

    [SerializeField, Tooltip("Starting width of the trail.")]
    private float _startWidth = 0.1f;

    [SerializeField, Tooltip("Ending width of the trail.")]
    private float _endWidth = 0f;

    [
        SerializeField,
        Tooltip("Material to use for the trail. If left empty, a default Sprite material is used.")
    ]
    private Material _trailMaterial;

    [SerializeField, Tooltip("Starting color of the trail.")]
    private Color _startColor = Color.white;

    [SerializeField, Tooltip("Ending color of the trail.")]
    private Color _endColor = new Color(1, 1, 1, 0);

    [Header("Animation Settings")]
    [SerializeField, Tooltip("The scale multiplier when the mouse button is held.")]
    private float _clickScaleMultiplier = 1.5f;

    [SerializeField, Tooltip("The duration of the animation to the active state (in seconds).")]
    private float _scaleUpDuration = 0.1f;

    [SerializeField, Tooltip("The duration of the animation back to normal state (in seconds).")]
    private float _scaleDownDuration = 0.1f;

    [SerializeField, Tooltip("The color to change to when the mouse button is held.")]
    private Color _holdColor = Color.gray;

    [Header("Dynamic Scaling Settings")]
    [SerializeField, Tooltip("Multiplier for trail length influence on dynamic scaling.")]
    private float scaleModifier = 1f;

    [SerializeField, Tooltip("Minimum dynamic scale multiplier.")]
    private float minDynamicScale = 0.5f;

    [SerializeField, Tooltip("Maximum dynamic scale multiplier.")]
    private float maxDynamicScale = 1f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _animationCoroutine;
    private Vector3 _originalScale;

    // Holds the scale value modified by animations (click effects).
    private Vector3 _currentAnimatedScale;
    private Color _originalColor;

    private bool _isCursorEnabled = false;

    void Awake()
    {
        // Hide the system cursor.
        Cursor.visible = false;

        // Save the original scale and initialize animated scale.
        _originalScale = transform.localScale;
        _currentAnimatedScale = _originalScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        if (_cursorSprite != null)
        {
            _spriteRenderer.sprite = _cursorSprite;
        }
        _originalColor = _spriteRenderer.color;

        // Set up the CustomTrailRenderer.
        if (_customTrailRenderer == null)
        {
            _customTrailRenderer = GetComponent<CustomTrailRenderer>();
            if (_customTrailRenderer == null)
            {
                _customTrailRenderer = gameObject.AddComponent<CustomTrailRenderer>();
            }
        }
        _customTrailRenderer.trailTime = _trailTime;
        _customTrailRenderer.trailMinVertexDistance = _trailMinVertexDistance;
        _customTrailRenderer.startWidth = _startWidth;
        _customTrailRenderer.endWidth = _endWidth;
        _customTrailRenderer.startColor = _startColor;
        _customTrailRenderer.endColor = _endColor;

        LineRenderer lr = _customTrailRenderer.GetComponent<LineRenderer>();
        if (lr != null)
        {
            if (_trailMaterial != null)
            {
                lr.material = _trailMaterial;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
    }

    public void EnableCursor()
    {
        if (_isCursorEnabled)
            return;

        _isCursorEnabled = true;
        _customTrailRenderer.enabled = true;
        _spriteRenderer.enabled = true;
        _lineRenderer.enabled = true;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(_headTransform.position);
        Mouse.current.WarpCursorPosition(screenPos);
    }

    public void DisableCursor()
    {
        if (!_isCursorEnabled)
            return;

        _isCursorEnabled = false;
        _lineRenderer.enabled = false;
        _customTrailRenderer.enabled = false;
        _spriteRenderer.enabled = false;
    }

    public void SetIsCursorEnabled(bool isEnabled)
    {
        _isCursorEnabled = isEnabled;
    }

    void Update()
    {
        if (_isCursorEnabled)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Adjust the distance from the camera if needed.
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos) + _offset;
            transform.position = worldPos;

            // On mouse press: animate to active state and extend trail time.
            if (Input.GetMouseButtonDown(0))
            {
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }
                _customTrailRenderer.trailTime = _trailTimeOnHold;
                Vector3 activeScale = _originalScale * _clickScaleMultiplier;
                _animationCoroutine = StartCoroutine(
                    AnimateState(activeScale, _holdColor, _scaleUpDuration)
                );
            }

            // On mouse release: animate back to normal state and reset trail time.
            if (Input.GetMouseButtonUp(0))
            {
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }
                _customTrailRenderer.trailTime = _trailTime;
                _animationCoroutine = StartCoroutine(
                    AnimateState(_originalScale, _originalColor, _scaleDownDuration)
                );
            }
        }
        else
        {
            transform.position = _headTransform.position;
        }

        // Dynamic scaling based on the current trail length.
        // The formula uses the scaleModifier to adjust sensitivity, and clamps the result between minDynamicScale and maxDynamicScale.
        float trailLength = _customTrailRenderer.CurrentTrailLength;
        float dynamicFactor = Mathf.Clamp(
            1f / (1f + (trailLength * scaleModifier)),
            minDynamicScale,
            maxDynamicScale
        );
        transform.localScale = _currentAnimatedScale * dynamicFactor;
    }

    /// <summary>
    /// Animates the cursor’s base scale (_currentAnimatedScale) and color toward target values.
    /// </summary>
    private IEnumerator AnimateState(Vector3 targetScale, Color targetColor, float duration)
    {
        Vector3 initialScale = _currentAnimatedScale;
        Color initialColor = _spriteRenderer.color;
        float timer = 0f;

        while (timer < duration)
        {
            float lerp = timer / duration;
            _currentAnimatedScale = Vector3.Lerp(initialScale, targetScale, lerp);
            Color color = Color.Lerp(initialColor, targetColor, lerp);
            _spriteRenderer.color = color;
            _customTrailRenderer.startColor = color;
            _customTrailRenderer.endColor = color;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        _currentAnimatedScale = targetScale;
        _spriteRenderer.color = targetColor;
        _customTrailRenderer.startColor = targetColor;
        _customTrailRenderer.endColor = targetColor;
    }

    public void RepositionCursor(Transform transform)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Mouse.current.WarpCursorPosition(screenPos);
    }
}
