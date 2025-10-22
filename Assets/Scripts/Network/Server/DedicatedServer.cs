﻿using Mirror;
using UnityEngine;
public class DedicatedServer : MonoBehaviour
{
#if UNITY_SERVER
    void Awake()
    {
        Debug.Log("[DedicatedServer] Headless server starting...");
        var nm = NetworkManager.singleton;

        if (nm != null)
        {
            var transport = nm.transport as kcp2k.KcpTransport;
            if (transport != null)
            {
                // 例: 7000〜8000 の範囲からランダムにポートを決定
                System.Random rand = new System.Random();
                transport.Port = (ushort)rand.Next(7000, 8000);

                Debug.Log($"[Server] Set Port: {transport.Port}");
            }

            nm.StartServer();
            // 広告開始
            var discovery = gameObject.AddComponent<LanDiscovery>();
            discovery.AdvertiseServer();
            Debug.Log($"[ServerDiscovery] Port: {discovery.GetBroadcastPort()}");
        }
        else
        {
            Debug.LogError("[DedicatedServer] NetworkManager not found!");
        }
    }
#endif
}