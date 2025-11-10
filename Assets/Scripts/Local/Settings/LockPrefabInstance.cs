using UnityEngine;

[ExecuteAlways]
public class LockPrefabInstance : MonoBehaviour
{
    void OnEnable()
    {
        // プレハブインスタンスかどうかを確認
        if (gameObject.scene.IsValid())
        {
            // このオブジェクトを編集不可・非表示に設定
            gameObject.hideFlags = HideFlags.NotEditable;
        }
    }
}
