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
    CONNECT_NONE,       //�ڑ����Ă��Ȃ�
    CONNECT_SUCCESS,    //�ڑ�����
}
public class ClientGameManager : NetworkManager
{
    // ���̃N���X�̗B��̃C���X�^���X��ێ����� (�V���O���g��)
    public static ClientGameManager Instance { get; private set; }
    [Header("�F�؃v���n�u")]
    [Tooltip("���s���ɐ�������F�؃R���|�[�l���g�̃v���n�u")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;     // �ʐM�Ǘ��t���O
    private bool isInitialized = false;
    AsyncOperationHandle<IList<GameObject>> loadHandle;
    private List<GameObject> loadedPrefabs = new List<GameObject>();
    /// <summary>
    /// �T�[�o�[�̐ڑ����
    /// </summary>
    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;
    public override void Awake()
    {
        base.Awake();//test
        // �V���O���g���p�^�[���̎���
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject); // �V�[�����܂����ő��݂�����
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
                // prefab��null�łȂ����Ƃ��m�F���Ă���gameObject�ɃA�N�Z�X
                GameObject go = prefab.gameObject;
                if (go == null)
                {
                    // prefab���̂͑��݂��邪�AgameObject�v���p�e�B��null�ɂȂ�ُ�P�[�X
                    GUILayout.Label($"spawnPrefabsObject: Prefab exists, but gameObject is NULL!");
                }
                else
                {
                    GUILayout.Label($"spawnPrefabsObject: {go.name}");
                    validPrefabCount++; // �L���ȃv���n�u���J�E���g
                }
            }
        }
        GUILayout.Label($"Valid Prefabs Count in spawnPrefabs: {validPrefabCount}"); // �L���ȃv���n�u����\��
                                                                                      // ������ �����܂ŏC�� ������

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
                // �l��null�łȂ����Ƃ��m�F���Ă���name�ɃA�N�Z�X
                GameObject go = kvp.Value.gameObject;
                if (go == null)
                {
                    GUILayout.Label($"NetworkClientObject {kvp.Key} : Value exists, but gameObject is NULL!");
                }
                else
                {
                    GUILayout.Label($"NetworkClientObject {kvp.Key} : {go.name}");
                    validNetworkClientPrefabCount++; // �L���ȃv���n�u���J�E���g
                }
            }
        }
        GUILayout.Label($"Valid Prefabs Count in NetworkClient.prefabs: {validNetworkClientPrefabCount}"); // �L���ȃv���n�u����\��
        GUILayout.Label("--------------------------");
    }

    public override void Start()//test
    {
        base.Start();
        StartCoroutine(Initialized());
    }
    private async void RegisterAddressablePrefabsAsyncTest()
    {
        Debug.Log("[Client] Addressables����v���n�u�̓ǂݍ��݂��J�n���܂�...");
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
            Debug.Log($"[Client] {count} �̃v���n�u��Addressables����o�^���܂����B");
        }
        else
        {
            Debug.LogError("[Client] LoadAssetsAsync �̌Ăяo�����ɗ�O�������I");
        }
    }
    IEnumerator Initialized()//test:start
    {
        Debug.Log("[Client] Done waiting. Starting Addressables �������������J�n���܂�...");
        // �n���h����ϐ��ɕێ����Ȃ� �� IsValid() �� Status �ɐG��Ȃ�
        yield return Addressables.InitializeAsync();

        Debug.Log("[Client] Addressables initialized.");
        Debug.Log($"[Client] Catalogs Count ({Addressables.ResourceLocators.Count()})");

        // --- �v���n�u�̓ǂݍ��݂Ɠo�^ ---
        //yield return RegisterAddressablePrefabsAsync();
        RegisterAddressablePrefabsAsyncTest();
        // --- Authenticator�̐ݒ� ---
        SetupAuthenticator();
        // --- ���������� ---
        //isInitialized = true;
        Debug.Log("[Client] �������������������܂����B�ڑ��\�ł��B");
    }



    private IEnumerator RegisterAddressablePrefabsAsync()
    {
        Debug.Log("[Client] Addressables����v���n�u�̓ǂݍ��݂��J�n���܂�...");

        try
        {
            loadHandle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Client] LoadAssetsAsync �̌Ăяo�����ɗ�O�������I");
            Debug.LogException(ex);
            yield break;
        }

        // try�̊O�� yield return�iC#����΍�j
        yield return loadHandle;

        if (!loadHandle.IsValid())
        {
            Debug.LogError("[Client] LoadAssetsAsync �n���h����������ɖ����ɂȂ�܂����B");
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
            Debug.Log($"[Client] {count} �̃v���n�u��Addressables����o�^���܂����B");
        }
        else
        {
            Debug.LogError("[Client] �v���n�u�̓ǂݍ��݂Ɏ��s���܂����B Status: " + loadHandle.Status);
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
                Debug.Log($"[Client] Authenticator '{authInstance.name}' �𓮓I�ɐݒ肵�܂����B");
            }
            else
            {
                Debug.LogError("[Client] Authenticator Prefab��CustomNetworkManager�R���|�[�l���g��������܂���I");
            }
        }
        // (�G���[/�x�����O�͏ȗ�)
    }

    // (OnEnable, OnDisable, OnAuthenticationSuccess, OnDisconnected, ConnectToServer, SetSceneToServer �͕ύX�Ȃ�)
    #region Network Callbacks and Connection

    void OnEnable()
    {
        // �l�b�g���[�N�C�x���g�̍w��
        NetworkClient.OnDisconnectedEvent += OnDisconnected;
        //NetworkClient.RegisterHandler<ClientReadyResponse>(OnClientReadyResponse);
    }

    void OnDisable()
    {
        // �l�b�g���[�N�C�x���g�̍w�ǉ���
        NetworkClient.OnDisconnectedEvent -= OnDisconnected;
        //NetworkClient.UnregisterHandler<ClientReadyResponse>();
    }
    /// <summary>
    /// CustomNetworkManager���璼�ڌĂяo�����F�ؐ������\�b�h
    /// </summary>
    public void OnAuthenticationSuccess()
    {
        Debug.Log("�T�[�o�[�F�؂ɐ������܂����B");
        connectStatus = ClientConnectStatus.CONNECT_SUCCESS;
    }

    /// <summary>
    /// �T�[�o�[����ؒf���ꂽ���ɌĂ΂�鏈��
    /// </summary>
    private void OnDisconnected()
    {
        Debug.LogWarning("�T�[�o�[����ؒf����܂����B");
        connectStatus = ClientConnectStatus.CONNECT_NONE;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        // �A�v���P�[�V�����I�����ɕێ����X�g���N���A�i�Q�Ƃ�����j
        loadedPrefabs.Clear();
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("[Client] Addressables LoadAssetsAsync �n���h����������܂����B");
        }
        Debug.Log("[Client] ClientGameManager destroyed.");
    }
    /// <summary>
    /// UI����Ăяo�����T�[�o�[�ڑ����\�b�h
    /// </summary>
    public void ConnectToServer(string address, ushort port)
    {
        // ���������������Ă��Ȃ���ΐڑ����Ȃ�
        if (!isInitialized)
        {
            Debug.LogError("[Client] ClientGameManager������������Ă��܂���B�ڑ��ł��܂���B");
            return;
        }
        Debug.Log($"�ڑ��J�n: {address}:{port}");
        NetworkManager.singleton.networkAddress = address;
        (NetworkManager.singleton.transport as kcp2k.KcpTransport).Port = port;
        NetworkManager.singleton.StartClient();
    }

    /// <summary>
    /// �V�[���ڍs���\�b�h
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
            Debug.LogError("�T�[�o�[�ɐڑ�����Ă��܂���B���b�Z�[�W�͑��M�ł��܂���ł����B");
        }
    }

    public void RequestServerSceneChange(GameScene requestedScene)
    {
        if (connectStatus == ClientConnectStatus.CONNECT_SUCCESS && NetworkClient.isConnected)
        {
            // Enum���T�[�o�[�������ł���`�� (int��string) �ɕϊ����đ��M
            // ��: phase 3 ���V�[���J�ڃ��N�G�X�g�ArequestedScene.ToString() �ŃV�[�����𑗂�
            Debug.Log($"[Client] Send Scene Change Request: {requestedScene}");
            NetworkClient.Send(new ClientSceneChangeRequest { _targetSceneName = requestedScene.ToString() }); // �V�������b�Z�[�W�^���`
        }
        else
        {
            Debug.LogError("[Client] �T�[�o�[�ɐڑ�����Ă��܂���B");
        }
    }
    #endregion
}