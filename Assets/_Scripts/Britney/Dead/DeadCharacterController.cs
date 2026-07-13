using UnityEngine;

public class DeadCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private float _followSpeed = 5f;

    [SerializeField]
    private float _rotationSpeed = 8f;

    [Header("Limbs")]
    [SerializeField]
    private Transform _leftLegTarget;

    [SerializeField]
    private Transform _rightLegTarget;

    [SerializeField]
    private Transform _leftArmTarget;

    [SerializeField]
    private Transform _rightArmTarget;

    [Header("Ragdoll Effect")]
    [SerializeField]
    private float _limbTrailAmount = 1.5f;

    [SerializeField]
    private float _limbOscillationAmount = 0.1f;

    [SerializeField]
    private float _limbOscillationSpeed = 3f;

    private Vector3 _cursorPosition;
    private Vector3 _previousPosition;
    private Vector2 _movementDirection;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        // Use kinematic body for direct control
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
        _rb.freezeRotation = false;

        _previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (_cursorPosition == Vector3.zero)
            return;

        // Calculate direction to cursor
        Vector3 direction = _cursorPosition - transform.position;
        float distance = direction.magnitude;

        // Move directly toward cursor (no momentum/physics)
        if (distance > 0.01f)
        {
            // Move toward cursor at speed proportional to distance
            Vector3 newPosition = Vector3.Lerp(
                transform.position,
                _cursorPosition,
                Time.fixedDeltaTime * _followSpeed
            );

            // Calculate movement direction for limb effects
            _movementDirection = (newPosition - _previousPosition).normalized;
            _previousPosition = transform.position;

            // Apply position directly
            transform.position = newPosition;

            // Rotate to face movement direction
            if (_movementDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle =
                    Mathf.Atan2(_movementDirection.y, _movementDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0, 0, targetAngle + 90f),
                    Time.fixedDeltaTime * _rotationSpeed
                );
            }
        }

        UpdateLimbPositions();
    }

    private void UpdateLimbPositions()
    {
        // Inverse movement direction for dragging limbs behind
        Vector2 dragDirection = -_movementDirection * _limbTrailAmount;

        // Left leg (back leg trails more)
        ApplyRagdollToLimb(_leftLegTarget, new Vector2(-0.4f, -1f), dragDirection * 1.2f);

        // Right leg
        ApplyRagdollToLimb(_rightLegTarget, new Vector2(0.4f, -1f), dragDirection);

        // Arms hang limp, with slight oscillation
        ApplyRagdollToLimb(_leftArmTarget, new Vector2(-0.8f, 0f), dragDirection * 0.8f);
        ApplyRagdollToLimb(_rightArmTarget, new Vector2(0.8f, 0f), dragDirection * 0.8f);
    }

    private void ApplyRagdollToLimb(Transform limb, Vector2 baseOffset, Vector2 dragOffset)
    {
        if (limb == null)
            return;

        // Calculate base position relative to character
        Vector3 basePos = transform.TransformPoint(baseOffset);

        // Apply drag effect based on movement
        Vector3 draggedPos = basePos + (Vector3)dragOffset;

        // Add oscillation for natural limb movement
        float oscillationX = Mathf.Sin(Time.time * _limbOscillationSpeed) * _limbOscillationAmount;
        float oscillationY =
            Mathf.Cos(Time.time * (_limbOscillationSpeed * 0.7f)) * _limbOscillationAmount;

        Vector3 targetPos = draggedPos + new Vector3(oscillationX, oscillationY, 0);

        // Smooth limb movement
        limb.position = Vector3.Lerp(limb.position, targetPos, Time.deltaTime * 7f);
    }

    public void SetCursorPosition(Vector3 position)
    {
        _cursorPosition = position;
    }
}
