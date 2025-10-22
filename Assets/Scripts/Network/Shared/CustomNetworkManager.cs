using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class CustomNetworkManager : NetworkAuthenticator
{
    public static CustomNetworkManager Instance { get; private set; }

    [Header("識別情報")]
    public string gameId = "My3DGame";   // プロジェクト固有名に変更推奨
    public string version = "0.1.0";     // クライアント/サーバーで一致させる

    // ★★★ Awakeを追加 ★★★
    void Awake()
    {
        // 簡単なシングルトン設定（シーン内に複数存在しない前提）
        if (Instance == null)
        {
            Instance = this;
            // NetworkManagerの子になっているので、DontDestroyOnLoadは不要
        }
        else
        {
            Debug.LogWarning("[Shared-Success] Multiple CustomNetworkManager instances detected.");
            Destroy(gameObject); // 必要に応じて破棄
        }
    }

    // ========================================
    // Mirror 標準フック（サーバー側）
    // ========================================
#if UNITY_SERVER && !UNITY_EDITOR
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<HandshakeRequest>(OnServerHandshakeRequest, false);
        Debug.Log($"サーバー初期化");
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<HandshakeRequest>();
    }
#endif

    // ========================================
    // Mirror 標準フック（クライアント側）
    // ========================================
#if !UNITY_SERVER || UNITY_EDITOR
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<HandshakeResponse>(OnClientHandshakeResponse, false);
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<HandshakeResponse>();
    }
#endif

    // ========================================
    // 認証フック（サーバー）
    // ========================================
#if UNITY_SERVER && !UNITY_EDITOR
    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // 何もしない: HandshakeRequest を待つ
    }
#endif

    // ========================================
    // 認証フック（クライアント）
    // ========================================
#if !UNITY_SERVER || UNITY_EDITOR
    public override void OnClientAuthenticate()
    {
        var req = new HandshakeRequest { _gameId = gameId, _version = version };
        Debug.Log($"[AUTH/CLIENT] send handshake: {req._gameId} v{req._version}");
        NetworkClient.Send(req);
    }
#endif

    // ========================================
    // Handshake サーバー側
    // ========================================
#if UNITY_SERVER && !UNITY_EDITOR
    void OnServerHandshakeRequest(NetworkConnectionToClient conn, HandshakeRequest req)
    {
        bool ok = (req._gameId == gameId && req._version == version);
        string msg = ok ? "OK" : $"Mismatch (srv:{gameId} {version} / cli:{req._gameId} {req._version})";

        conn.Send(new HandshakeResponse { _accepted = ok, _message = msg });

        if (ok)
        {
            Debug.Log($"[AUTH/SERVER] accept: {conn.connectionId} {msg}");
            // ServerAcceptを呼ぶだけ。Mirrorのデフォルトフローに任せる。
            ServerAccept(conn);
        }
        else
        {
            Debug.LogWarning($"[AUTH/SERVER] reject: {conn.connectionId} {msg}");
            ServerReject(conn);   // Mirror API
        }
    }
#endif

    // ========================================
    // Handshake クライアント側
    // ========================================
#if !UNITY_SERVER || UNITY_EDITOR
    void OnClientHandshakeResponse(HandshakeResponse res)
    {
        if (res._accepted)
        {
            Debug.Log($"[AUTH/CLIENT] accepted: {res._message}");
            ClientAccept();       // Mirror API
            // 認証が成功したことをClientGameManagerに直接伝える
            ClientGameManager.Instance.OnAuthenticationSuccess();
        }
        else
        {
            Debug.LogWarning($"[AUTH/CLIENT] rejected: {res._message}");
            ClientReject();       // Mirror API
        }
    }
#endif
}