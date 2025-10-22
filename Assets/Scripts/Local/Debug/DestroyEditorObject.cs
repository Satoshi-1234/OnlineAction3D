using UnityEngine;
/**************************************************
 * エディタ：×
 * ビルド　：〇
 *************************************************/
public class DestroyEditorObject : MonoBehaviour
{
#if !UNITY_EDITOR
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}
