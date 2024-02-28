using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance
    {
        get
        {
            _instance ??= FindAnyObjectByType<T>() ?? new GameObject(typeof(T).Name + " " + nameof(Singleton<T>)).AddComponent<T>();

            //DontDestroyOnLoad(_instance);

            return _instance;
        }
    }
    private static T _instance;

    private void OnDestroy()
    {
        _instance = null;
    }
}