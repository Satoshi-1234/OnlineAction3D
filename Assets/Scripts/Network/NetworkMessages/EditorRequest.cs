using Mirror;

public struct ServerInfoRequest: NetworkMessage
{
}

public struct ServerInfoResponse: NetworkMessage
{
    public string _gameId;
    public string _version;
    public int _playerCount;
    public int _maxPlayers;
    public string _serverName;
}
