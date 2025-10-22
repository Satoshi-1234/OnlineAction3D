using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class CustomNetworkManager : NetworkAuthenticator
{
    public static CustomNetworkManager Instance { get; private set; }

    [Header("���ʏ��")]
    public string gameId = "My3DGame";   // �v���W�F�N�g�ŗL���ɕύX����
    public string version = "0.1.0";     // �N���C�A���g/�T�[�o�[�ň�v������

    // ������ Awake��ǉ� ������
    void Awake()
    {
        // �ȒP�ȃV���O���g���ݒ�i�V�[�����ɕ������݂��Ȃ��O��j
        if (Instance == null)
        {
            Instance = this;
            // NetworkManager�̎q�ɂȂ��Ă���̂ŁADontDestroyOnLoad�͕s�v
        }
        else
        {
            Debug.LogWarning("[Shared-Success] Multiple CustomNetworkManager instances detected.");
            Destroy(gameObject); // �K�v�ɉ����Ĕj��
        }
    }

    // ========================================
    // Mirror �W���t�b�N�i�T�[�o�[���j
    // ========================================
#if UNITY_SERVER && !UNITY_EDITOR
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<HandshakeRequest>(OnServerHandshakeRequest, false);
        Debug.Log($"�T�[�o�[������");
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<HandshakeRequest>();
    }
#endif

    // ========================================
    // Mirror �W���t�b�N�i�N���C�A���g���j
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
    // �F�؃t�b�N�i�T�[�o�[�j
    // ========================================
#if UNITY_SERVER && !UNITY_EDITOR
    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // �������Ȃ�: HandshakeRequest ��҂�
    }
#endif

    // ========================================
    // �F�؃t�b�N�i�N���C�A���g�j
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
    // Handshake �T�[�o�[��
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
            // ServerAccept���ĂԂ����BMirror�̃f�t�H���g�t���[�ɔC����B
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
    // Handshake �N���C�A���g��
    // ========================================
#if !UNITY_SERVER || UNITY_EDITOR
    void OnClientHandshakeResponse(HandshakeResponse res)
    {
        if (res._accepted)
        {
            Debug.Log($"[AUTH/CLIENT] accepted: {res._message}");
            ClientAccept();       // Mirror API
            // �F�؂������������Ƃ�ClientGameManager�ɒ��ړ`����
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