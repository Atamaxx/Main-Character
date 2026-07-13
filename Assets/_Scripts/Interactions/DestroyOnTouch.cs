using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
    [Header("Layers to Destroy")]
    [SerializeField] private LayerMask layersToDestroy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object's layer is in the specified layer mask
        if ((layersToDestroy.value & (1 << other.gameObject.layer)) != 0)
        {
            Destroy(other.gameObject);
        }
    }
}
