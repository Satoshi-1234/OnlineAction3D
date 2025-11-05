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
    private List<GameObject> characterPrefabs = new List<GameObject>();
    private bool prefabsLoaded = false;
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

    private void FixedUpdate()
    {
        if (NetworkServer.active)
        {
            return;
        }
        if (NetworkManager.singleton != null)
            Debug.LogError("[Server-Error] NetworkServer is not active during scene load");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<ClientSceneChangeRequest>(OnClientSceneChange);
        NetworkServer.RegisterHandler<ClientSceneReadyRequest>(OnClientSceneReady);
        LoadCharacterPrefabsAsync();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        //NetworkServer.UnregisterHandler<ClientReadyRequest>();
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
            List<GameObject> loadedCharacterModels = new List<GameObject>();
            foreach (var prefab in handle.Result)
            {
                if (prefab == null) continue;
                // A. 'PlayerState' コンポーネントを持つプレハブ (＝魂プレハブ)
                if (prefab.GetComponent<PlayerState>() != null)
                {
                    Debug.Log($"[Server] Addressables 'Player' (Soul) Prefab Load: {prefab.name}");
                    // OnServerAddPlayer で使用する 'playerPrefab' を上書き
                    playerPrefab = prefab;
                }
                // B. 'NetworkPlayerController' を持つプレハブ (＝キャラクターモデル)
                else if (prefab.GetComponent<NetworkPlayerController>() != null)
                {
                    loadedCharacterModels.Add(prefab);
                    if (!spawnPrefabs.Contains(prefab))
                    {
                        spawnPrefabs.Add(prefab);
                        characterPrefabs.Add(prefab);
                    }
                }
            }

            // 'playerPrefab' (魂) も spawnPrefabs リストに登録されていることを確認
            if (playerPrefab != null && !spawnPrefabs.Contains(playerPrefab))
            {
                spawnPrefabs.Add(playerPrefab);
            }
            else if (playerPrefab == null)
            {
                Debug.LogError("[Server] 'Character' on the label 'PlayerState' No prefab found with !");
            }
            prefabsLoaded = true;
            // ★★★ 修正点 3/3 ★★★
            // SpawnCharacterForPlayer で使う 'characterPrefabs' 配列を、モデルのみで再構築
            Debug.Log($"[Server] {characterPrefabs.Count} character model was loaded and registered from Addressables.");
        }
        else
        {
            Debug.LogError("[Server] Failed to load character prefab from Addressables.");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // baseの処理を呼び出して、プレイヤーオブジェクトなどを正しくクリーンアップする
        base.OnServerDisconnect(conn);
    }

    void OnClientSceneChange(NetworkConnectionToClient conn,ClientSceneChangeRequest msg)
    {
        switch (msg._nextSceneLabel)
        {
            case GameScene.Title:
                break;
            case GameScene.Home:
            case GameScene.BattleCastle://test
            case GameScene.BattleForest:
                Debug.Log($"[Server] SceneMessage: {msg._nextSceneLabel.ToString()}, sceneOperation: {msg._sceneOperation}, customHandling: {msg._customHandling}");
                conn.Send(new SceneMessage { sceneName = msg._nextSceneLabel.ToString(), sceneOperation = msg._sceneOperation, customHandling = msg._customHandling });
                break;
            case GameScene.Debug:
                conn.Send(new SceneMessage { sceneName = msg._targetSceneName, sceneOperation = msg._sceneOperation, customHandling = msg._customHandling });
                break;
        }
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
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (!prefabsLoaded)
        {
            Debug.LogError($"[Server] OnServerAddPlayer: Dont Loaded");
            return;
        }
        if (playerPrefab == null)
        {
            Debug.LogError($"[Server-Wernning] Not PlayerPrefab");
            return;
        }
        // ★★★ デバッグログここまで ★★★
        Debug.Log($"[Server] OnServerAddPlayer: conn {conn.connectionId} Generated assetID-{playerPrefab.GetComponent<NetworkIdentity>().assetId}");
        GameObject player = Instantiate(playerPrefab);
        // PlayerConnectionの可視性を所有者のみに限定する
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        identity.visibility = Visibility.ForceHidden; // まず全員から隠す
        NetworkServer.AddPlayerForConnection(conn, player); //AddObserverを行っている
        if (identity.observers.Count > 0) 
            Debug.Log($"[Server] observers Count: {identity.observers.Count},Send assetID-{identity.assetId}");
    }
    public void SpawnCharacterForPlayer(NetworkConnectionToClient conn, int characterId)
    {
        if (characterId < 0 || characterId >= characterPrefabs.Count)
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