using Mirror;
using System;
using System.Collections;
//using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks; // ★ Taskを使うために必要
//using UnityEditor.SceneManagement;

//using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
public enum ClientConnectStatus
{
    CONNECT_NONE,       //接続していない
    CONNECT_SUCCESS,    //接続成功
}
public class ClientGameManager : NetworkManager // NetworkManagerを継承
{
    public static ClientGameManager Instance { get; private set; }
    [Header("認証プレハブ")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;
    private bool isInitialized = false;
    // ハンドルとプレハブリストは不要になったので削除
    AsyncOperationHandle<IList<GameObject>> loadHandle = default;

    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;

    public GameScene StringToGameScene(string str)
    {
        if (Enum.TryParse(str, out GameScene parsedScene))
        {
            // 成功時
            Debug.Log($"変換成功: {parsedScene}");
            return parsedScene;
        }
            // 失敗時
        Debug.LogWarning($"'{str}' は GameScene 列挙体に存在しません。");
        return GameScene.Debug;
    }
    void OnGUI()
    {
        GUILayout.Label($"Build Path: {UnityEngine.AddressableAssets.Addressables.BuildPath}");
        GUILayout.Label($"Runtime Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}");
        GUILayout.Label($"Initialized: {isInitialized}");

        GUILayout.Label("--- loadedPrefabs ---");
        int validPrefabCount = 0;
        foreach (var prefab in spawnPrefabs)
        {
            if (prefab == null)
            {
                GUILayout.Label($"spawnPrefabsObject: !!! PREFAB IS NULL !!!");
            }
            else
            {
                // prefabがnullでないことを確認してからgameObjectにアクセス
                GameObject go = prefab.gameObject;
                if (go == null)
                {
                    // prefab自体は存在するが、gameObjectプロパティがnullになる異常ケース
                    GUILayout.Label($"spawnPrefabsObject: Prefab exists, but gameObject is NULL!");
                }
                else
                {
                    GUILayout.Label($"spawnPrefabsObject: {go.name}");
                    validPrefabCount++; // 有効なプレハブをカウント
                }
            }
        }
        GUILayout.Label($"Valid Prefabs Count in spawnPrefabs: {validPrefabCount}"); // 有効なプレハブ数を表示
                                                                                     // ★★★ ここまで修正 ★★★

        GUILayout.Label("--- NetworkClient.prefabs ---");
        int validNetworkClientPrefabCount = 0;
        foreach (var kvp in NetworkClient.prefabs)
        {
            if (kvp.Value == null)
            {
                GUILayout.Label($"NetworkClientObject {kvp.Key} : NULL");
            }
            else
            {
                // 値がnullでないことを確認してからnameにアクセス
                GameObject go = kvp.Value.gameObject;
                if (go == null)
                {
                    GUILayout.Label($"NetworkClientObject {kvp.Key} : Value exists, but gameObject is NULL!");
                }
                else
                {
                    GUILayout.Label($"NetworkClientObject {kvp.Key} : {go.name}");
                    validNetworkClientPrefabCount++; // 有効なプレハブをカウント
                }
            }
        }
        GUILayout.Label($"Valid Prefabs Count in NetworkClient.prefabs: {validNetworkClientPrefabCount}"); // 有効なプレハブ数を表示
        GUILayout.Label("--------------------------");
    }
    // (Awakeは変更なし)
    public override void Awake()
    {
        base.Awake();
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject.transform.root.gameObject); 
        }
        else { Destroy(gameObject); }
    }

    // ★★★ Startを async void に変更 ★★★
    public override async void Start()
    {
        base.Start();
        if (isInitialized) return;
        Debug.Log("[Client] 初期化処理を開始します...");

        try
        {
            await Addressables.InitializeAsync().Task;
            Debug.Log("[Client] Addressables初期化完了。");
            bool loadSuccess = await RegisterAddressablePrefabsAsyncTask();
            // Addressables.Release(initHandle); // 通常解放不要

            if (!loadSuccess)
            {
                Debug.LogError("[Client] プレハブのロード/登録に失敗したため、初期化を中断します。");
                return; // プレハブがなければ続行できない
            }

            // --- Authenticatorの設定 ---
            SetupAuthenticator();

            // --- 初期化完了 ---
            isInitialized = true;
            Debug.Log("[Client] 初期化処理が完了しました。接続可能です。");
        }
        catch (System.Exception ex)
        {
            // 初期化プロセス全体で予期せぬ例外が発生した場合
            Debug.LogError("[Client] 初期化プロセス中に例外が発生しました。");
            Debug.LogException(ex);
        }
    }

    // ★★★ async Task<bool> に変更し、try-catchを追加 ★★★
    private async Task<bool> RegisterAddressablePrefabsAsyncTask()
    {
        Debug.Log("[Client] Addressablesからプレハブの読み込みを開始します...");
        try
        {
            loadHandle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
            // ★ awaitで完了を待つ ★
            IList<GameObject> prefabs = await loadHandle.Task;

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                int count = 0;
                foreach (var prefab in prefabs)
                {
                    if (prefab == null) continue;

                    //spawnPrefabsリストはNetworkManagerが持つ
                    if (!spawnPrefabs.Contains(prefab))
                    {
                        if (prefab.GetComponent<PlayerState>() != null)
                        {
                            Debug.Log($"[Client] Addressablesから 'Player' (魂) プレハブをロードしました: {prefab.name}");
                            // OnServerAddPlayer で使用する 'playerPrefab' を上書き
                            playerPrefab = prefab;

                            NetworkIdentity identityToRegister = playerPrefab.GetComponent<NetworkIdentity>();
                            Debug.LogWarning($"[Client-Register] 'Player' プレハブを登録します。");
                            Debug.LogWarning($"[Client-Register] 登録する AssetID: {identityToRegister.assetId}");
                            //continue;
                        }

                        spawnPrefabs.Add(prefab);
                        count++;
                    }
                    // NetworkClientへの登録も行う (重複登録は内部で無視されるはず)
                    //NetworkClient.RegisterPrefab(prefab);
                }
                Debug.Log($"[Client] {count} 個の新規プレハブをAddressablesから登録しました。");
                return true; // 成功
            }
            else
            {
                Debug.LogError("[Client] Addressablesからのプレハブ読み込みに失敗しました。 Status: " + loadHandle.Status);
                if (loadHandle.OperationException != null) Debug.LogException(loadHandle.OperationException);
                return false; // 失敗
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Client] RegisterAddressablePrefabsAsyncTask中に例外が発生！");
            Debug.LogException(ex);
            return false; // 失敗
        }
    }
    public override void OnDestroy() // NetworkManagerを継承しているので override
    {
        base.OnDestroy(); // 基底クラスの処理を呼ぶ
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("[Client] Addressables LoadAssetsAsync ハンドルを解放しました。");
        }
    }
    // (SetupAuthenticator以下のメソッドは変更なし)
    #region Setup, Network Callbacks, Connection
    private void SetupAuthenticator()
    {
        // NetworkManagerを継承しているので GetComponent は不要
        if (authenticatorPrefab != null)
        {
            GameObject authInstance = Instantiate(authenticatorPrefab, transform);
            CustomNetworkManager authComponent = authInstance.GetComponent<CustomNetworkManager>();
            if (authComponent != null)
            {
                authenticator = authComponent; // 自分自身のauthenticatorフィールドに設定
                Debug.Log($"[Client] Authenticator '{authInstance.name}' を動的に設定しました。");
            }
            else { Debug.LogError("[Client] Authenticator PrefabにCustomNetworkManagerコンポーネントが見つかりません！"); }
        }
        else if (authenticator == null) { Debug.LogWarning("[Client] Authenticator Prefabが設定されていません。"); }
    }

    void OnEnable()
    {
        // ネットワークイベントの購読
        NetworkClient.OnDisconnectedEvent += OnDisconnected;
        //NetworkClient.RegisterHandler<ClientReadyResponse>(OnClientReadyResponse);
    }

    void OnDisable()
    {
        // ネットワークイベントの購読解除
        NetworkClient.OnDisconnectedEvent -= OnDisconnected;
        //NetworkClient.UnregisterHandler<ClientReadyResponse>();
    }
    /// <summary>
    /// CustomNetworkManagerから直接呼び出される認証成功メソッド
    /// </summary>
    public void OnAuthenticationSuccess()
    {
        Debug.Log("サーバー認証に成功しました。");
        connectStatus = ClientConnectStatus.CONNECT_SUCCESS;
    }

    /// <summary>
    /// サーバーから切断された時に呼ばれる処理
    /// </summary>
    private void OnDisconnected()
    {
        Debug.LogWarning("サーバーから切断されました。");
        connectStatus = ClientConnectStatus.CONNECT_NONE;
    }
    /// <summary>
    /// UIから呼び出されるサーバー接続メソッド
    /// </summary>
    public void ConnectToServer(string address, ushort port)
    {
        // 初期化が完了していなければ接続しない
        if (!isInitialized)
        {
            Debug.LogError("[Client] ClientGameManagerが初期化されていません。接続できません。");
            return;
        }
        Debug.Log($"接続開始: {address}:{port}");
        NetworkManager.singleton.networkAddress = address;
        (NetworkManager.singleton.transport as kcp2k.KcpTransport).Port = port;
        NetworkManager.singleton.StartClient();
    }

    /// <summary>
    /// シーン移行メソッド
    /// </summary>
    public void SetSceneToServer(string scene)
    {
        if (connectStatus == ClientConnectStatus.CONNECT_SUCCESS && NetworkClient.isConnected)
        {
            switch (scene)
            {
                case "Home":
                    Debug.Log($"[Client] Send ClientReadyRequest:Home");
                    NetworkClient.Send(new ClientReadyRequest { _phase = 1 });
                    break;
                case "BattleScene":
                    Debug.Log($"[Client] Send ClientReadyRequest:BattleScene");
                    NetworkClient.Send(new ClientReadyRequest { _phase = 2 });
                    break;
            }
        }
        else
        {
            Debug.LogError("サーバーに接続されていません。メッセージは送信できませんでした。");
        }
    }

    public void RequestServerSceneChange(GameScene requestedScene,
        string requestedSceneName = "none",
        SceneOperation sceneOperation = SceneOperation.Normal,
        bool customHandling = true)
    {
        if (connectStatus == ClientConnectStatus.CONNECT_SUCCESS && NetworkClient.isConnected)
        {
            // Enumをサーバーが理解できる形式 (intやstring) に変換して送信
            // 例: phase 3 がシーン遷移リクエスト、requestedScene.ToString() でシーン名を送る
            Debug.Log($"[Client] Send Scene Change Request: {requestedScene}");
            NetworkClient.Send(new ClientSceneChangeRequest { 
                _nextSceneLabel = requestedScene,
                _targetSceneName = requestedSceneName, 
                _sceneOperation = sceneOperation,
                _customHandling=customHandling });
        }
        else
        {
            Debug.LogError("[Client] サーバーに接続されていません。");
        }
    }

    /// <summary>
    /// サーバーからのSceneMessageを受け取った際の処理をオーバーライド
    /// </summary>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log($"[Client] OnClientChangeScene Received: {newSceneName}, SceneOperation: {sceneOperation}");

        if (NetworkClient.isConnected)
        {
            //Debug.Log($"[Client] NetworkClient Ready:{NetworkClient.ready}");
            NetworkClient.ready = false;
        }

        // Addressables経由でシーンをロードするコルーチンを開始
        StartCoroutine(LoadSceneAddressable(newSceneName, sceneOperation, customHandling));
    }

    /// <summary>
    /// Addressablesでシーンをロードし、完了後にサーバーへ通知するコルーチン
    /// </summary>
    private IEnumerator LoadSceneAddressable(string sceneAddressOrLabel, SceneOperation sceneOperation, bool customHandling)
    {
        if (!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
            yield break;
        }
        LoadSceneMode loadMode = sceneOperation == SceneOperation.Normal ? LoadSceneMode.Single : LoadSceneMode.Additive;
        Debug.Log($"[Client] Addressablesシーンロード開始: {sceneAddressOrLabel} ({loadMode}) - {customHandling}");
        AsyncOperationHandle<SceneInstance> handle = default;
        bool loadSuccess = false;

        try
        {
            // trueでロード後自動アクティベート
            handle = Addressables.LoadSceneAsync(sceneAddressOrLabel, loadMode, customHandling);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Client] LoadSceneAsync 例外！ Address: {sceneAddressOrLabel}");
            Debug.LogException(ex);
        }

        if (!handle.IsValid())
        {
            Debug.LogError($"[Client] LoadSceneAsync ハンドル無効 (呼び出し直後)。 Address: {sceneAddressOrLabel}");
        }
        else
        {
            // IsDoneで完了を待つ
            while (!handle.IsDone)
            {
                yield return null;
            }

            if (!handle.IsValid()) { Debug.LogError($"[Client] LoadSceneAsync ハンドル無効 (完了後)。 Address: {sceneAddressOrLabel}"); }
            else if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // ★★★ 修正点 ★★★
                // LoadSceneMode.Single によって NetworkClient.prefabs がクリアされたため、
                // ここでプレハブを再登録します。
                // (spawnPrefabs リストは DontDestroyOnLoad で保持されているはず)
                //Debug.LogWarning($"[Client-Fix] シーンロード完了。NetworkClient.prefabs を再登録します...");
                //int registeredCount = 0;
                //foreach (var prefab in spawnPrefabs) //
                //{
                //    if (prefab != null)
                //    {
                //        NetworkClient.RegisterPrefab(prefab); //
                //        registeredCount++;
                //    }
                //}
                //Debug.LogWarning($"[Client-Fix] {registeredCount} 個のプレハブを再登録しました。");

                Debug.Log($"[Client] Addressables Scene Load Complete: {sceneAddressOrLabel}");
                loadSuccess = true;
                //SceneManager.SetActiveScene(handle.Result.Scene);
                // ここでAwake()/OnEnable()は完了しているが Start() はまだ
                yield return null; // 1フレーム待つと全てのStart()完了
                Debug.Log($"[Client-Debug] PlayerPrefab assetID-{playerPrefab.GetComponent<NetworkIdentity>().assetId}");
            }
            else
            {
                Debug.LogError($"[Client] Addressables Scene Load Failed: {sceneAddressOrLabel}, Status: {handle.Status}");
                if (handle.OperationException != null) Debug.LogException(handle.OperationException);
            }
            // ハンドル解放
            //Addressables.Release(handle);
        }

        // ロード失敗時は切断など
        if (!loadSuccess)
        {
            Debug.LogError($"シーンロード失敗のため切断します: {sceneAddressOrLabel}");
            NetworkClient.Disconnect();
        }
    }
    /// <summary>
    /// Mirrorにシーンロード完了を伝えた後に呼び出される
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged(); // Mirrorの基本処理 (Ready状態の設定など)
        Debug.Log("[Client] OnClientSceneChanged Called after Addressables load.");
        // 必要であれば、シーンロード後の追加処理をここに記述
        // 例: 新しいシーンのカメラ設定、UIの初期化など
    }
    #endregion
}