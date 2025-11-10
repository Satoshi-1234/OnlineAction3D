using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using Mirror;
//using VSCodeEditor;

public class UI_ServerList : MonoBehaviour
{
    public TMP_Dropdown serverDropdown; // ボタンの親の代わりにDropdownを紐づける
    public Button connectButton;        // 接続ボタンを紐づける
    public Button startButton;          // ホームへ移行するボタンを紐づける
    private List<DiscoveryResponse> discoveredServers = new List<DiscoveryResponse>();
#if UNITY_EDITOR
    [Header("EditorNetworkSetting")]
    [SerializeField] private EditorNetworkSettings networkEditor;
    //[Header("サーバーIPアドレス")]
    //[SerializeField] private string serverIP = "None";
    //[Header("サーバーポート")]
    //[SerializeField] private ushort serverPort = 7777;
    [Header("接続ボタン")]
    public Button editorConnectButton;    // エディタ用接続ボタンを紐づける
#endif

#if !UNITY_EDITOR && !UNITY_SERVER
    private LanDiscovery discovery;
    // 発見したサーバーの完全な情報を保持するためのリスト
    void Awake()
    {
        discovery = gameObject.AddComponent<LanDiscovery>();
        discovery.OnServerFoundEvent.AddListener(OnServerFound);
        Debug.Log($"[ClientDiscovery]ポートは{discovery.GetBroadcastPort()}です");
        // 起動時は接続ボタンを非活性にしておく
        if (connectButton != null)
        {
            connectButton.interactable = false;
            RefreshServers();
        }
    }
    public void RefreshServers()
    {
        // 既存のリストとドロップダウンの選択肢をクリア
        discoveredServers.Clear();
        //serverDropdown.ClearOptions();
        UpdateDropdown(); // ドロップダウンを「サーバー選択」だけの状態にリセット
        // サーバーが見つかるまで接続ボタンは非活性化
        connectButton.interactable = false;

        // 検索開始
        discovery?.StartDiscovery();
        Debug.Log("[LAN] サーバー検索開始");
    }
    void OnServerFound(DiscoveryResponse info)
    {
        if (!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client-Warning] ClientGameManagerが初期化されていません");
            return;
        }
        //ゲーム名またはVerが違う場合リストに追加しない
        if (ClientGameManager.Instance.authenticatorPrefab?.GetComponent<CustomNetworkManager>() is CustomNetworkManager cnm) 
        {
            if (cnm.gameId != info._gameId || cnm.version != info._version)
            {
                Debug.LogWarning($"[Client-Warning] GameIDまたはVerが違います！");
                return;
            }
        }
        // IPとポートをキーにして、同じサーバーが既にリストにあるか確認
        var existingServer = discoveredServers.FirstOrDefault(s => s._address == info._address && s._port == info._port);

        if (existingServer._address == null) // リストにない場合
        {
            discoveredServers.Add(info);
        }
        else // 既にあるサーバー → 情報更新
        {
            // FirstOrDefaultはstructのコピーを返すため、元のリストを更新
            int index = discoveredServers.IndexOf(existingServer);
            discoveredServers[index] = info;
        }

        // ドロップダウンの表示を更新
        UpdateDropdown();
    }

    /// <summary>
    /// discoveredServersリストの内容をもとにDropdownの表示を更新する
    /// </summary>
    void UpdateDropdown()
    {
        int selectNum = serverDropdown.value;
        serverDropdown.ClearOptions();

        // 最初に「サーバー選択」というラベルを追加
        List<string> options = new List<string> { "サーバー選択" };

        // 発見したサーバーのリストを文字列に変換して追加
        options.AddRange(discoveredServers.Select(server =>
            $"{server._address}({server._playerCount}/{server._maxPlayers})"
        ));

        serverDropdown.AddOptions(options);
        serverDropdown.value = discoveredServers.Count > 0 ? selectNum : 0;
    }
#elif UNITY_EDITOR
    public void OnClickDirectConnect()
    {
        if(networkEditor == null)
        {
            Debug.LogError($"[Client/Error] networkEditor がnullです！");
            return;
        }
        ClientGameManager.Instance.ConnectToServer(networkEditor.serverIPv4, networkEditor.serverPort);
    }
#endif
    private void FixedUpdate()
    {
        // GameManagerから現在の接続状態を取得
        ClientConnectStatus status = ClientGameManager.Instance.GetConnectStatus();
        bool isConnected = (status == ClientConnectStatus.CONNECT_SUCCESS);

        // 接続状態に応じてUIの表示/非表示を切り替える
#if !UNITY_EDITOR
        serverDropdown.gameObject.SetActive(!isConnected);
        connectButton.gameObject.SetActive(!isConnected);
        // 接続ボタンの活性/非活性
        connectButton.interactable = discoveredServers.Count > 0 && serverDropdown.value > 0;
#elif UNITY_EDITOR
        editorConnectButton.gameObject.SetActive(!isConnected);
#endif

        startButton.gameObject.SetActive(isConnected);
    }
    /// <summary>
    /// 接続ボタンが押されたときに呼び出されるメソッド
    /// </summary>
    public void OnConnectButtonClick()
    {
        int selectedIndex = serverDropdown.value;

        if (selectedIndex == 0)
        {
            Debug.Log("接続するサーバーを選択してください。");
            return;
        }

        int serverListIndex = selectedIndex - 1;
        if (serverListIndex >= discoveredServers.Count) return;

        DiscoveryResponse selectedServer = discoveredServers[serverListIndex];
        // ClientGameManagerの接続メソッドを呼び出す
        ClientGameManager.Instance.ConnectToServer(selectedServer._address, (ushort)selectedServer._port);
        Debug.Log("接続します");
    }

    public void OnClickStart()
    {
        Debug.Log("クライアントの準備完了をサーバーに通知します。");
        //ClientGameManager.Instance.SetSceneToServer("Home");
        ClientGameManager.Instance.RequestServerSceneChange(GameScene.Home, GameScene.Home.ToString());
    }
    private void OnDestroy()
    {
        Debug.Log($"[Title] Destroy");
    }
}
