using UnityEngine;
using Mirror;

// プレイヤーの現在のステータス
public enum PlayerStatus
{
    Connected,  // 接続完了（Homeシーン）
    InQueue,    // マッチング待機中
    InBattle    // バトル中
}

public class PlayerState : NetworkBehaviour
{
    public GameObject characterInstance;
    // このキャラクターがどの試合に属しているかを示すID
    [SyncVar]
    public uint matchId = 0;
    // [SyncVar]により、この値はサーバーから全クライアントに自動同期される
    [SyncVar]
    public int selectedCharacterId = 0; // 0: Aキャラ, 1: Bキャラ, ...

    // [SyncVar] このプレイヤーの現在のステータス
    [SyncVar]
    public PlayerStatus status = PlayerStatus.Connected;

    // [SyncVar] このプレイヤーが現在いるシーン
    [SyncVar]
    public GameScene currentScene = GameScene.Home;
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[DEBUG/CLIENT] PlayerState.OnStartClient");
    }
    /// <summary>
    /// オブジェクトの「所有権」がクライアントに与えられた瞬間に呼ばれます
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log($"[Client-PlayerState] OnStartAuthority: netId={netId}. 所有権が与えられました。");
    }

    /// <summary>
    /// オブジェクトが「ローカルプレイヤー」として設定された瞬間に呼ばれます
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[Client-PlayerState] OnStartLocalPlayer: netId={netId}. ★★★ローカルプレイヤーとして設定されました★★★");
    }
#if !UNITY_SERVER
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        Debug.Log($"[DEBUG/CLIENT] PlayerState.Awake - Generate player object。isLocalPlayer: {isLocalPlayer}");
    }

    private void OnDestroy()
    {
        Debug.Log($"[DEBUG/CLIENT] PlayerState.OnDestroy - Destroy player object");
    }
#endif

    // [Command]属性: クライアントからサーバーへ送られる命令
    [Command]
    public void CmdSetCharacter(int characterId)
    {
        selectedCharacterId = characterId;
        Debug.Log($"[Server-Command] Player : {connectionToClient.connectionId} - CharacterID : {characterId} Selected");
    }

    //[Command]
    //public void CmdPlayerReadyInBattle()
    //{
    //    //Debug.Log($"[Server-Command]: Player {connectionToClient.connectionId} Set BattleScene");
    //    // このコードはサーバー上でのみ実行されるため、安全に呼び出せる
    //    ServerGameManager.Instance.SpawnCharacterForPlayer(connectionToClient, selectedCharacterId);
    //}
    [Command]
    public void CmdPlayerReadyInBattle()
    {
        Debug.Log($"[Server-Command]: Player {connectionToClient.connectionId} Set BattleScene");
        // このコードはサーバー上でのみ実行されるため、安全に呼び出せる

        // ★修正： matchId と characterId をこのオブジェクトから渡す
        ServerGameManager.Instance.SpawnCharacterForPlayer(connectionToClient, selectedCharacterId, this.matchId);
    }
    [Command]
    public void CmdFindMatch()
    {
        // サーバー側のServerGameManagerにマッチングを要求
        ServerGameManager.Instance.AddPlayerToMatchmakingQueue(this);
    }

    [Command]
    public void CmdCancelMatch()
    {
        // サーバー側のServerGameManagerにキャンセルを要求
        ServerGameManager.Instance.RemovePlayerFromMatchmakingQueue(this);
    }
}