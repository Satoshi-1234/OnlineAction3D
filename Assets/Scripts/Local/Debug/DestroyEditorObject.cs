using UnityEngine;
/**************************************************
 * �G�f�B�^�F�~
 * �r���h�@�F�Z
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
