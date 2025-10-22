using Mirror;

public struct HandshakeRequest : NetworkMessage
{
    public string _gameId;
    public string _version;
}