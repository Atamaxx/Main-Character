using UnityEngine;

public class Point_Hey : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 5f;          // Radius to detect the player
    public float detectionRadiusOnDetected = 15f;          // Radius to detect the player
    public LayerMask playerLayer;               // Layer where the player resides

    [Header("Movement Settings")]
    public float moveSpeed = 2f;                // Speed at which the enemy moves
    public float jumpForce = 5f;                // Force applied when jumping
    public LayerMask groundLayer;               // Layer representing the ground

    [Header("Ground Check Settings")]
    public Transform groundCheck;               // Reference to the ground check object
    public float groundCheckRadius = 0.1f;      // Radius for ground checking

    [Header("Gap Detection Settings")]
    public float gapCheckDistance = 1f;         // Distance to check for gaps

    private Rigidbody2D rb;
    private Transform playerTransform;
    private bool isGrounded;
    private Vector2 movementDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find the player by tag. Ensure the player has the "Player" tag.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Please ensure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        DetectPlayerAndAct();
    }

    /// <summary>
    /// Detects the player within the detection radius and moves/jumps accordingly.
    /// </summary>
    void DetectPlayerAndAct()
    {
        if (playerTransform == null)
            return;

        // Check if the player is within the detection radius
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRadius)
        {
            // Determine movement direction away from the player
            float directionX = transform.position.x < playerTransform.position.x ? -1 : 1;
            movementDirection = new Vector2(directionX, 0);

            // Check if the enemy is grounded
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (isGrounded)
            {
                // Check for gaps ahead
                bool isGap = CheckForGap(directionX);
                if (isGap)
                {
                    Jump();
                }
                else
                {
                    Move();
                }
            }
        }
        else
        {
            // Player not detected: Stop movement
            detectionRadius = detectionRadiusOnDetected;
            StopMovement();
        }
    }

    /// <summary>
    /// Moves the enemy in the movement direction.
    /// </summary>
    void Move()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = movementDirection.x * moveSpeed;
        rb.linearVelocity = velocity;

        // Optional: Flip the sprite based on movement direction
        if (movementDirection.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(movementDirection.x), 1, 1);
    }

    /// <summary>
    /// Makes the enemy jump.
    /// </summary>
    void Jump()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Stops the enemy's horizontal movement.
    /// </summary>
    void StopMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0;
        rb.linearVelocity = velocity;

        // Optional: Reset sprite orientation if needed
        // This line ensures the enemy faces forward when idle.
        // Modify as necessary based on your sprite's default orientation.
        // Example: transform.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Checks if there's ground ahead to determine if there's a gap.
    /// </summary>
    /// <param name="directionX">Direction to check (1 for right, -1 for left)</param>
    /// <returns>True if there's a gap, otherwise false.</returns>
    bool CheckForGap(float directionX)
    {
        Vector2 origin = groundCheck.position;
        Vector2 direction = Vector2.down;
        Vector2 rayOrigin = origin + Vector2.right * directionX * gapCheckDistance;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, groundCheckRadius, groundLayer);
        // Debug line to visualize the raycast
        Debug.DrawRay(rayOrigin, direction * groundCheckRadius, Color.red, 1f);

        return hit.collider == null;
    }

    /// <summary>
    /// Visualize the detection radius and ground check radius in the editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw ground check radius
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Optionally, draw gap check ray
        if (groundCheck != null)
        {
            // Example direction (right)
            Vector2 direction = Vector2.right;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheck.position + Vector3.right * gapCheckDistance, groundCheck.position + Vector3.right * gapCheckDistance + Vector3.down * groundCheckRadius);
        }
    }
}
