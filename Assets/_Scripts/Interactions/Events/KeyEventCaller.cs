using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KeyEventMapping
{
    [Tooltip("Key to listen for.")]
    public KeyCode key;

    [Tooltip("Event to invoke when the key is pressed.")]
    public UnityEvent unityEvent;
}

public class KeyEventCaller : MonoBehaviour
{
    [Header("Key Event Mappings")]
    [Tooltip("Assign keys and their corresponding events.")]
    public List<KeyEventMapping> keyEvents = new List<KeyEventMapping>();

    private void Update()
    {
        // Check each mapping to see if its key is pressed
        foreach (KeyEventMapping mapping in keyEvents)
        {
            if (Input.GetKeyDown(mapping.key))
            {
                mapping.unityEvent.Invoke();
            }
        }
    }
}
