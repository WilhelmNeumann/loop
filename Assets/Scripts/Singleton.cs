using UnityEngine;

/// <summary>
/// Generic Singleton base class for MonoBehaviour singletons.
/// Usage: public class MyManager : Singleton<MyManager> {}
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// Access to the singleton instance
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// On destroy, mark the application as quitting so we don't recreate instances.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _applicationIsQuitting = true;
        }
    }

    /// <summary>
    /// Reset application quitting flag when loading new domain
    /// (mainly useful in editor)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnLoad()
    {
        _applicationIsQuitting = false;
        _instance = null;
    }
}