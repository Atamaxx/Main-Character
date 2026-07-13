using UnityEngine;

/// <summary>
/// A static instance is similar to a singleton, but instead of destroying any new
/// instances, it overrides the current instance. This is handy for resetting the state
/// without doing it manually.
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a basic singleton. This will destroy any new
/// versions created, leaving the original instance intact.
/// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        // If an instance already exists and it's not this one, destroy the new object.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }
}

/// <summary>
/// This is a persistent version of the singleton. It survives scene loads, making it perfect
/// for system classes that require stateful, persistent data (e.g., GameManager).
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
