using System.Net.NetworkInformation;
//using System.Linq;
using Mirror;
using Mirror.Discovery;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
//using VSCodeEditor;

// Mirror �� NetworkDiscoveryBase ���p��nse>
public class LanDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    [System.Serializable]
    public class ServerFoundEvent : UnityEvent<DiscoveryResponse> { }
    public ServerFoundEvent OnServerFoundEvent = new ServerFoundEvent();
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
#if UNITY_SERVER && !UNITY_EDITOR
        
        var nm = NetworkManager.singleton;
        var transport = nm.transport as kcp2k.KcpTransport;
        Debug.Log($"[Server:{GetLocalIPv4()}] Get ProcessRequest");
        return new DiscoveryResponse
        {
            _uri = transport.ServerUri(),
            // endpoint�͒��ڕێ��ł��Ȃ��̂�string��
            _address = GetLocalIPv4(),
            _port = transport.Port,
            //_port = (nm.transport as kcp2k.KcpTransport)?.Port ?? 7777,
            // CustomNetworkManager �̎��ʏ��������ŗ��p
            _gameId = (nm.authenticator as CustomNetworkManager)?.gameId ?? "Unknown",
            _version = (nm.authenticator as CustomNetworkManager)?.version ?? "0.0.0",
            _playerCount = nm.numPlayers,
            _maxPlayers = nm.maxConnections,
            _serverName = System.Environment.MachineName // PC�����T�[�o�[���ɐݒ�
        };
#else
        Debug.Log($"[Server] Get ProcessRequest�FNotServer");
        return new DiscoveryResponse();
#endif
    }
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
#if !UNITY_SERVER || UNITY_EDITOR
        Debug.Log($"[LAN] �T�[�o�[���o: {response._serverName} ({response._gameId} v{response._version}) " +
                  $"Players {response._playerCount}/{response._maxPlayers}");
        OnServerFoundEvent.Invoke(response);
#endif
    }
    public int GetBroadcastPort() => serverBroadcastListenPort;
    public IPAddress GetIPAddressPort() => IPAddress.Broadcast;

    /// <summary>
    /// �}�V���̃��[�J��IPv4�A�h���X���擾
    /// </summary>
    /// <returns>LAN IP�A�h���X�B������Ȃ��ꍇ�� "127.0.0.1" ��Ԃ�</returns>
    public static string GetLocalIPv4()
    {
        try
        {
            // �߂�l�̌����C�[�T�l�b�g�AWi-Fi�̏��ŗD�悷��
            string candidateIp = null;

            // �S�Ẵl�b�g���[�N�C���^�[�t�F�[�X�i�A�_�v�^�[�j���擾
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // �C���^�[�t�F�[�X�����p�\�ŁA���[�v�o�b�N�łȂ����Ƃ��m�F
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // �C���^�[�t�F�[�X��Description�i�����j�ɉ��z�A�_�v�^���L�̕����񂪊܂܂�Ă��Ȃ����`�F�b�N
                    // "VMware", "VirtualBox", "TAP", "Teredo" �Ȃǂ����O
                    if (ni.Description.Contains("VMware") || ni.Description.Contains("Virtual") ||
                        ni.Description.Contains("TAP") || ni.Description.Contains("Teredo"))
                    {
                        continue; // ���z�A�_�v�^�[�Ȃ�X�L�b�v
                    }

                    // ���̃C���^�[�t�F�[�X�Ɋ��蓖�Ă��Ă���IP�A�h���X�����擾
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        // IPv4�A�h���X�݂̂�ΏۂƂ���
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            // �ŏ��̌�����������ێ�
                            if (candidateIp == null)
                            {
                                candidateIp = ip.Address.ToString();
                            }

                            // �C�[�T�l�b�g�i�L��LAN�j������������A������ŗD�悵�ă��[�v�𔲂���
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            // �C�[�T�l�b�g��������Ȃ������ꍇ�A�ێ����Ă��������iWi-Fi�Ȃǁj��Ԃ�
            return candidateIp ?? "127.0.0.1";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"���[�J��IP�A�h���X�̎擾�Ɏ��s���܂���: {ex.Message}");
            return "127.0.0.1";
        }
    }
}
