using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum ClientConnectStatus
{
    CONNECT_NONE,       //接続していない
    CONNECT_SUCCESS,    //接続成功
}
public class ClientGameManager : MonoBehaviour
{
    // このクラスの唯一のインスタンスを保持する (シングルトン)
    public static ClientGameManager Instance { get; private set; }
    [Header("認証プレハブ")]
    [Tooltip("実行時に生成する認証コンポーネントのプレハブ")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;     // 通信管理フラグ
    private bool isInitialized = false;
    /// <summary>
    /// サーバーの接続状態
    /// </summary>
    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;
    void Awake()
    {
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
    private string initPrefabName;
    void OnGUI()
    {
        GUILayout.Label($"Build Path: {UnityEngine.AddressableAssets.Addressables.BuildPath}");
        GUILayout.Label($"Runtime Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}");
        GUILayout.Label($"Instantiated_Object: {initPrefabName}");
        GUILayout.Label($"Initialized: {isInitialized}");
    }
    //IEnumerator LoadByLabel(string label)
    //{
    //    var handle = Addressables.LoadAssetsAsync<GameObject>(label, null);
    //    yield return handle;

    //    if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded)
    //    {
    //        Debug.LogError($"[LoadByLabel] {label} 読み込み失敗");
    //        yield break;
    //    }

    //    foreach (var prefab in handle.Result)
    //    {
    //        GameObject go = Instantiate(prefab);
    //        go.name = $"Instantiated_{prefab.name}";
    //        initPrefabName = go.name;
    //    }

    //    Addressables.Release(handle);
    //}
    //IEnumerator Start()
    //{
    //    Debug.Log("[Client] Addressables 初期化開始...");

    //    yield return Addressables.InitializeAsync(); // ★ ハンドルを保持しない

    //    Debug.Log("[Client] Addressables initialized.");

    //    // ResourceLocatorsが2件以上あるか確認
    //    Debug.Log($"[Client] Catalogs Count: {Addressables.ResourceLocators.Count()}");

    //    // ここからPrefab読み込みへ
    //    yield return LoadByLabel("Character");
    //}
    IEnumerator Start()
    {
        Debug.Log("[Client] Done waiting. Starting Addressables initialization...");
        Debug.Log("[Client] 初期化処理を開始します...");

        Debug.Log($"[Client] Build Path: {UnityEngine.AddressableAssets.Addressables.BuildPath}");
        Debug.Log($"[Client] Addressables Runtime Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}");
        Debug.Log($"[Client] Catalogs Count (before init): {Addressables.ResourceLocators.Count()}");

        // ハンドルを変数に保持しない → IsValid() や Status に触らない
        yield return Addressables.InitializeAsync();

        Debug.Log("[Client] Addressables initialized.");
        Debug.Log($"[Client] Catalogs Count (after init): {Addressables.ResourceLocators.Count()}");

        // --- プレハブの読み込みと登録 ---
        yield return RegisterAddressablePrefabsAsync();

        // --- Authenticatorの設定 ---
        SetupAuthenticator();

        // --- 初期化完了 ---
        isInitialized = true;
        Debug.Log("[Client] 初期化処理が完了しました。接続可能です。");
    }



    private IEnumerator RegisterAddressablePrefabsAsync()
    {
        Debug.Log("[Client] Addressablesからプレハブの読み込みを開始します...");

        AsyncOperationHandle<IList<GameObject>> loadHandle = default;

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
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    NetworkClient.RegisterPrefab(prefab);
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

        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
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
    #endregion
}