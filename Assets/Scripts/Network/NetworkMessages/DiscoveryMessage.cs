using Mirror;
using System;
using System.Net;

public struct DiscoveryRequest : NetworkMessage
{
}

public struct DiscoveryResponse : NetworkMessage
{
    public Uri _uri;
    public string _address;
    public int _port;
    // ’Ç‰Áî•ñ
    public string _gameId;
    public string _version;
    public int _playerCount;
    public int _maxPlayers;
    public string _serverName;
}
