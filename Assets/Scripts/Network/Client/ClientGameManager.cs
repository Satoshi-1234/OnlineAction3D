using Mirror;
using System;
using System.Collections;
//using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks; // �� Task���g�����߂ɕK�v
//using UnityEditor.SceneManagement;

//using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
public enum ClientConnectStatus
{
    CONNECT_NONE,       //�ڑ����Ă��Ȃ�
    CONNECT_SUCCESS,    //�ڑ�����
}
public class ClientGameManager : NetworkManager // NetworkManager���p��
{
    public static ClientGameManager Instance { get; private set; }
    [Header("�F�؃v���n�u")]
    public GameObject authenticatorPrefab;
    private ClientConnectStatus connectStatus = ClientConnectStatus.CONNECT_NONE;
    private bool isInitialized = false;
    // �n���h���ƃv���n�u���X�g�͕s�v�ɂȂ����̂ō폜
    AsyncOperationHandle<IList<GameObject>> loadHandle = default;

    public ClientConnectStatus GetConnectStatus() => connectStatus;
    public bool GetInitialized() => isInitialized;

    public GameScene StringToGameScene(string str)
    {
        if (Enum.TryParse(str, out GameScene parsedScene))
        {
            // ������
            Debug.Log($"�ϊ�����: {parsedScene}");
            return parsedScene;
        }
            // ���s��
        Debug.LogWarning($"'{str}' �� GameScene �񋓑̂ɑ��݂��܂���B");
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
    // (Awake�͕ύX�Ȃ�)
    public override void Awake()
    {
        base.Awake();
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root.gameObject); }
        else { Destroy(gameObject); }
    }

    // ������ Start�� async void �ɕύX ������
    public override async void Start()
    {
        base.Start();
        if (isInitialized) return;
        Debug.Log("[Client] �������������J�n���܂�...");

        try
        {
            await Addressables.InitializeAsync().Task;
            Debug.Log("[Client] Addressables�����������B");
            bool loadSuccess = await RegisterAddressablePrefabsAsyncTask();
            // Addressables.Release(initHandle); // �ʏ����s�v

            if (!loadSuccess)
            {
                Debug.LogError("[Client] �v���n�u�̃��[�h/�o�^�Ɏ��s�������߁A�������𒆒f���܂��B");
                return; // �v���n�u���Ȃ���Α��s�ł��Ȃ�
            }

            // --- Authenticator�̐ݒ� ---
            SetupAuthenticator();

            // --- ���������� ---
            isInitialized = true;
            Debug.Log("[Client] �������������������܂����B�ڑ��\�ł��B");
        }
        catch (System.Exception ex)
        {
            // �������v���Z�X�S�̂ŗ\�����ʗ�O�����������ꍇ
            Debug.LogError("[Client] �������v���Z�X���ɗ�O���������܂����B");
            Debug.LogException(ex);
        }
    }

    // ������ async Task<bool> �ɕύX���Atry-catch��ǉ� ������
    private async Task<bool> RegisterAddressablePrefabsAsyncTask()
    {
        Debug.Log("[Client] Addressables����v���n�u�̓ǂݍ��݂��J�n���܂�...");
        try
        {
            loadHandle = Addressables.LoadAssetsAsync<GameObject>("Character", null);
            // �� await�Ŋ�����҂� ��
            IList<GameObject> prefabs = await loadHandle.Task;

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                int count = 0;
                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                    {
                        // spawnPrefabs���X�g��NetworkManager������
                        if (!spawnPrefabs.Contains(prefab))
                        {
                            spawnPrefabs.Add(prefab);
                            count++;
                        }
                        // NetworkClient�ւ̓o�^���s�� (�d���o�^�͓����Ŗ��������͂�)
                        //NetworkClient.RegisterPrefab(prefab);
                    }
                }
                Debug.Log($"[Client] {count} �̐V�K�v���n�u��Addressables����o�^���܂����B");
                return true; // ����
            }
            else
            {
                Debug.LogError("[Client] Addressables����̃v���n�u�ǂݍ��݂Ɏ��s���܂����B Status: " + loadHandle.Status);
                if (loadHandle.OperationException != null) Debug.LogException(loadHandle.OperationException);
                return false; // ���s
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Client] RegisterAddressablePrefabsAsyncTask���ɗ�O�������I");
            Debug.LogException(ex);
            return false; // ���s
        }
    }
    public override void OnDestroy() // NetworkManager���p�����Ă���̂� override
    {
        base.OnDestroy(); // ���N���X�̏������Ă�
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("[Client] Addressables LoadAssetsAsync �n���h����������܂����B");
        }
    }
    // (SetupAuthenticator�ȉ��̃��\�b�h�͕ύX�Ȃ�)
    #region Setup, Network Callbacks, Connection
    private void SetupAuthenticator()
    {
        // NetworkManager���p�����Ă���̂� GetComponent �͕s�v
        if (authenticatorPrefab != null)
        {
            GameObject authInstance = Instantiate(authenticatorPrefab, transform);
            CustomNetworkManager authComponent = authInstance.GetComponent<CustomNetworkManager>();
            if (authComponent != null)
            {
                authenticator = authComponent; // �������g��authenticator�t�B�[���h�ɐݒ�
                Debug.Log($"[Client] Authenticator '{authInstance.name}' �𓮓I�ɐݒ肵�܂����B");
            }
            else { Debug.LogError("[Client] Authenticator Prefab��CustomNetworkManager�R���|�[�l���g��������܂���I"); }
        }
        else if (authenticator == null) { Debug.LogWarning("[Client] Authenticator Prefab���ݒ肳��Ă��܂���B"); }
    }

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

    public void RequestServerSceneChange(GameScene requestedScene,
        string requestedSceneName = "none",
        SceneOperation sceneOperation = SceneOperation.LoadAdditive,
        bool customHandling = true)
    {
        if (connectStatus == ClientConnectStatus.CONNECT_SUCCESS && NetworkClient.isConnected)
        {
            // Enum���T�[�o�[�������ł���`�� (int��string) �ɕϊ����đ��M
            // ��: phase 3 ���V�[���J�ڃ��N�G�X�g�ArequestedScene.ToString() �ŃV�[�����𑗂�
            Debug.Log($"[Client] Send Scene Change Request: {requestedScene}");
            NetworkClient.Send(new ClientSceneChangeRequest { 
                _nextSceneLabel = requestedScene,
                _targetSceneName = requestedSceneName, 
                _sceneOperation = sceneOperation,
                _customHandling=customHandling });
        }
        else
        {
            Debug.LogError("[Client] �T�[�o�[�ɐڑ�����Ă��܂���B");
        }
    }

    /// <summary>
    /// �T�[�o�[�����SceneMessage���󂯎�����ۂ̏������I�[�o�[���C�h
    /// </summary>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log($"[Client] OnClientChangeScene Received: {newSceneName}, SceneOperation: {sceneOperation}");

        // Addressables�Ń��[�h���邽�߁AMirror�̃f�t�H���g�����͍s��Ȃ�
        // base.OnClientChangeScene(newSceneName, sceneOperation, customHandling); // �� �Ăяo���Ȃ�

        // Addressables�o�R�ŃV�[�������[�h����R���[�`�����J�n
        StartCoroutine(LoadSceneAddressable(newSceneName, sceneOperation, customHandling));
    }

    /// <summary>
    /// Addressables�ŃV�[�������[�h���A������ɃT�[�o�[�֒ʒm����R���[�`��
    /// </summary>
    private IEnumerator LoadSceneAddressable(string sceneAddressOrLabel, SceneOperation sceneOperation, bool customHandling)
    {
        if (!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
            yield break;
        }
        LoadSceneMode loadMode = sceneOperation == SceneOperation.Normal ? LoadSceneMode.Single : LoadSceneMode.Additive;
        Debug.Log($"[Client] Addressables�V�[�����[�h�J�n: {sceneAddressOrLabel} ({loadMode}) - {customHandling}");
        AsyncOperationHandle<SceneInstance> handle = default;
        bool loadSuccess = false;

        try
        {
            // true�Ń��[�h�㎩���A�N�e�B�x�[�g
            handle = Addressables.LoadSceneAsync(sceneAddressOrLabel, loadMode, customHandling);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Client] LoadSceneAsync ��O�I Address: {sceneAddressOrLabel}");
            Debug.LogException(ex);
        }

        if (!handle.IsValid())
        {
            Debug.LogError($"[Client] LoadSceneAsync �n���h������ (�Ăяo������)�B Address: {sceneAddressOrLabel}");
        }
        else
        {
            // IsDone�Ŋ�����҂�
            while (!handle.IsDone)
            {
                yield return null;
            }

            if (!handle.IsValid()) { Debug.LogError($"[Client] LoadSceneAsync �n���h������ (������)�B Address: {sceneAddressOrLabel}"); }
            else if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[Client] Addressables Scene Load Complete: {sceneAddressOrLabel}");
                loadSuccess = true;

                // �� �T�[�o�[�Ƀ��[�h������ʒm (���상�b�Z�[�W) ��
                NetworkClient.Send(new ClientSceneReadyRequest{ _nowScene = StringToGameScene(sceneAddressOrLabel)}); // �ȑO�� ClientSceneReadyRequest ����ύX

                // �� Mirror�ɏ���������ʒm (�d�v�I) ��
                if (!NetworkClient.ready) // ����Ready�łȂ��ꍇ�̂݌Ăяo��
                {
                    NetworkClient.Ready();
                }
            }
            else
            {
                Debug.LogError($"[Client] Addressables Scene Load Failed: {sceneAddressOrLabel}, Status: {handle.Status}");
                if (handle.OperationException != null) Debug.LogException(handle.OperationException);
            }

            // �n���h�����
            //Addressables.Release(handle);
        }

        // ���[�h���s���͐ؒf�Ȃ�
        if (!loadSuccess)
        {
            Debug.LogError($"�V�[�����[�h���s�̂��ߐؒf���܂�: {sceneAddressOrLabel}");
            NetworkClient.Disconnect();
        }
    }

    /// <summary>
    /// Mirror�ɃV�[�����[�h������`������ɌĂяo�����
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged(); // Mirror�̊�{���� (Ready��Ԃ̐ݒ�Ȃ�)
        Debug.Log("[Client] OnClientSceneChanged Called after Addressables load.");
        // �K�v�ł���΁A�V�[�����[�h��̒ǉ������������ɋL�q
        // ��: �V�����V�[���̃J�����ݒ�AUI�̏������Ȃ�
    }
    #endregion
}