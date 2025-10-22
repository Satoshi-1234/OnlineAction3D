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

// Mirror の NetworkDiscoveryBase を継承nse>
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
            // endpointは直接保持できないのでstring化
            _address = GetLocalIPv4(),
            _port = transport.Port,
            //_port = (nm.transport as kcp2k.KcpTransport)?.Port ?? 7777,
            // CustomNetworkManager の識別情報をここで流用
            _gameId = (nm.authenticator as CustomNetworkManager)?.gameId ?? "Unknown",
            _version = (nm.authenticator as CustomNetworkManager)?.version ?? "0.0.0",
            _playerCount = nm.numPlayers,
            _maxPlayers = nm.maxConnections,
            _serverName = System.Environment.MachineName // PC名をサーバー名に設定
        };
#else
        Debug.Log($"[Server] Get ProcessRequest：NotServer");
        return new DiscoveryResponse();
#endif
    }
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
#if !UNITY_SERVER || UNITY_EDITOR
        Debug.Log($"[LAN] サーバー検出: {response._serverName} ({response._gameId} v{response._version}) " +
                  $"Players {response._playerCount}/{response._maxPlayers}");
        OnServerFoundEvent.Invoke(response);
#endif
    }
    public int GetBroadcastPort() => serverBroadcastListenPort;
    public IPAddress GetIPAddressPort() => IPAddress.Broadcast;

    /// <summary>
    /// マシンのローカルIPv4アドレスを取得
    /// </summary>
    /// <returns>LAN IPアドレス。見つからない場合は "127.0.0.1" を返す</returns>
    public static string GetLocalIPv4()
    {
        try
        {
            // 戻り値の候補をイーサネット、Wi-Fiの順で優先する
            string candidateIp = null;

            // 全てのネットワークインターフェース（アダプター）を取得
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // インターフェースが利用可能で、ループバックでないことを確認
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // インターフェースのDescription（説明）に仮想アダプタ特有の文字列が含まれていないかチェック
                    // "VMware", "VirtualBox", "TAP", "Teredo" などを除外
                    if (ni.Description.Contains("VMware") || ni.Description.Contains("Virtual") ||
                        ni.Description.Contains("TAP") || ni.Description.Contains("Teredo"))
                    {
                        continue; // 仮想アダプターならスキップ
                    }

                    // このインターフェースに割り当てられているIPアドレス情報を取得
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        // IPv4アドレスのみを対象とする
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            // 最初の候補を見つけたら保持
                            if (candidateIp == null)
                            {
                                candidateIp = ip.Address.ToString();
                            }

                            // イーサネット（有線LAN）が見つかったら、それを最優先してループを抜ける
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            // イーサネットが見つからなかった場合、保持しておいた候補（Wi-Fiなど）を返す
            return candidateIp ?? "127.0.0.1";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ローカルIPアドレスの取得に失敗しました: {ex.Message}");
            return "127.0.0.1";
        }
    }
}
