using Mirror;

// クライアントがホームシーンへ遷移する準備ができたことをサーバーに伝える
public struct ClientReadyRequest : NetworkMessage 
{
    public uint _phase;
    public string _StageName;
}
public struct ClientReadyResponse : NetworkMessage 
{
    public bool _isConnect;
}
public struct ClientSelectCharacterRequest : NetworkMessage
{
    public uint _characterId;
}