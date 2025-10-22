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
    CONNECT_NONE,       //�ڑ����Ă��Ȃ�
    CONNECT_SUCCESS,    //�ڑ�����
}
public class ClientGameManager : MonoBehaviour
{
    // ���̃N���X�̗B��̃C���X�^���X��ێ����� (�V���O���g��)
    public static ClientGameManager Instance { get; private set; }
    [Header("�F�؃v���n�u")]
    [Tooltip("���s���ɐ�������F�؃R���|�[�l���g�̃v���n�u")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;     // �ʐM�Ǘ��t���O
    private bool isInitialized = false;
    /// <summary>
    /// �T�[�o�[�̐ڑ����
    /// </summary>
    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;
    void Awake()
    {
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
    //        Debug.LogError($"[LoadByLabel] {label} �ǂݍ��ݎ��s");
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
    //    Debug.Log("[Client] Addressables �������J�n...");

    //    yield return Addressables.InitializeAsync(); // �� �n���h����ێ����Ȃ�

    //    Debug.Log("[Client] Addressables initialized.");

    //    // ResourceLocators��2���ȏ゠�邩�m�F
    //    Debug.Log($"[Client] Catalogs Count: {Addressables.ResourceLocators.Count()}");

    //    // ��������Prefab�ǂݍ��݂�
    //    yield return LoadByLabel("Character");
    //}
    IEnumerator Start()
    {
        Debug.Log("[Client] Done waiting. Starting Addressables initialization...");
        Debug.Log("[Client] �������������J�n���܂�...");

        Debug.Log($"[Client] Build Path: {UnityEngine.AddressableAssets.Addressables.BuildPath}");
        Debug.Log($"[Client] Addressables Runtime Path: {UnityEngine.AddressableAssets.Addressables.RuntimePath}");
        Debug.Log($"[Client] Catalogs Count (before init): {Addressables.ResourceLocators.Count()}");

        // �n���h����ϐ��ɕێ����Ȃ� �� IsValid() �� Status �ɐG��Ȃ�
        yield return Addressables.InitializeAsync();

        Debug.Log("[Client] Addressables initialized.");
        Debug.Log($"[Client] Catalogs Count (after init): {Addressables.ResourceLocators.Count()}");

        // --- �v���n�u�̓ǂݍ��݂Ɠo�^ ---
        yield return RegisterAddressablePrefabsAsync();

        // --- Authenticator�̐ݒ� ---
        SetupAuthenticator();

        // --- ���������� ---
        isInitialized = true;
        Debug.Log("[Client] �������������������܂����B�ڑ��\�ł��B");
    }



    private IEnumerator RegisterAddressablePrefabsAsync()
    {
        Debug.Log("[Client] Addressables����v���n�u�̓ǂݍ��݂��J�n���܂�...");

        AsyncOperationHandle<IList<GameObject>> loadHandle = default;

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
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    NetworkClient.RegisterPrefab(prefab);
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
    #endregion
}