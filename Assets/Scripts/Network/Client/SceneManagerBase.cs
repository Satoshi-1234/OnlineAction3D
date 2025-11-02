using Mirror;
using UnityEngine;
using System.Collections;
public enum GameScene 
{ 
    Debug,
    Title, 
    Home, 
    BattleForest, 
    BattleCastle 
}
public abstract class SceneManagerBase : MonoBehaviour
{
    [Header("シーン設定")]
    [Tooltip("このシーンがどのシーンか")]
    [SerializeField]
    protected GameScene thisScene; // ★ Inspectorで設定

    [Header("シーン遷移設定")]
    public GameScene nextSceneRequest = GameScene.Home;
    public string nextSceneAddress = GameScene.Home.ToString();

    protected PlayerState localPlayerState;

    /// <summary>
    /// 全てのシーンマネージャーで実行される処理のテンプレート
    /// </summary>
    protected virtual IEnumerator Start()
    {

        Debug.Log($"[Client/{thisScene}] Mirrorに準備完了を通知します。");
        NetworkClient.Ready();
        // 1. プレイヤーオブジェクトの準備（シーンによって処理が異なる）
        yield return EnsurePlayerIsReady();

        if (localPlayerState == null)
        {
            Debug.LogError($"[{thisScene}] PlayerStateの準備に失敗しました。処理を中断します。");
            yield break;
        }
        Debug.Log($"[Client/{thisScene}] サーバーにシーン準備完了を通知します。");
        NetworkClient.Send(new ClientSceneReadyRequest { _nowScene = thisScene });

        // 3. シーン固有の初期化処理を呼び出す (派生クラスが実装)
        InitializeScene();
    }

    /// <summary>
    /// プレイヤーオブジェクトを準備する (virtual)
    /// Home以外のシーン (Battleなど) は、既存の DontDestroyOnLoad プレイヤーを待つ
    /// </summary>
    protected virtual IEnumerator EnsurePlayerIsReady()
    {
        while (NetworkClient.localPlayer == null)
        {
            Debug.LogWarning($"[{thisScene}] 既存のPlayerオブジェクトを待機中...");
            yield return null;
        }
        localPlayerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
        Debug.Log($"[{thisScene}] 既存のPlayerStateを取得しました。");
    }

    /// <summary>
    /// シーン固有の初期化（UI有効化など）。派生クラスで必ず実装する (abstract)
    /// </summary>
    protected abstract void InitializeScene();

    /// <summary>
    /// シーン遷移リクエスト (変更なし)
    /// </summary>
    protected void RequestSceneTransition()
    {
        ClientGameManager.Instance.RequestServerSceneChange(nextSceneRequest, nextSceneAddress);
    }
}