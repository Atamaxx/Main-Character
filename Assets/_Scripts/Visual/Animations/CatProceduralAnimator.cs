using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCreatureController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float followSpeed = 3f;
    public float runThreshold = 5f;
    public float runSpeedMultiplier = 2f;
    public float maxSlopeAngle = 45f;

    [Header("Body Parts")]
    public LineRenderer bodyLine;
    public LineRenderer[] legs = new LineRenderer[4];
    public LineRenderer neckLine;
    public LineRenderer headLine;
    public LineRenderer tailLine;

    [Header("Animation Settings")]
    public float stepDistance = 0.5f;
    public float legLength = 1f;
    public float bodyHeight = 1.2f;
    public float neckLength = 0.8f;
    public float headSize = 0.5f;
    public float tailLength = 1.2f;
    public float walkCycleSpeed = 2f;
    public float runCycleMultiplier = 1.8f;
    public float bobAmount = 0.2f;
    public float tailWagSpeed = 3f;
    public float jumpHeight = 3f;
    public float jumpCooldown = 1f;
    public float turnSmoothing = 5f;
    public float groundCheckDistance = 5f;

    [Header("Physics Settings")]
    public float gravity = 20f;
    public float groundAdhesion = 5f;
    public LayerMask groundLayer;
    public bool showDebugRays = true;
    public float bodyTiltSmoothing = 3f;

    // Internal state variables
    private Vector3 velocity;
    private Vector3 currentPosition;
    private Vector3[] legPositions = new Vector3[4];
    private float[] legPhases = new float[4];
    private bool[] legIsMoving = new bool[4];
    private float[] legGroundHeights = new float[4];
    private float bodyAngle = 0f;
    private float bodyTilt = 0f;
    private float targetBodyTilt = 0f;
    private float tailWag = 0f;
    private bool isGrounded = true;
    private float jumpCooldownTimer = 0f;
    private float moveSpeed;
    private float groundLevel = 0f;
    private float lastGroundY = 0f;
    private Vector3 groundNormal = Vector3.up;
    private float currentAnimationSpeed = 0f;
    private float animationAcceleration = 2f;
    private float horizontalInput = 0f;
    private float smoothedHeadTrackSpeed = 3f;
    private Vector3 currentHeadDirection;

    // VFX
    private List<GameObject> footstepVFX = new List<GameObject>();
    private Dictionary<int, float> lastFootstepTime = new Dictionary<int, float>();

    void Start()
    {
        currentPosition = transform.position;

        // Initialize leg phases with offset for natural movement
        legPhases[0] = 0f;          // Front left
        legPhases[1] = 0.5f;        // Front right
        legPhases[2] = 0.25f;       // Back left
        legPhases[3] = 0.75f;       // Back right

        // Set initial leg positions and ground heights
        for (int i = 0; i < 4; i++)
        {
            legPositions[i] = CalculateLegPosition(i, 0f);
            legGroundHeights[i] = 0f;
            lastFootstepTime[i] = 0f;
        }

        currentHeadDirection = new Vector3(1, 0, 0);

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        DetectGround();
        UpdateMovement();
        UpdateAnimationState();
        UpdateBodyShape();
        UpdateLegs();
        UpdateHeadAndNeck();
        UpdateTail();
    }


    void DetectGround()
    {
        // Cast rays downward to find ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            groundLevel = hit.point.y;
            groundNormal = hit.normal;
            lastGroundY = groundLevel;

            // Calculate ground angle for body tilt
            float groundAngle = Vector2.SignedAngle(Vector2.up, groundNormal);
            targetBodyTilt = Mathf.Clamp(groundAngle, -maxSlopeAngle, maxSlopeAngle);
        }
        else
        {
            // If no ground directly below, use last known ground height
            groundLevel = lastGroundY;
            groundNormal = Vector3.up;
            targetBodyTilt = 0f;
        }

        // Update body tilt smoothly
        bodyTilt = Mathf.LerpAngle(bodyTilt, targetBodyTilt, Time.deltaTime * bodyTiltSmoothing);

        // Check if we're grounded by raycasting from the body
        isGrounded = false;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector3.down, bodyHeight + 0.1f, groundLayer);
        if (groundHit.collider != null)
        {
            isGrounded = true;

            // If on the ground but above, push down with ground adhesion
            if (transform.position.y > groundLevel + bodyHeight)
            {
                velocity.y -= groundAdhesion * Time.deltaTime;
            }
        }

        // Detect ground beneath each leg
        for (int i = 0; i < 4; i++)
        {
            Vector3 legBase = CalculateLegBase(i);
            RaycastHit2D legHit = Physics2D.Raycast(legBase + Vector3.up * 0.5f, Vector3.down, legLength + 1f, groundLayer);

            if (legHit.collider != null)
            {
                legGroundHeights[i] = legHit.point.y;
            }
            else
            {
                // If no ground found, use the main ground level
                legGroundHeights[i] = groundLevel;
            }

            if (showDebugRays)
            {
                Debug.DrawRay(legBase + Vector3.up * 0.5f, Vector3.down * (legLength + 1f), legHit.collider != null ? Color.green : Color.red);
            }
        }
    }

    void UpdateMovement()
    {
        if (target == null) return;

        // Calculate distance to target for behavior decision
        float distanceToTarget = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(target.position.x, 0));

        // Update jump cooldown
        if (jumpCooldownTimer > 0)
            jumpCooldownTimer -= Time.deltaTime;

        // Check if we should jump
        if (isGrounded && jumpCooldownTimer <= 0 && ShouldJump())
        {
            StartCoroutine(Jump());
            jumpCooldownTimer = jumpCooldown;
        }

        // Determine movement speed based on distance
        moveSpeed = followSpeed;
        if (distanceToTarget > runThreshold)
            moveSpeed *= runSpeedMultiplier;

        // Calculate horizontal input
        horizontalInput = Mathf.Clamp(target.position.x - transform.position.x, -1f, 1f);
        if (Mathf.Abs(horizontalInput) < 0.1f)
            horizontalInput = 0;

        // Apply gravity when not grounded
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            // Reset vertical velocity when grounded
            velocity.y = 0;

            // Apply horizontal movement
            velocity.x = Mathf.Lerp(velocity.x, horizontalInput * moveSpeed, Time.deltaTime * 8f);
        }

        // Apply velocity
        currentPosition += velocity * Time.deltaTime;

        // Adjust height when grounded to follow terrain
        if (isGrounded)
        {
            currentPosition.y = groundLevel + bodyHeight;
        }

        // Update transform
        transform.position = currentPosition;

        // Update body angle based on movement direction
        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            bodyAngle = Mathf.LerpAngle(bodyAngle, targetAngle, Time.deltaTime * turnSmoothing);

            // Flip based on movement direction
            transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
        }
    }

    bool ShouldJump()
    {
        // Direct line to target
        Vector3 dirToTarget = target.position - transform.position;

        // Check if target is higher than us by a significant amount
        bool targetIsHigher = target.position.y > transform.position.y + 0.5f;
        float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
        bool withinJumpRange = horizontalDistance < legLength * 2.5f;

        // Check if there's an obstacle in front of us
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = new Vector3(transform.localScale.x, 0, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, legLength * 2.5f, groundLayer);

        // Also check for gaps in the ground
        Vector3 groundRayStart = transform.position + rayDirection * legLength * 1.2f;
        RaycastHit2D groundHit = Physics2D.Raycast(groundRayStart, Vector2.down, groundCheckDistance, groundLayer);
        bool gapAhead = groundHit.collider == null;

        if (showDebugRays)
        {
            Debug.DrawRay(rayStart, rayDirection * legLength * 2.5f, hit.collider != null ? Color.red : Color.yellow);
            Debug.DrawRay(groundRayStart, Vector2.down * groundCheckDistance, groundHit.collider != null ? Color.green : Color.red);
        }

        // Jump if target is higher and within range, or if there's an obstacle, but not if there's a gap
        return ((targetIsHigher && withinJumpRange) || (hit.collider != null)) && !gapAhead;
    }

    IEnumerator Jump()
    {
        isGrounded = false;
        float jumpTime = 0.5f;
        float elapsedTime = 0;
        Vector3 startPos = currentPosition;

        // Calculate jump trajectory
        Vector3 peakPos = currentPosition + new Vector3(transform.localScale.x * jumpHeight * 0.5f, jumpHeight, 0);

        // Use target position as landing spot if appropriate
        Vector3 targetPos = target.position;
        if (target.position.y < currentPosition.y || Vector3.Distance(target.position, currentPosition) > jumpHeight * 2)
        {
            // If target isn't viable, jump forward
            targetPos = currentPosition + new Vector3(transform.localScale.x * jumpHeight, 0, 0);
        }

        // Start with upward velocity for natural jump
        velocity = new Vector3(transform.localScale.x * moveSpeed, jumpHeight * 2, 0);

        // FIXED: Prepare legs for jump by resetting phases to tucked position
        for (int i = 0; i < 4; i++)
        {
            // Set all legs to roughly the same phase for a coordinated jump
            legPhases[i] = 0.7f; // Leg lift phase
        }

        while (elapsedTime < jumpTime)
        {
            elapsedTime += Time.deltaTime;

            // Apply gravity manually during jump for more control
            velocity.y -= gravity * Time.deltaTime * 0.5f;
            currentPosition += velocity * Time.deltaTime;

            // Check for landing
            RaycastHit2D groundHit = Physics2D.Raycast(currentPosition, Vector3.down, bodyHeight + 0.1f, groundLayer);
            if (groundHit.collider != null && velocity.y < 0)
            {
                // We've hit ground while moving downward, so land
                groundLevel = groundHit.point.y;
                lastGroundY = groundLevel;
                currentPosition.y = groundLevel + bodyHeight;
                isGrounded = true;

                // FIXED: On landing, set legs to contact phase
                for (int i = 0; i < 4; i++)
                {
                    // Set all legs to roughly the ground contact phase
                    legPhases[i] = 0.0f;
                }

                // Create landing effect
                SpawnFootstepVFX(currentPosition + Vector3.down * bodyHeight);

                break;
            }

            yield return null;
        }

        // Ensure we're grounded at the end
        isGrounded = true;
        velocity.y = 0;
    }
    void UpdateAnimationState()
    {
        // Smoothly adjust animation speed based on movement
        float targetAnimSpeed = Mathf.Abs(velocity.x) * walkCycleSpeed;

        // FIXED: Smoother transition to running animation
        float runThreshold = followSpeed * 1.3f;
        if (Mathf.Abs(velocity.x) > runThreshold)
        {
            // Running animation speed with smoother transition
            float runBlend = Mathf.Clamp01((Mathf.Abs(velocity.x) - runThreshold) / (followSpeed * 0.5f));
            targetAnimSpeed = Mathf.Lerp(targetAnimSpeed, targetAnimSpeed * runCycleMultiplier, runBlend);
        }

        // Lerp the animation speed
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetAnimSpeed, Time.deltaTime * animationAcceleration);

        // FIXED: Move leg phase update to separate function
        UpdateLegPhasesForMovement();

        // Rest of animation state code...
        // Body bobbing - only when grounded and moving
        float bobOffset = 0;
        if (isGrounded && Mathf.Abs(velocity.x) > 0.1f)
        {
            bobOffset = Mathf.Sin(Time.time * currentAnimationSpeed * 2) * bobAmount * (Mathf.Abs(velocity.x) / followSpeed);
        }

        // Only apply bob if grounded
        if (isGrounded)
        {
            currentPosition.y = groundLevel + bodyHeight + bobOffset;
        }

        // Tail wagging - intensity based on speed and state
        float wagIntensity = 30f;
        float wagSpeed = tailWagSpeed;

        if (Mathf.Abs(velocity.x) > followSpeed)
        {
            // Running tail animation
            wagIntensity = 40f;
            wagSpeed = tailWagSpeed * 1.5f;
        }
        else if (!isGrounded)
        {
            // Jumping tail animation
            wagIntensity = 15f;
            wagSpeed = tailWagSpeed * 0.7f;
        }

        tailWag = Mathf.Sin(Time.time * wagSpeed) * wagIntensity;
    }


    void UpdateBodyShape()
    {
        if (bodyLine != null)
        {
            // Apply body tilt based on ground normal
            Quaternion tiltRotation = Quaternion.Euler(0, 0, bodyTilt);

            // Make body longer when running
            float stretchFactor = Mathf.Lerp(1f, 1.3f, Mathf.Abs(velocity.x) / (followSpeed * runSpeedMultiplier));

            // Configure body line with tilt
            bodyLine.positionCount = 2;
            bodyLine.SetPosition(0, transform.position + tiltRotation * new Vector3(-0.8f * stretchFactor, 0, 0));
            bodyLine.SetPosition(1, transform.position + tiltRotation * new Vector3(0.8f * stretchFactor, 0, 0));
        }
    }

    Vector3 CalculateLegBase(int legIndex)
    {
        // Front or back legs
        bool isFrontLeg = legIndex < 2;

        // Apply body tilt
        Quaternion tiltRotation = Quaternion.Euler(0, 0, bodyTilt);

        // Make positions adjust with running stretch
        float stretchFactor = Mathf.Lerp(1f, 1.3f, Mathf.Abs(velocity.x) / (followSpeed * runSpeedMultiplier));
        float xOffset = isFrontLeg ? 0.5f * stretchFactor : -0.5f * stretchFactor;

        // Return the base hip position
        return transform.position + tiltRotation * new Vector3(xOffset, 0, 0);
    }

    Vector3 CalculateLegPosition(int legIndex, float overridePhase = -1)
    {
        float phase = overridePhase >= 0 ? overridePhase : legPhases[legIndex];
        float footDownPosition = phase < 0.5f ? 1 : 0;

        // Get leg base (hip) position
        Vector3 hipPosition = CalculateLegBase(legIndex);

        // Left or right legs
        bool isLeftLeg = legIndex % 2 == 0;

        // Apply body tilt to leg direction
        Quaternion tiltRotation = Quaternion.Euler(0, 0, bodyTilt);
        Vector3 legDirection = tiltRotation * new Vector3(isLeftLeg ? -0.6f : 0.6f, -1, 0).normalized;

        // Get ground height for this leg
        float legGroundY = legGroundHeights[legIndex];

        // Calculate ground position 
        Vector3 groundPosition = hipPosition + legDirection * legLength;
        groundPosition.y = legGroundY;

        // FIXED: Improved ground check for foot placement
        // Cast ray to ensure foot never goes below ground
        RaycastHit2D footHit = Physics2D.Raycast(hipPosition, Vector3.down, legLength * 1.5f, groundLayer);
        if (footHit.collider != null)
        {
            // Ensure foot never goes below detected ground
            groundPosition.y = Mathf.Max(groundPosition.y, footHit.point.y);
        }

        // If foot is lifting, calculate arc
        if (footDownPosition < 0.5f)
        {
            // Foot is on the ground
            return groundPosition;
        }
        else
        {
            // FIXED: Improved step arc with proper ground clearance
            float liftProgress = (phase - 0.5f) * 2f;

            // Calculate forward movement - more when running, with smoother transition
            float speedRatio = Mathf.Clamp01(Mathf.Abs(velocity.x) / followSpeed);
            float strideLength = Mathf.Lerp(stepDistance, stepDistance * 1.8f, speedRatio);

            // FIXED: Smoother arc calculation with sin function
            float horizontalOffset = Mathf.Sin(liftProgress * Mathf.PI) * strideLength * Mathf.Sign(velocity.x);
            if (Mathf.Abs(velocity.x) < 0.1f)
                horizontalOffset = 0;

            // FIXED: Better vertical arc that ensures foot clears ground
            // Higher arc for running
            float arcHeight = legLength * 0.4f * Mathf.Lerp(1f, 1.5f, speedRatio);
            float verticalOffset = Mathf.Sin(liftProgress * Mathf.PI) * arcHeight;

            return groundPosition + new Vector3(horizontalOffset, verticalOffset, 0);
        }
    }

    void UpdateLegs()
    {
        // FIXED: Better leg behavior during jumps
        bool shouldFreezeLegs = !isGrounded && velocity.y > 0;

        for (int i = 0; i < 4; i++)
        {
            if (legs[i] != null)
            {
                // Set leg positions
                Vector3 hipPosition = CalculateLegBase(i);

                // FIXED: Handle legs during jump differently
                Vector3 footPosition;
                if (shouldFreezeLegs)
                {
                    // FIXED: When jumping upward, tuck legs up
                    float tuckAmount = velocity.y * 0.1f;
                    bool isTuckedLeftLeg = i % 2 == 0;
                    Vector3 tuckedDir = new Vector3(isTuckedLeftLeg ? -0.3f : 0.3f, -1, 0).normalized;

                    // Tuck legs closer to body during jump
                    float jumpLegLength = legLength * 0.7f;
                    footPosition = hipPosition + tuckedDir * jumpLegLength;
                }
                else
                {
                    // Normal leg positioning when not jumping upward
                    footPosition = CalculateLegPosition(i);
                }

                // Add slight curve to the leg
                legs[i].positionCount = 3;
                legs[i].SetPosition(0, hipPosition);

                // Middle control point for natural leg bend
                Vector3 midPoint = Vector3.Lerp(hipPosition, footPosition, 0.5f);

                // Add natural bend
                float bendAmount = 0.2f;

                // FIXED: More pronounced bend during jumps
                if (!isGrounded)
                    bendAmount = 0.3f;

                // Direction of bend depends on leg side
                bool isLeftLeg = i % 2 == 0;
                Vector3 bendDir = isLeftLeg ? Vector3.left : Vector3.right;

                // Apply bend perpendicular to leg direction
                Vector3 legDir = (footPosition - hipPosition).normalized;
                Vector3 perpDir = new Vector3(-legDir.y, legDir.x, 0);

                midPoint += perpDir * bendAmount * legLength;

                legs[i].SetPosition(1, midPoint);
                legs[i].SetPosition(2, footPosition);
            }
        }
    }

    void UpdateLegPhasesForMovement()
    {
        // Don't update leg phases in air during jump
        if (!isGrounded)
        {
            // FIXED: Freeze leg animation phases during jump
            return;
        }

        // Only animate legs if we're actually moving
        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            // Update leg phases
            for (int i = 0; i < 4; i++)
            {
                float prevPhase = legPhases[i];
                legPhases[i] = (legPhases[i] + Time.deltaTime * currentAnimationSpeed) % 1f;

                // Check if leg just touched the ground (phase crossed from >0.5 to <0.5)
                if (prevPhase > 0.5f && legPhases[i] < 0.5f)
                {
                    // Spawn footstep VFX when leg touches ground
                    Vector3 footPos = CalculateLegPosition(i);
                    if (Time.time - lastFootstepTime[i] > 0.2f) // prevent spamming
                    {
                        SpawnFootstepVFX(footPos);
                        lastFootstepTime[i] = Time.time;
                    }
                }
            }
        }
    }

    void UpdateHeadAndNeck()
    {
        if (neckLine == null || headLine == null) return;

        // Apply body tilt to neck position
        Quaternion tiltRotation = Quaternion.Euler(0, 0, bodyTilt);
        Vector3 neckStart = transform.position + tiltRotation * new Vector3(0.8f, 0.1f, 0);

        // Make head look towards target with smooth interpolation
        Vector3 targetDir = (target.position - neckStart).normalized;

        // FIXED: Reduced head tracking speed for more natural movement
        smoothedHeadTrackSpeed = Mathf.Lerp(1.5f, 3f, Mathf.Abs(velocity.x) / (followSpeed * runSpeedMultiplier));

        // Smoothly interpolate head direction
        currentHeadDirection = Vector3.Slerp(currentHeadDirection, targetDir, Time.deltaTime * smoothedHeadTrackSpeed);

        // Limit head tracking angle
        float angle = Mathf.Atan2(currentHeadDirection.y, currentHeadDirection.x * transform.localScale.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, -45f, 45f);
        Vector3 clampedDir = Quaternion.Euler(0, 0, angle) * new Vector3(transform.localScale.x, 0, 0);

        // FIXED: More subtle and contextual head movement
        float headBobAmount = 0.03f;
        float headBobTime = Time.time * 2f;

        if (!isGrounded)
        {
            // Different head animation during jumps - more focused, less bobbing
            headBobAmount = 0.05f;
            headBobTime = Time.time * 1.5f;
        }
        else if (Mathf.Abs(velocity.x) > followSpeed)
        {
            // Running head animation - consistent forward focus
            headBobAmount = 0.04f;
            headBobTime = Time.time * 4f;
        }
        else if (Mathf.Abs(velocity.x) < 0.1f)
        {
            // Idle head movement - more curious looking around
            headBobAmount = 0.04f;
            headBobTime = Time.time * 1.2f;
        }

        // FIXED: Apply head bob only vertically for more natural look
        Vector3 headBob = new Vector3(0, Mathf.Sin(headBobTime) * headBobAmount, 0);
        Vector3 neckEnd = neckStart + clampedDir * neckLength + headBob;

        // Rest of the head drawing code...
        neckLine.positionCount = 3;
        neckLine.SetPosition(0, neckStart);

        // Neck middle point with natural curve
        Vector3 neckMid = Vector3.Lerp(neckStart, neckEnd, 0.5f);
        neckMid += Vector3.up * 0.1f; // Slight upward curve
        neckLine.SetPosition(1, neckMid);
        neckLine.SetPosition(2, neckEnd);

        // Head shape (triangle)
        headLine.positionCount = 4;
        headLine.SetPosition(0, neckEnd);
        headLine.SetPosition(1, neckEnd + Quaternion.Euler(0, 0, -30) * clampedDir * headSize);
        headLine.SetPosition(2, neckEnd + Quaternion.Euler(0, 0, 30) * clampedDir * headSize);
        headLine.SetPosition(3, neckEnd);

        // Add ear points for extra detail
        if (headLine.positionCount < 6)
            headLine.positionCount = 6;

        // Add ears
        Vector3 earBase = neckEnd + Quaternion.Euler(0, 0, 10) * clampedDir * headSize * 0.6f;
        Vector3 earTip = earBase + Vector3.up * headSize * 0.5f;
        headLine.SetPosition(4, earBase);
        headLine.SetPosition(5, earTip);
    }
    void UpdateTail()
    {
        if (tailLine == null) return;

        // Apply body tilt to tail position
        Quaternion tiltRotation = Quaternion.Euler(0, 0, bodyTilt);
        Vector3 tailBase = transform.position + tiltRotation * new Vector3(-0.8f, 0.1f, 0);

        // Create curved tail with multiple segments
        int segments = 8;
        tailLine.positionCount = segments;

        // Calculate tail state based on movement
        float wagAmplitude = 30f;
        float tailCurlAmount = 0.7f;

        if (!isGrounded)
        {
            // Jumping tail state
            wagAmplitude = 15f;
            tailCurlAmount = 0.9f;
        }
        else if (Mathf.Abs(velocity.x) > followSpeed)
        {
            // Running tail state
            wagAmplitude = 40f;
            tailCurlAmount = 0.5f; // More straight out when running
        }

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            // Make tail wave from side to side
            float waveOffset = Mathf.Sin(t * Mathf.PI * 2 + Time.time * tailWagSpeed) * 0.2f;

            // Add the wag angle that increases toward the tip of the tail
            float currentWagAngle = tailWag * t;

            // Base tail direction is backward with some droop
            Vector3 tailDir = tiltRotation * new Vector3(-1, -0.2f, 0).normalized;

            // Apply wag rotation
            Vector3 wagDirection = Quaternion.Euler(0, 0, currentWagAngle) * tailDir;

            // Add curl to the tail (progressively more curved)
            float curl = Mathf.Pow(t, 1.5f) * tailCurlAmount;
            wagDirection = Quaternion.Euler(0, 0, curl * -40f) * wagDirection;

            // Calculate segment position
            Vector3 segmentPos = tailBase + wagDirection * t * tailLength + new Vector3(0, waveOffset * t, 0);

            tailLine.SetPosition(i, segmentPos);
        }
    }

    void SpawnFootstepVFX(Vector3 position)
    {
        // You can implement footstep VFX here
        // For example:
        /*
        GameObject footstepVfx = Instantiate(footstepPrefab, position, Quaternion.identity);
        footstepVFX.Add(footstepVfx);
        Destroy(footstepVfx, 1.0f);
        */

        // For debug visualization
        Debug.DrawRay(position, Vector3.up * 0.5f, Color.white, 0.2f);
    }

    void OnDrawGizmos()
    {
        if (!showDebugRays) return;

        Gizmos.color = Color.yellow;
        if (target != null)
            Gizmos.DrawWireSphere(target.position, 0.3f);

        // Draw ground level
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + new Vector3(-5, groundLevel, 0),
                         transform.position + new Vector3(5, groundLevel, 0));

        // Draw ground normal
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.down * bodyHeight, groundNormal);
    }
}