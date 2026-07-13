using UnityEngine;

[CreateAssetMenu(fileName = "CollisionSettings", menuName = "Britney/CollisionSettings")]
public class CollisionSettings : ScriptableObject
{
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundedRadius = 0.25f;

    [Header("Slope Settings")]
    public float slopeCheckDistance = 0.5f;
    public float maxSlopeAngle = 70f;
}
