using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerState : NetworkBehaviour
{
    public GameObject characterInstance;
    // このキャラクターがどの試合に属しているかを示すID
    [SyncVar]
    public uint matchId = 0;
    // [SyncVar]により、この値はサーバーから全クライアントに自動同期される
    [SyncVar]
    public int selectedCharacterId = 0; // 0: Aキャラ, 1: Bキャラ, ...
    public override void OnStartClient()
    {
        // このオブジェクトが生成されたら、シーンをまたいでも破棄されないように設定する
        Debug.Log($"[DEBUG/CLIENT] PlayerState.OnStartClient");
        DontDestroyOnLoad(this.gameObject);
    }
#if !UNITY_SERVER
    private void Awake()
    {
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

    [Command]
    public void CmdPlayerReadyInBattle()
    {
        Debug.Log($"[Server-Command]: Player {connectionToClient.connectionId} Set BattleScene");
        // このコードはサーバー上でのみ実行されるため、安全に呼び出せる
        ServerGameManager.Instance.SpawnCharacterForPlayer(connectionToClient, selectedCharacterId);
    }
}