using UnityEngine;
/**************************************************
 * �G�f�B�^�F�Z
 * �r���h�@�F�~
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
