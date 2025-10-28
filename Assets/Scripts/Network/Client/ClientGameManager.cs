using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Security.Principal;
//using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum ClientConnectStatus
{
    CONNECT_NONE,       //接続していない
    CONNECT_SUCCESS,    //接続成功
}
public class ClientGameManager : NetworkManager
{
    // このクラスの唯一のインスタンスを保持する (シングルトン)
    public static ClientGameManager Instance { get; private set; }
    [Header("認証プレハブ")]
    [Tooltip("実行時に生成する認証コンポーネントのプレハブ")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;     // 通信管理フラグ
    private bool isInitialized = false;
    AsyncOperationHandle<IList<GameObject>> loadHandle;
    private List<GameObject> loadedPrefabs = new List<GameObject>();
    /// <summary>
    /// サーバーの接続状態
    /// </summary>
    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;
    public override void Awake()
    {
        base.Awake();//test
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject); // シーンをまたいで存在させる
        }
        else
        {
            Destroy(gameObject);
        }
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

    public override void Start()//test
    {
        base.Start();
        StartCoroutine(Initialized());
    }
    private async void RegisterAddressablePrefabsAsyncTest()
    {
        Debug.Log("[Client] Addressablesからプレハブの読み込みを開始します...");
        var handle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<GameObject> prefabs = handle.Result;
            loadedPrefabs.Clear();
            int count = 0;
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    if (!spawnPrefabs.Contains(prefab))
                    {
                        spawnPrefabs.Add(prefab);
                    }
                    //NetworkClient.RegisterPrefab(prefab);
                    loadedPrefabs.Add(prefab);
                    count++;
                }
            }
            isInitialized = true;
            Debug.Log($"[Client] {count} 個のプレハブをAddressablesから登録しました。");
        }
        else
        {
            Debug.LogError("[Client] LoadAssetsAsync の呼び出し中に例外が発生！");
        }
    }
    IEnumerator Initialized()//test:start
    {
        Debug.Log("[Client] Done waiting. Starting Addressables 初期化処理を開始します...");
        // ハンドルを変数に保持しない → IsValid() や Status に触らない
        yield return Addressables.InitializeAsync();

        Debug.Log("[Client] Addressables initialized.");
        Debug.Log($"[Client] Catalogs Count ({Addressables.ResourceLocators.Count()})");

        // --- プレハブの読み込みと登録 ---
        //yield return RegisterAddressablePrefabsAsync();
        RegisterAddressablePrefabsAsyncTest();
        // --- Authenticatorの設定 ---
        SetupAuthenticator();
        // --- 初期化完了 ---
        //isInitialized = true;
        Debug.Log("[Client] 初期化処理が完了しました。接続可能です。");
    }



    private IEnumerator RegisterAddressablePrefabsAsync()
    {
        Debug.Log("[Client] Addressablesからプレハブの読み込みを開始します...");

        try
        {
            loadHandle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Client] LoadAssetsAsync の呼び出し中に例外が発生！");
            Debug.LogException(ex);
            yield break;
        }

        // tryの外で yield return（C#制約対策）
        yield return loadHandle;

        if (!loadHandle.IsValid())
        {
            Debug.LogError("[Client] LoadAssetsAsync ハンドルが完了後に無効になりました。");
            yield break;
        }

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<GameObject> prefabs = loadHandle.Result;
            int count = 0;
            //roadObjectName.Clear();
            loadedPrefabs.Clear();
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    if (!spawnPrefabs.Contains(prefab))
                    {
                        spawnPrefabs.Add(prefab);
                    }
                    //NetworkClient.RegisterPrefab(prefab);
                    loadedPrefabs.Add(prefab);
                    count++;
                }
            }
            Debug.Log($"[Client] {count} 個のプレハブをAddressablesから登録しました。");
        }
        else
        {
            Debug.LogError("[Client] プレハブの読み込みに失敗しました。 Status: " + loadHandle.Status);
            if (loadHandle.OperationException != null)
                Debug.LogException(loadHandle.OperationException);
        }
    }




    private void SetupAuthenticator()
    {
        NetworkManager manager = GetComponent<NetworkManager>();
        if (manager != null && authenticatorPrefab != null)
        {
            GameObject authInstance = Instantiate(authenticatorPrefab, manager.transform);
            CustomNetworkManager authComponent = authInstance.GetComponent<CustomNetworkManager>();
            if (authComponent != null)
            {
                manager.authenticator = authComponent;
                Debug.Log($"[Client] Authenticator '{authInstance.name}' を動的に設定しました。");
            }
            else
            {
                Debug.LogError("[Client] Authenticator PrefabにCustomNetworkManagerコンポーネントが見つかりません！");
            }
        }
        // (エラー/警告ログは省略)
    }

    // (OnEnable, OnDisable, OnAuthenticationSuccess, OnDisconnected, ConnectToServer, SetSceneToServer は変更なし)
    #region Network Callbacks and Connection

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
    public override void OnDestroy()
    {
        base.OnDestroy();
        // アプリケーション終了時に保持リストをクリア（参照を解放）
        loadedPrefabs.Clear();
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("[Client] Addressables LoadAssetsAsync ハンドルを解放しました。");
        }
        Debug.Log("[Client] ClientGameManager destroyed.");
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

    public void RequestServerSceneChange(GameScene requestedScene)
    {
        if (connectStatus == ClientConnectStatus.CONNECT_SUCCESS && NetworkClient.isConnected)
        {
            // Enumをサーバーが理解できる形式 (intやstring) に変換して送信
            // 例: phase 3 がシーン遷移リクエスト、requestedScene.ToString() でシーン名を送る
            Debug.Log($"[Client] Send Scene Change Request: {requestedScene}");
            NetworkClient.Send(new ClientSceneChangeRequest { _targetSceneName = requestedScene.ToString() }); // 新しいメッセージ型を定義
        }
        else
        {
            Debug.LogError("[Client] サーバーに接続されていません。");
        }
    }
    #endregion
}