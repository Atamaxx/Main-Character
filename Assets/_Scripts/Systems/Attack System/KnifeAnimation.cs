using UnityEngine;

public class KnifeAnimation : MonoBehaviour
{
    private enum KnifeState
    {
        Idle,
        Prepare,
        Attack,
        Stuck, // New state for pausing when hitting a target
        BackToIdle,
    }

    [Header("Path Settings")]
    [Tooltip("LineRenderer defines the knife's attack path (requires at least two points).")]
    [SerializeField]
    private LineRenderer pathLineRenderer;

    [Header("Easing & Timing")]
    [Tooltip("AnimationCurve for easing the transitions.")]
    [SerializeField]
    private AnimationCurve _prepareCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    private AnimationCurve _attackCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    private AnimationCurve _releaseCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Time in seconds to move from idle to prepare.")]
    [SerializeField]
    private float prepareDuration = 0.5f;

    [Tooltip("Time in seconds to move along the attack path.")]
    [SerializeField]
    private float attackDuration = 0.3f;

    [Tooltip("Time in seconds to remain stuck in target.")]
    [SerializeField]
    private float stuckDuration = 0.2f;

    [Tooltip("Time in seconds to return to idle after the attack.")]
    [SerializeField]
    private float backDuration = 0.3f;

    [Header("Idle Movement")]
    [Tooltip("The amplitude for slight idle movement (local offset).")]
    [SerializeField]
    private float idleMovementAmplitude = 0.05f;

    [Header("Attack Components")]
    [SerializeField]
    private KnifeDamage knifeDamage;

    [SerializeField]
    private ParticleSystem slashEffect;

    // Internal state
    private KnifeState currentState = KnifeState.Idle;
    private float stateTimer = 0f;
    private Vector3 idleLocalPosition;
    private bool targetHit = false;
    private Vector3 stuckPosition;

    // Data for attack movement along a path.
    private Vector3[] attackPoints;
    private float[] segmentLengths;
    private float totalDistance;

    // Reference for converting world positions to local.
    private Transform referenceTransform;

    void Start()
    {
        // Use the parent as reference if available, otherwise self.
        referenceTransform = (transform.parent != null) ? transform.parent : transform;

        // Record the starting idle local position.
        idleLocalPosition = transform.localPosition;

        // Setup the attack path based on the LineRenderer.
        InitializePath();

        // Find knife damage component if not set
        if (knifeDamage == null)
            knifeDamage = GetComponent<KnifeDamage>();

        // Subscribe to damage events
        if (knifeDamage != null)
            knifeDamage.OnTargetHit += HandleTargetHit;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (knifeDamage != null)
            knifeDamage.OnTargetHit -= HandleTargetHit;
    }

    void InitializePath()
    {
        if (pathLineRenderer == null || pathLineRenderer.positionCount < 2)
        {
            Debug.LogError(
                "PathLineRenderer is not assigned or does not contain at least 2 points!"
            );
            return;
        }

        int count = pathLineRenderer.positionCount;
        attackPoints = new Vector3[count];

        // Get the positions from the LineRenderer.
        pathLineRenderer.GetPositions(attackPoints);

        // Convert points to world space if needed then to local space relative to referenceTransform.
        for (int i = 0; i < count; i++)
        {
            Vector3 worldPoint;
            if (pathLineRenderer.useWorldSpace)
            {
                worldPoint = attackPoints[i];
            }
            else
            {
                // If the LineRenderer is not in world space, convert its local point to world space.
                worldPoint = pathLineRenderer.transform.TransformPoint(attackPoints[i]);
            }
            // Convert the world space point to the local space of the reference transform.
            attackPoints[i] = referenceTransform.InverseTransformPoint(worldPoint);
        }

        // Compute the lengths of each segment and the total path length.
        segmentLengths = new float[count - 1];
        totalDistance = 0f;
        for (int i = 0; i < count - 1; i++)
        {
            float segLen = Vector3.Distance(attackPoints[i], attackPoints[i + 1]);
            segmentLengths[i] = segLen;
            totalDistance += segLen;
        }
    }

    // Handle hit event from KnifeDamage
    public void HandleTargetHit(Vector3 hitPosition)
    {
        if (currentState == KnifeState.Attack)
        {
            targetHit = true;
            stuckPosition = transform.localPosition;
            currentState = KnifeState.Stuck;
            stateTimer = 0f;

            // Play slash effect only when a target is hit
            if (slashEffect != null)
                slashEffect.Play();
        }
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case KnifeState.Idle:
                IdleUpdate();
                break;
            case KnifeState.Prepare:
                PrepareUpdate();
                break;
            case KnifeState.Attack:
                AttackUpdate();
                break;
            case KnifeState.Stuck:
                StuckUpdate();
                break;
            case KnifeState.BackToIdle:
                BackToIdleUpdate();
                break;
        }
    }

    void IdleUpdate()
    {
        // Apply a slight oscillating offset to the idle local position.
        Vector3 offset =
            new Vector3(Mathf.Sin(Time.time * 1.0f), Mathf.Cos(Time.time * 1.2f), 0f)
            * idleMovementAmplitude;

        transform.localPosition = idleLocalPosition + offset;

        // Transition to Prepare state when left mouse button is held.
        if (Input.GetMouseButton(0))
        {
            currentState = KnifeState.Prepare;
            stateTimer = 0f;
        }
    }

    void PrepareUpdate()
    {
        // Move from the current position to the first point of the attack path.
        if (attackPoints != null && attackPoints.Length > 0)
        {
            float t = Mathf.Clamp01(stateTimer / prepareDuration);
            float easedT = _prepareCurve.Evaluate(t);

            // Use the current position as start position instead of always using idleLocalPosition
            // This ensures smooth transition from stuck state
            Vector3 startPosition = transform.localPosition;
            transform.localPosition = Vector3.Lerp(startPosition, attackPoints[0], easedT);
        }

        // When the left mouse button is released, transition to Attack state.
        if (!Input.GetMouseButton(0))
        {
            currentState = KnifeState.Attack;
            stateTimer = 0f;
            targetHit = false;

            // Enable damage collider at the start of attack
            if (knifeDamage != null)
                knifeDamage.EnableDamage();
        }
    }

    void AttackUpdate()
    {
        if (attackPoints != null && attackPoints.Length >= 2)
        {
            float t = Mathf.Clamp01(stateTimer / attackDuration);
            float easedT = _attackCurve.Evaluate(t);

            // Determine the target distance along the entire path.
            float targetDistance = easedT * totalDistance;

            // Identify which segment this distance is in.
            int segmentIndex = 0;
            float accumulatedDistance = 0f;
            while (
                segmentIndex < segmentLengths.Length
                && (accumulatedDistance + segmentLengths[segmentIndex]) < targetDistance
            )
            {
                accumulatedDistance += segmentLengths[segmentIndex];
                segmentIndex++;
            }

            if (segmentIndex >= segmentLengths.Length)
                segmentIndex = segmentLengths.Length - 1;

            float segmentFraction =
                (segmentLengths[segmentIndex] > 0f)
                    ? (targetDistance - accumulatedDistance) / segmentLengths[segmentIndex]
                    : 0f;

            // Interpolate along the segment.
            Vector3 attackLocalPosition = Vector3.Lerp(
                attackPoints[segmentIndex],
                attackPoints[segmentIndex + 1],
                segmentFraction
            );
            transform.localPosition = attackLocalPosition;
        }

        // After finishing the attack movement, transition to BackToIdle if no target was hit
        if (stateTimer >= attackDuration && !targetHit)
        {
            currentState = KnifeState.BackToIdle;
            stateTimer = 0f;

            // Disable damage collider at the end of attack
            if (knifeDamage != null)
                knifeDamage.DisableDamage();
        }
    }

    void StuckUpdate()
    {
        // Keep the knife in the stuck position
        transform.localPosition = stuckPosition;

        // After the stuck duration, check for next state
        if (stateTimer >= stuckDuration)
        {
            // If mouse button is held, go to prepare state for another attack
            if (Input.GetMouseButton(0))
            {
                // Directly set position to the stuck position before transitioning
                // This prevents any intermediate positions during state change
                transform.localPosition = stuckPosition;

                currentState = KnifeState.Prepare;
                stateTimer = 0f;

                // Disable damage collider
                if (knifeDamage != null)
                    knifeDamage.DisableDamage();
            }
            // Otherwise return to idle
            else
            {
                currentState = KnifeState.BackToIdle;
                stateTimer = 0f;

                // Disable damage collider
                if (knifeDamage != null)
                    knifeDamage.DisableDamage();
            }
        }
    }

    void BackToIdleUpdate()
    {
        if (attackPoints != null && attackPoints.Length >= 1)
        {
            // Move back from the end of the attack path (or stuck position) to the idle local position
            float t = Mathf.Clamp01(stateTimer / backDuration);
            float easedT = _releaseCurve.Evaluate(t);

            Vector3 startPosition = targetHit
                ? stuckPosition
                : attackPoints[attackPoints.Length - 1];
            transform.localPosition = Vector3.Lerp(startPosition, idleLocalPosition, easedT);
        }

        if (stateTimer >= backDuration)
        {
            currentState = KnifeState.Idle;
            stateTimer = 0f;
            targetHit = false;
        }
    }
}
