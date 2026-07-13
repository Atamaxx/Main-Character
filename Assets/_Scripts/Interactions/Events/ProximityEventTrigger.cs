using UnityEngine;
using UnityEngine.Events;

public class ProximityEventTrigger : MonoBehaviour
{
    [Header("Objects to Monitor")]
    // Assign the two objects in the inspector.
    public Transform objectA;
    public Transform objectB;

    [Header("Trigger Settings")]
    // The distance threshold for triggering events.
    public float triggerDistance = 5f;

    [Header("Events")]
    // Event to call when objects come close.
    public UnityEvent onClose;
    // Event to call when objects move out of range.
    public UnityEvent onOutOfRange;

    // Tracks whether the objects are currently within the trigger distance.
    private bool isInRange = false;

    void Update()
    {
        // Ensure both objects are assigned.
        if (objectA == null || objectB == null)
        {
            return;
        }

        // Calculate the distance between the two objects.
        float distance = Vector3.Distance(objectA.position, objectB.position);
        bool currentlyInRange = distance <= triggerDistance;

        // If the objects have just come within range, invoke the onClose event.
        if (currentlyInRange && !isInRange)
        {
            onClose?.Invoke();
        }
        // If the objects have just moved out of range, invoke the onOutOfRange event.
        else if (!currentlyInRange && isInRange)
        {
            onOutOfRange?.Invoke();
        }

        // Update the current state.
        isInRange = currentlyInRange;
    }
}
