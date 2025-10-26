using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 実行環境に応じてGameObjectを削除するスクリプト。
/// </summary>
[DisallowMultipleComponent]
public class RemoveOnBuildOrEditor : MonoBehaviour
{
    [Header("削除条件")]
    [Tooltip("エディタ上で実行中（Play Mode）のときに削除する")]
    public bool removeInEditor = false;

    [Tooltip("ビルド後（実行時）に削除する")]
    public bool removeInBuild = false;

    private void Awake()
    {
#if UNITY_EDITOR
        // エディタ上で再生中なら
        if (EditorApplication.isPlaying && removeInEditor)
        {
            DestroyImmediate(gameObject);
            return;
        }
#else
        // ビルド後の実行環境なら
        if (removeInBuild)
        {
            Destroy(gameObject);
            return;
        }
#endif
    }
}
