using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
//using VSCodeEditor;

public class UI_ServerList : MonoBehaviour
{
    public TMP_Dropdown serverDropdown; // �{�^���̐e�̑����Dropdown��R�Â���
    public Button connectButton;        // �ڑ��{�^����R�Â���
    public Button startButton;          // �z�[���ֈڍs����{�^����R�Â���
    private List<DiscoveryResponse> discoveredServers = new List<DiscoveryResponse>();
#if UNITY_EDITOR
    [Header("�T�[�o�[IP�A�h���X")]
    [SerializeField] private string serverIP = "None";
    [Header("�T�[�o�[�|�[�g")]
    [SerializeField] private ushort serverPort = 7777;
    [Header("�ڑ��{�^��")]
    public Button editorConnectButton;    // �G�f�B�^�p�ڑ��{�^����R�Â���
#endif

#if !UNITY_EDITOR && !UNITY_SERVER
    private LanDiscovery discovery;
    // ���������T�[�o�[�̊��S�ȏ���ێ����邽�߂̃��X�g
    void Awake()
    {
        discovery = gameObject.AddComponent<LanDiscovery>();
        discovery.OnServerFoundEvent.AddListener(OnServerFound);
        Debug.Log($"[ClientDiscovery]�|�[�g��{discovery.GetBroadcastPort()}�ł�");
        // �N�����͐ڑ��{�^����񊈐��ɂ��Ă���
        if (connectButton != null)
        {
            connectButton.interactable = false;
            RefreshServers();
        }
    }
    public void RefreshServers()
    {
        // �����̃��X�g�ƃh���b�v�_�E���̑I�������N���A
        discoveredServers.Clear();
        //serverDropdown.ClearOptions();
        UpdateDropdown(); // �h���b�v�_�E�����u�T�[�o�[�I���v�����̏�ԂɃ��Z�b�g
        // �T�[�o�[��������܂Őڑ��{�^���͔񊈐���
        connectButton.interactable = false;

        // �����J�n
        discovery?.StartDiscovery();
        Debug.Log("[LAN] �T�[�o�[�����J�n");
    }
    void OnServerFound(DiscoveryResponse info)
    {
        if (!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client-Warning] ClientGameManager������������Ă��܂���");
            return;
        }
        //�Q�[�����܂���Ver���Ⴄ�ꍇ���X�g�ɒǉ����Ȃ�
        if (ClientGameManager.Instance.authenticatorPrefab?.GetComponent<CustomNetworkManager>() is CustomNetworkManager cnm) 
        {
            if (cnm.gameId != info._gameId || cnm.version != info._version)
            {
                Debug.LogWarning($"[Client-Warning] GameID�܂���Ver���Ⴂ�܂��I");
                return;
            }
        }
        // IP�ƃ|�[�g���L�[�ɂ��āA�����T�[�o�[�����Ƀ��X�g�ɂ��邩�m�F
        var existingServer = discoveredServers.FirstOrDefault(s => s._address == info._address && s._port == info._port);

        if (existingServer._address == null) // ���X�g�ɂȂ��ꍇ
        {
            discoveredServers.Add(info);
        }
        else // ���ɂ���T�[�o�[ �� ���X�V
        {
            // FirstOrDefault��struct�̃R�s�[��Ԃ����߁A���̃��X�g���X�V
            int index = discoveredServers.IndexOf(existingServer);
            discoveredServers[index] = info;
        }

        // �h���b�v�_�E���̕\�����X�V
        UpdateDropdown();
    }

    /// <summary>
    /// discoveredServers���X�g�̓��e�����Ƃ�Dropdown�̕\�����X�V����
    /// </summary>
    void UpdateDropdown()
    {
        int selectNum = serverDropdown.value;
        serverDropdown.ClearOptions();

        // �ŏ��Ɂu�T�[�o�[�I���v�Ƃ������x����ǉ�
        List<string> options = new List<string> { "�T�[�o�[�I��" };

        // ���������T�[�o�[�̃��X�g�𕶎���ɕϊ����Ēǉ�
        options.AddRange(discoveredServers.Select(server =>
            $"{server._address}({server._playerCount}/{server._maxPlayers})"
        ));

        serverDropdown.AddOptions(options);
        serverDropdown.value = discoveredServers.Count > 0 ? selectNum : 0;
    }
#elif UNITY_EDITOR
    public void OnClickDirectConnect()
    {
        ClientGameManager.Instance.ConnectToServer(serverIP, serverPort);
    }
#endif
    private void FixedUpdate()
    {
        // GameManager���猻�݂̐ڑ���Ԃ��擾
        ClientConnectStatus status = ClientGameManager.Instance.GetConnectStatus();
        bool isConnected = (status == ClientConnectStatus.CONNECT_SUCCESS);

        // �ڑ���Ԃɉ�����UI�̕\��/��\����؂�ւ���
#if !UNITY_EDITOR
        serverDropdown.gameObject.SetActive(!isConnected);
        connectButton.gameObject.SetActive(!isConnected);
        // �ڑ��{�^���̊���/�񊈐�
        connectButton.interactable = discoveredServers.Count > 0 && serverDropdown.value > 0;
#elif UNITY_EDITOR
        editorConnectButton.gameObject.SetActive(!isConnected);
#endif

        startButton.gameObject.SetActive(isConnected);
    }
    /// <summary>
    /// �ڑ��{�^���������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    /// </summary>
    public void OnConnectButtonClick()
    {
        int selectedIndex = serverDropdown.value;

        if (selectedIndex == 0)
        {
            Debug.Log("�ڑ�����T�[�o�[��I�����Ă��������B");
            return;
        }

        int serverListIndex = selectedIndex - 1;
        if (serverListIndex >= discoveredServers.Count) return;

        DiscoveryResponse selectedServer = discoveredServers[serverListIndex];
        // ClientGameManager�̐ڑ����\�b�h���Ăяo��
        ClientGameManager.Instance.ConnectToServer(selectedServer._address, (ushort)selectedServer._port);
        Debug.Log("�ڑ����܂�");
    }

    public void OnClickStart()
    {
        Debug.Log("�N���C�A���g�̏����������T�[�o�[�ɒʒm���܂��B");
        ClientGameManager.Instance.SetSceneToServer("Home");
    }
}
