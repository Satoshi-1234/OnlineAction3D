using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor; // エディタでのみ使用
#endif
public abstract class SceneManagerBase : MonoBehaviour
{
    // このクラスのシングルトンインスタンス
    public static SceneManagerBase Instance { get; private set; }
    [Header("シーン設定")]
    [Tooltip("このシーンがどのシーンか")]
    [SerializeField]
    protected GameScene thisScene; // ★ Inspectorで設定

    [Header("シーン遷移設定")]
    [SerializeField]
    private LocalSceneSettings sceneSettings;
    private GameScene nextSceneRequest = GameScene.Home;
    private string nextSceneAddress = GameScene.Home.ToString();

    [Header("ローカルオブジェクト設定")]
    [Tooltip("このシーンで非同期にロードするローカルオブジェクト（建物、障害物など）のリスト")]
    [SerializeField]
    private List<AssetReference> localObjectsToLoad; // ★Inspectorで設定

    private int totalObjectsToLoad = 0; // ロードすべき総数
    private int readyObjectCount = 0;   // 準備完了の報告を受けた数

    protected PlayerState localPlayerState;

    /// <summary>
    /// 全てのシーンマネージャーで実行される処理のテンプレート
    /// </summary>
    protected virtual IEnumerator Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"[SceneManagerBase] 警告: 古いInstanceが残っていました。上書きします。");
        }
        // 1. シングルトンとして自身を登録
        Instance = this;

        if(sceneSettings == null)
        {
            Debug.LogError($"[Client/{thisScene}] sceneSettings がnullです");
            new WaitForSeconds(0.1f);
        }

        {
            nextSceneRequest = sceneSettings.nextSceneRequest;
            nextSceneAddress = sceneSettings.nextSceneAddress;
        }
        // -----------------------------------------------
        // ▼ 3. ネットワーク処理の開始フェーズ
        // -----------------------------------------------
        if (NetworkClient.isLoadingScene)
        {
            Debug.LogWarning($"[Client/{thisScene}] NetworkClient.isLoadingScene が true でした。強制的に false にリセットします。");
            NetworkClient.isLoadingScene = false;
        }
        Debug.Log($"[Client/{thisScene}] Mirrorに準備完了を通知します。");
        Debug.Log($"[Client {thisScene}] IsConnected: {NetworkClient.isConnected}, ActiveHost: {NetworkClient.activeHost}");
        
        NetworkClient.Ready();

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
    protected virtual void OnDestroy()
    {
        Debug.Log($"[{thisScene}] Destroyed!");
        if (Instance == this)
        {
            Instance = null;
        }
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

    /// <summary>
    /// LocalObjectReporterから呼び出されるコールバックメソッド
    /// </summary>
    public void ReportLocalObjectReady(GameObject reporter)
    {
        readyObjectCount++;
        Debug.Log($"[SceneManagerBase] 完了報告: {reporter.name} (現在 {readyObjectCount} / {totalObjectsToLoad})");
    }
}