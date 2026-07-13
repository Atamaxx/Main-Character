using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class MethodInvoker : MonoBehaviour
{
    // Drag objects here or leave empty to find all in scene
    public List<GameObject> targetObjects = new List<GameObject>();

    // Method name to search for and invoke
    public string methodName = "OnResumed";

    [ContextMenu("Invoke Method")]
    public void InvokeMethodOnTargets()
    {
        List<GameObject> objectsToProcess = targetObjects;

        int invokedCount = 0;

        foreach (GameObject obj in objectsToProcess)
        {
            Component[] components = obj.GetComponents<MonoBehaviour>();

            foreach (Component component in components)
            {
                MethodInfo method = component.GetType().GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.Instance);

                if (method != null && method.GetParameters().Length == 0)
                {
                    method.Invoke(component, null);
                    invokedCount++;
                }
            }
        }
    }
}