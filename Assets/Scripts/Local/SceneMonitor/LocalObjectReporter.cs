using System.Collections;
using UnityEngine;

// Addressablesでロードする全てのローカルオブジェクトにアタッチ
public class LocalObjectReporter : MonoBehaviour
{
    // Startをコルーチンにできる
    IEnumerator Start()
    {
        // -------------------------------------------------
        // ▼ 1. このオブジェクト固有の初期化処理
        // (もし何もなければ、このセクションは不要)

        Debug.Log($"[LocalReporter] {gameObject.name} の初期化（非同期）を開始...");

        // 例：重い処理や、別のアセットロードをシミュレート
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"[LocalReporter] {gameObject.name} の初期化が完了。");


        //    即座に報告せず、同じフレームの最後に報告することで、
        //    他のStart()が実行される余地を残す
        yield return new WaitForEndOfFrame();

        // シーン上のSceneManagerBaseインスタンスを探して報告
        if (SceneManagerBase.Instance != null)
        {
            SceneManagerBase.Instance.ReportLocalObjectReady(this.gameObject);
        }
        else
        {
            Debug.LogError($"[LocalReporter] SceneManagerBase.Instance が見つかりません！ 報告に失敗しました。");
        }
    }
}