using UnityEngine;
using UnityEngine.Events;

public class TriggerEventHandler : MonoBehaviour
{
    // Define a UnityEvent that can be assigned in the Inspector
    [SerializeField]
    private UnityEvent onTriggerEnter2DEvent;

    // Optionally, define a tag to filter which objects can trigger the event
    [SerializeField]
    private string triggerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the entering object has the specified tag
        if (other.CompareTag(triggerTag))
        {
            // Invoke the UnityEvent
            onTriggerEnter2DEvent.Invoke();
        }
    }
}
