using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerState : NetworkBehaviour
{
    public GameObject characterInstance;
    // ���̃L�����N�^�[���ǂ̎����ɑ����Ă��邩������ID
    [SyncVar]
    public uint matchId = 0;
    // [SyncVar]�ɂ��A���̒l�̓T�[�o�[����S�N���C�A���g�Ɏ������������
    [SyncVar]
    public int selectedCharacterId = 0; // 0: A�L����, 1: B�L����, ...
    public override void OnStartClient()
    {
        // ���̃I�u�W�F�N�g���������ꂽ��A�V�[�����܂����ł��j������Ȃ��悤�ɐݒ肷��
        Debug.Log($"[DEBUG/CLIENT] PlayerState.OnStartClient");
        DontDestroyOnLoad(this.gameObject);
    }
#if !UNITY_SERVER
    private void Awake()
    {
        Debug.Log($"[DEBUG/CLIENT] PlayerState.Awake - Generate player object�BisLocalPlayer: {isLocalPlayer}");
    }

    private void OnDestroy()
    {
        Debug.Log($"[DEBUG/CLIENT] PlayerState.OnDestroy - Destroy player object");
    }
#endif

    // [Command]����: �N���C�A���g����T�[�o�[�֑����閽��
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
        // ���̃R�[�h�̓T�[�o�[��ł̂ݎ��s����邽�߁A���S�ɌĂяo����
        ServerGameManager.Instance.SpawnCharacterForPlayer(connectionToClient, selectedCharacterId);
    }
}