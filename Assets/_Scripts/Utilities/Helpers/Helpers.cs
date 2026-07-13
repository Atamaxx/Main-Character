using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// A static class for general helpful methods
/// </summary>
public static class Helpers 
{
    /// <summary>
    /// Destroy all child objects of this transform.
    /// <code>
    /// transform.DestroyChildren();
    /// </code>
    /// </summary>
    public static void DestroyChildren(this Transform t) {
        foreach (Transform child in t) UnityEngine.Object.Destroy(child.gameObject);
    }


    /// <summary>
    /// Fill a list with null values up to the specified count.
    /// <code>
    /// myList.FillWithNulls(count);
    /// </code>
    /// </summary>
    public static void FillWithNulls<T>(this List<T> list, int count) where T : class
    {
        for (int i = 0; i < count; i++)
        {
            list.Add(null);
        }
    }

    public static void ChangeVector4Parameter(this VisualEffect vfx, string parameterName, Vector4 value)
    {
        vfx.SetVector4(parameterName, value);
    }

    public static void ChangeVector4Parameter(this VisualEffect vfx, int parameterId, Vector4 value)
    {
        vfx.SetVector4(parameterId, value);
    }


    /// <summary>
    /// Executes an action on the next frame.
    /// </summary>
    /// <param name="monoBehaviour">A MonoBehaviour to run the coroutine.</param>
    /// <param name="action">The action to execute.</param>
    public static void ExecuteNextFrame(this MonoBehaviour monoBehaviour, Action action) {
        monoBehaviour.StartCoroutine(ExecuteAfterFramesCoroutine(action, 1));
    }

    /// <summary>
    /// Executes an action after a specified number of frames.
    /// </summary>
    /// <param name="monoBehaviour">A MonoBehaviour to run the coroutine.</param>
    /// <param name="frameCount">The number of frames to wait before executing the action.</param>
    /// <param name="action">The action to execute.</param>
    public static void ExecuteAfterFrames(this MonoBehaviour monoBehaviour, int frameCount, Action action) {
        monoBehaviour.StartCoroutine(ExecuteAfterFramesCoroutine(action, frameCount));
    }

    private static IEnumerator ExecuteAfterFramesCoroutine(Action action, int frameCount) {
        for (int i = 0; i < frameCount; i++) {
            yield return null; // Wait one frame
        }
        action?.Invoke();
    }
}
