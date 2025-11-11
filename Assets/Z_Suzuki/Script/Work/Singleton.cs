using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindAnyObjectByType(typeof(T));
                if (_instance == null)
                {
                    SetupInstance();
                }
            }

            return _instance;
        }
    }


    //　継承先でAwakeで行う処理を追加する場合は、この関数をオーバーライドして定義する
    protected virtual void DoAwake() { }


    private static T _instance;


    private void Awake()
    {
        RemoveDuplicates();
        DoAwake();
    }


    private static void SetupInstance()
    {
        GameObject gameObj = new GameObject();
        gameObj.name = typeof(T).Name;
        _instance = gameObj.AddComponent<T>();
        DontDestroyOnLoad(gameObj);
    }

    private void RemoveDuplicates()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
