using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ServerGameManager : NetworkManager
{
    public static ServerGameManager Instance { get; private set; }
    [Header("認証プレハブ")]
    [Tooltip("実行時に生成する認証コンポーネントのプレハブ")]
    public GameObject authenticatorPrefab;
    [Header("キャラクタープレハブ")]
    private GameObject[] characterPrefabs;
    //#if UNITY_SERVER && !UNITY_EDITOR
    public override void Awake()
    {
        base.Awake();
        if (authenticatorPrefab != null)
        {
            // プレハブから認証コンポーネントを生成
            GameObject authInstance = Instantiate(authenticatorPrefab, transform); // NetworkManagerの子として生成

            // 生成したインスタンスからCustomNetworkManagerコンポーネントを取得
            CustomNetworkManager authComponent = authInstance.GetComponent<CustomNetworkManager>();

            if (authComponent != null)
            {
                // NetworkManagerのauthenticatorフィールドに設定
                authenticator = authComponent;
                Debug.Log($"[Server-Success] Authenticator '{authInstance.name}' set it dynamically");
            }
            else
            {
                Debug.LogError("[Server-Error] Authenticator PrefabCustomNetworkManager Not Component");
            }
        }
        else if (authenticator == null) // プレハブが未設定で、かつ手動でも設定されていない場合
        {
            Debug.LogWarning("[Server-Warning] Authenticator Prefab Not Set. No authentication is performed.");
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // プレイヤー生成を許可された接続IDのリスト
    private readonly HashSet<int> connectionsReadyForPlayer = new HashSet<int>();
    public override void OnStartServer()
    {
        base.OnStartServer();
        //Instance = this;
        NetworkServer.RegisterHandler<ClientReadyRequest>(OnClientReady);//廃棄予定
        NetworkServer.RegisterHandler<ClientSceneChangeRequest>(OnClientSceneChange);
        NetworkServer.RegisterHandler<ClientSceneReadyRequest>(OnClientSceneReady);
        LoadCharacterPrefabsAsync();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        NetworkServer.UnregisterHandler<ClientReadyRequest>();
        NetworkServer.UnregisterHandler<ClientSceneChangeRequest>();
        NetworkServer.UnregisterHandler<ClientSceneReadyRequest>();
    }
    /// <summary>
    /// "Character"ラベルを持つ全てのアセットをAddressablesから非同期で読み込む
    /// </summary>
    private async void LoadCharacterPrefabsAsync()
    {
        Debug.Log($"[Server-Addressables] Build Path: {UnityEngine.AddressableAssets.Addressables.BuildPath}");
        Debug.Log($"[Server-Addressables] Runtime Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}");

        var handle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            characterPrefabs = handle.Result.ToArray();
            foreach (var prefab in characterPrefabs)
            {
                if (prefab != null && !spawnPrefabs.Contains(prefab))
                {
                    spawnPrefabs.Add(prefab);
                }
            }
            Debug.Log($"[Server] {characterPrefabs.Length} 体のキャラクタープレハブをAddressablesから読み込みました。");
        }
        else
        {
            Debug.LogError("[Server] Addressablesからのキャラクタープレハブ読み込みに失敗しました。");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // クライアントが切断したら、許可リストから削除する
        connectionsReadyForPlayer.Remove(conn.connectionId);

        // baseの処理を呼び出して、プレイヤーオブジェクトなどを正しくクリーンアップする
        base.OnServerDisconnect(conn);
    }
    // クライアントが準備できたら呼ばれる
    void OnClientReady(NetworkConnectionToClient conn, ClientReadyRequest msg)
    {
        Debug.Log($"[Server] GetReadyRequest from {conn.connectionId}, Phase:{msg._phase}");
        switch (msg._phase)
        {
            case 1:
                // シーン遷移を命令
                conn.Send(new SceneMessage { sceneName = "Home" });
                break;
            case 2:
                if (conn.identity != null)
                {
                    conn.Send(new SceneMessage { sceneName = "BattleStage" });
                }
                break;
        }
    }

    void OnClientSceneChange(NetworkConnectionToClient conn,ClientSceneChangeRequest msg)
    {
        Debug.Log($"[Server] GetClientSceneChangeRequest from {conn.connectionId}, TargetScene:{msg._targetSceneName}");
        conn.Send(new SceneMessage { sceneName = msg._targetSceneName });
    }

    void OnClientSceneReady(NetworkConnectionToClient conn, ClientSceneReadyRequest msg)
    {
        Debug.Log($"[Server-GetRequest] {conn.connectionId} : {msg._nowScene.ToString()} is Ready!");
        switch (msg._nowScene)
        {
            case GameScene.Title:
                break;
            case GameScene.Home:
                break;
            case GameScene.BattleForest:
                break;
            case GameScene.BattleCastle:
                break;
        }
        // ★★★ ここでマッチング参加者全員の準備が整ったかチェックするロジックを実装 ★★★
        // 例:
        // Match currentMatch = FindMatchContainingPlayer(conn);
        // if (currentMatch != null)
        // {
        //     currentMatch.MarkPlayerAsReady(conn.connectionId);
        //     if (currentMatch.AreAllPlayersReady())
        //     {
        //         StartMatchCountdown(currentMatch); // 全員準備完了なら試合開始処理へ
        //     }
        // }

        // 注意: 以前の SetClientReady はここでは呼ばないこと。
        // SetClientReady はプレイヤーオブジェクトの準備完了を意味し、
        // シーンの準備完了とは別です。
        // NetworkServer.SetClientReady(conn); // ← これは不要
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"[Server] OnServerAddPlayer: conn {conn.connectionId} Generated");
        GameObject player = Instantiate(playerPrefab);
        // PlayerConnectionの可視性を所有者のみに限定する
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        identity.visibility = Visibility.ForceHidden; // まず全員から隠す
        NetworkServer.AddPlayerForConnection(conn, player); //AddObserverを行っている
        if (identity.observers.Count > 0) 
            Debug.Log($"[Server] observers Count: {identity.observers.Count}");
    }
    public void SpawnCharacterForPlayer(NetworkConnectionToClient conn, int characterId)
    {
        if (characterId < 0 || characterId >= characterPrefabs.Length)
        {
            Debug.LogError($"[Server-Error] invalid ID: {characterId}");
            return;
        }

        GameObject characterPrefab = characterPrefabs[characterId];
        GameObject characterInstance = Instantiate(characterPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        NetworkPlayerController characterController = characterInstance.GetComponent<NetworkPlayerController>();
        NetworkIdentity characterIdentity = characterInstance.GetComponent<NetworkIdentity>();
        // 試合IDを設定
        uint newMatchId = 1;
        characterController.matchId = newMatchId;

        // プレイヤーオブジェクトをキャラクターに置き換え
        NetworkServer.ReplacePlayerForConnection(conn, characterInstance, ReplacePlayerOptions.Destroy);
        Debug.Log($"[Server] Conn {conn.connectionId} replace Character");

        // 新しいmatchIdに基づいて、このキャラクターの可視性をサーバーに強制的に再計算させる
        NetworkServer.RebuildObservers(characterIdentity, false);
    }
}