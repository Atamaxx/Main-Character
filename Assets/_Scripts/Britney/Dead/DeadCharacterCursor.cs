using UnityEngine;
using UnityEngine.InputSystem;

public class DeadDragCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform _characterTransform;

    [SerializeField]
    private DeadCharacterController _character;

    [SerializeField]
    private CustomTrailRenderer _trailRenderer;

    [SerializeField]
    private LineRenderer _connectionLine;

    [SerializeField]
    private Sprite _cursorSprite;

    [Header("Settings")]
    [SerializeField]
    private float _maxDistance = 4f;

    [SerializeField]
    private float _minDistance = 0.5f;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    [Header("Trail Effects")]
    [SerializeField]
    private float _pulseFrequency = 1.5f;

    [SerializeField]
    private float _pulseAmount = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private float _trailTime;

    private void Start()
    {
        Cursor.visible = false;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (_cursorSprite != null)
            _spriteRenderer.sprite = _cursorSprite;

        if (_connectionLine == null)
            _connectionLine = GetComponent<LineRenderer>();

        transform.position = _characterTransform.position;
        RepositionCursor(_characterTransform);
    }

    private void Update()
    {
        _trailTime += Time.deltaTime;

        // Get cursor position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos) + _offset;

        // Constrain distance
        Vector3 toCharacter = worldPos - _characterTransform.position;
        float distance = toCharacter.magnitude;

        if (distance > _maxDistance)
        {
            worldPos = _characterTransform.position + toCharacter.normalized * _maxDistance;
            Mouse.current.WarpCursorPosition(Camera.main.WorldToScreenPoint(worldPos));
        }
        else if (distance < _minDistance)
        {
            worldPos = _characterTransform.position + toCharacter.normalized * _minDistance;
        }

        // Update positions
        transform.position = worldPos;
        _character.SetCursorPosition(worldPos);

        UpdateTrail();
        UpdateConnectionLine(distance);
    }

    private void UpdateTrail()
    {
        if (_trailRenderer != null)
        {
            // Force the trail to start at character
            if (_trailRenderer.GetType().GetMethod("ForceFirstPoint") != null)
            {
                _trailRenderer.SendMessage("ForceFirstPoint", _characterTransform.position);
            }
        }
    }

    private void UpdateConnectionLine(float distance)
    {
        if (_connectionLine != null)
        {
            // Create a pulsing/elastic effect on the line
            float pulse = 1f + Mathf.Sin(_trailTime * _pulseFrequency) * _pulseAmount;

            // Set line width based on distance and pulse
            float width = Mathf.Lerp(0.2f, 0.05f, distance / _maxDistance) * pulse;
            _connectionLine.startWidth = width;
            _connectionLine.endWidth = width * 0.5f;

            // Update line positions
            _connectionLine.positionCount = 2;
            _connectionLine.SetPosition(0, _characterTransform.position);
            _connectionLine.SetPosition(1, transform.position);
        }
    }

    public void RepositionCursor(Transform target)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(target.position);
        Mouse.current.WarpCursorPosition(screenPos);
    }
}
