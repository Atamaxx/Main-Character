using UnityEngine;

using System.Collections.Generic;

public class Info : MonoBehaviour
{
    /* List of functions
     * FindChildWithTag
     * FindChildrenWithTag
    */
    public static LayerMask PlayerLayer = LayerMask.GetMask("Player");
    public static LayerMask PlatformLayer = LayerMask.GetMask("Platforms");
    public static string PlayerTag = "Player";



    public static GameObject FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }

            // Recursively search through child GameObjects
            GameObject foundChild = FindChildWithTag(child, tag);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null; // No child with the specified tag found
    }


    public static List<GameObject> FindChildrenWithTag(Transform parent, string tag)
    {
        List<GameObject> childrenWithTag = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                childrenWithTag.Add(child.gameObject);
            }

            // Recursively search through child GameObjects
            List<GameObject> foundChildren = FindChildrenWithTag(child, tag);
            if (foundChildren.Count > 0)
            {
                childrenWithTag.AddRange(foundChildren);
            }
        }

        return childrenWithTag;
    }
}