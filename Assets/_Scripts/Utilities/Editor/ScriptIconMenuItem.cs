using UnityEngine;
using UnityEditor;

public class ScriptIconMenuItem : MonoBehaviour
{
    const string k_label = "ScriptIcon";

    [MenuItem("Tools/Script Icons/Assign Label")]
    static void AssignScriptItemMenuItem()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            string[] labels = AssetDatabase.GetLabels(obj);
            if (!ArrayUtility.Contains(labels, k_label))
            {
                ArrayUtility.Add(ref labels, k_label);
                AssetDatabase.SetLabels(obj, labels);
            }
        }

    }

    [MenuItem("Tools/Script Icons/Remove Label")]
    static void RemoveScriptItemMenuItem()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            string[] labels = AssetDatabase.GetLabels(obj);

            ArrayUtility.Remove(ref labels, k_label);
            AssetDatabase.SetLabels(obj, labels);
        }
    }
}
