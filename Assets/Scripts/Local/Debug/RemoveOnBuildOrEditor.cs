using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ���s���ɉ�����GameObject���폜����X�N���v�g�B
/// </summary>
[DisallowMultipleComponent]
public class RemoveOnBuildOrEditor : MonoBehaviour
{
    [Header("�폜����")]
    [Tooltip("�G�f�B�^��Ŏ��s���iPlay Mode�j�̂Ƃ��ɍ폜����")]
    public bool removeInEditor = false;

    [Tooltip("�r���h��i���s���j�ɍ폜����")]
    public bool removeInBuild = false;

    private void Awake()
    {
#if UNITY_EDITOR
        // �G�f�B�^��ōĐ����Ȃ�
        if (EditorApplication.isPlaying && removeInEditor)
        {
            DestroyImmediate(gameObject);
            return;
        }
#else
        // �r���h��̎��s���Ȃ�
        if (removeInBuild)
        {
            Destroy(gameObject);
            return;
        }
#endif
    }
}
