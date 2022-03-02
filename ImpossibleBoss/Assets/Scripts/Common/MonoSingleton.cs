using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T mInstance;
    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<T>();
            }
            return mInstance;
        }
    }

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (Instance != this)
        {
            // ÀÚ½ÅÀ» ÆÄ±«
            Destroy(gameObject);
            Logger.LogWarning(string.Format("Test Singleton Awake : {0} is destroyed", Instance));
        }
        Logger.Log(string.Format("Test Singleton Awake {0}", Instance));
    }
}