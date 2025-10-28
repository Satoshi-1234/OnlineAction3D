using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ServerGameManager : NetworkManager
{
    public static ServerGameManager Instance { get; private set; }
    [Header("�F�؃v���n�u")]
    [Tooltip("���s���ɐ�������F�؃R���|�[�l���g�̃v���n�u")]
    public GameObject authenticatorPrefab;
    [Header("�L�����N�^�[�v���n�u")]
    private GameObject[] characterPrefabs;
    //#if UNITY_SERVER && !UNITY_EDITOR
    public override void Awake()
    {
        base.Awake();
        if (authenticatorPrefab != null)
        {
            // �v���n�u����F�؃R���|�[�l���g�𐶐�
            GameObject authInstance = Instantiate(authenticatorPrefab, transform); // NetworkManager�̎q�Ƃ��Đ���

            // ���������C���X�^���X����CustomNetworkManager�R���|�[�l���g���擾
            CustomNetworkManager authComponent = authInstance.GetComponent<CustomNetworkManager>();

            if (authComponent != null)
            {
                // NetworkManager��authenticator�t�B�[���h�ɐݒ�
                authenticator = authComponent;
                Debug.Log($"[Server-Success] Authenticator '{authInstance.name}' set it dynamically");
            }
            else
            {
                Debug.LogError("[Server-Error] Authenticator PrefabCustomNetworkManager Not Component");
            }
        }
        else if (authenticator == null) // �v���n�u�����ݒ�ŁA���蓮�ł��ݒ肳��Ă��Ȃ��ꍇ
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


    // �v���C���[�����������ꂽ�ڑ�ID�̃��X�g
    private readonly HashSet<int> connectionsReadyForPlayer = new HashSet<int>();
    public override void OnStartServer()
    {
        base.OnStartServer();
        //Instance = this;
        NetworkServer.RegisterHandler<ClientReadyRequest>(OnClientReady);//�p���\��
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
    /// "Character"���x�������S�ẴA�Z�b�g��Addressables����񓯊��œǂݍ���
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
            Debug.Log($"[Server] {characterPrefabs.Length} �̂̃L�����N�^�[�v���n�u��Addressables����ǂݍ��݂܂����B");
        }
        else
        {
            Debug.LogError("[Server] Addressables����̃L�����N�^�[�v���n�u�ǂݍ��݂Ɏ��s���܂����B");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // �N���C�A���g���ؒf������A�����X�g����폜����
        connectionsReadyForPlayer.Remove(conn.connectionId);

        // base�̏������Ăяo���āA�v���C���[�I�u�W�F�N�g�Ȃǂ𐳂����N���[���A�b�v����
        base.OnServerDisconnect(conn);
    }
    // �N���C�A���g�������ł�����Ă΂��
    void OnClientReady(NetworkConnectionToClient conn, ClientReadyRequest msg)
    {
        Debug.Log($"[Server] GetReadyRequest from {conn.connectionId}, Phase:{msg._phase}");
        switch (msg._phase)
        {
            case 1:
                // �V�[���J�ڂ𖽗�
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
        // ������ �����Ń}�b�`���O�Q���ґS���̏��������������`�F�b�N���郍�W�b�N������ ������
        // ��:
        // Match currentMatch = FindMatchContainingPlayer(conn);
        // if (currentMatch != null)
        // {
        //     currentMatch.MarkPlayerAsReady(conn.connectionId);
        //     if (currentMatch.AreAllPlayersReady())
        //     {
        //         StartMatchCountdown(currentMatch); // �S�����������Ȃ玎���J�n������
        //     }
        // }

        // ����: �ȑO�� SetClientReady �͂����ł͌Ă΂Ȃ����ƁB
        // SetClientReady �̓v���C���[�I�u�W�F�N�g�̏����������Ӗ����A
        // �V�[���̏��������Ƃ͕ʂł��B
        // NetworkServer.SetClientReady(conn); // �� ����͕s�v
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log($"[Server] OnServerAddPlayer: conn {conn.connectionId} Generated");
        GameObject player = Instantiate(playerPrefab);
        // PlayerConnection�̉��������L�҂݂̂Ɍ��肷��
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        identity.visibility = Visibility.ForceHidden; // �܂��S������B��
        NetworkServer.AddPlayerForConnection(conn, player); //AddObserver���s���Ă���
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
        // ����ID��ݒ�
        uint newMatchId = 1;
        characterController.matchId = newMatchId;

        // �v���C���[�I�u�W�F�N�g���L�����N�^�[�ɒu������
        NetworkServer.ReplacePlayerForConnection(conn, characterInstance, ReplacePlayerOptions.Destroy);
        Debug.Log($"[Server] Conn {conn.connectionId} replace Character");

        // �V����matchId�Ɋ�Â��āA���̃L�����N�^�[�̉������T�[�o�[�ɋ����I�ɍČv�Z������
        NetworkServer.RebuildObservers(characterIdentity, false);
    }
}