using Mirror;

public struct HandshakeResponse : NetworkMessage
{
    public bool _accepted;
    public string _message;
}