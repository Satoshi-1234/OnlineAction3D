using UnityEngine;
/**************************************************
 * エディタ：〇
 * ビルド　：×
 *************************************************/
public class DestroyBuildObject : MonoBehaviour
{
#if UNITY_EDITOR
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}
