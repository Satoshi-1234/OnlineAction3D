using UnityEngine;
using Mirror;
using System.Collections;
public class BattleSceneManager : SceneManagerBase
{
    IEnumerator Start()
    {
        Debug.Log($"[Client] BattleSceneManager: START");
        // localPlayer���ݒ肳���܂�1�t���[���҂�
        while (NetworkClient.localPlayer == null)
        {
            yield return null;
        }
        // ���̃V�[�������[�h���ꂽ��A������PlayerState�I�u�W�F�N�g��T����
        // �T�[�o�[�ɏ���������񍐂���R�}���h���Ăяo��
        if (NetworkClient.localPlayer != null)
        {
            Debug.Log($"[Client] discovery : NetworkClient.localPlayer");
            PlayerState playerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
            if (playerState != null)
            {
                Debug.Log($"[Client] Send:PlayerReady");
                playerState.CmdPlayerReadyInBattle();
            }
        }
    }
}