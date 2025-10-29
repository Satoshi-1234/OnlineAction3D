using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;
public class HomeSceneManager : SceneManagerBase
{
    private int selectCharacterId = 0;      // �L�����N�^�[ID
    private bool matchConnect = false;      // �}�b�`���O�J�n�t���O
    private PlayerState playerState = null; // �v���C���[�ݒ�
    [Header("�L�����N�^�[�P�{�^��")]
    public Button selectCharacter1;         // �ڑ��{�^����R�Â���
    [Header("�L�����N�^�[�Q�{�^��")]
    public Button selectCharacter2;         // �z�[���ֈڍs����{�^����R�Â���
    [Header("�}�b�`���O�J�n�{�^��")]
    public Button startMatch;               // �}�b�`���O�J�n�{�^����R�Â���

    // Start��IEnumerator�ɕύX
    private IEnumerator Start()
    {
        // ������ �v���C���[�I�u�W�F�N�g�����������̂�҂� ������
        yield return GetPlayerState();

        if (startMatch != null)
        {
            Debug.Log($"[Client/Home] Start Success");
            startMatch.GetComponentInChildren<TMP_Text>().text = "�o�g���J�n";
        }
        UpdateButtonInteractble();
    }

    // IEnumerator�ɕύX���APlayerState���擾�ł���܂őҋ@����悤�ɏC��
    private IEnumerator GetPlayerState()
    {
        // localPlayer��null�̊ԁA�T�[�o�[�Ƀv���C���[�ǉ���v����������
        while (!NetworkClient.ready)
        {
            // �������ł��Ă��Ȃ��Ԃ́A�����������̃t���[����҂�
            yield return null;
        }

        // 2. �ڑ��̏������ł�����A�����v���C���[�I�u�W�F�N�g���܂��Ȃ���΁A��x�����v������
        if (NetworkClient.localPlayer == null)
        {
            Debug.Log("Connection is ready. Requesting player object from server...");
            NetworkClient.AddPlayer();
        }

        // 3. �T�[�o�[���v���C���[�I�u�W�F�N�g�����蓖�Ă�̂�҂�
        while (NetworkClient.localPlayer == null)
        {
            Debug.Log("Waiting for server to assign player object...");
            yield return new WaitForSeconds(0.1f); // ���O����ʂɏo�Ȃ��悤�ɏ����҂�
        }

        // localPlayer������������AplayerState�ɃL���b�V������
        playerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
        Debug.Log("PlayerState�̎擾�ɐ������܂����I");
    }

    //private void GetPlayerState()
    //{
    //    if (NetworkClient.localPlayer != null && playerState == null)
    //    {
    //        playerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
    //        return;
    //    }
    //    if (playerState == null) Debug.LogWarning($"NetworkClient.localPlayer is Null");
    //}

    /// <summary>
    /// �L�����N�^�[�P�{�^���������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    /// </summary>
    public void SelectCharacter1()
    {
        selectCharacterId = 0;
        UpdateButtonInteractble();
    }
    /// <summary>
    /// �L�����N�^�[�Q�{�^���������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    /// </summary>
    public void SelectCharacter2()
    {
        selectCharacterId = 1;
        UpdateButtonInteractble();
    }

    /// <summary>
    /// �L�����N�^�[�Q�{�^���������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    /// </summary>
    public void StartMatching()
    {
        if (startMatch != null && !matchConnect)
        {
            matchConnect = true;
            //ClientGameManager.Instance.SetSceneToServer("BattleScene");
            RequestSceneTransition();
            startMatch.GetComponentInChildren<TMP_Text>().text = "�L�����Z��";
        }
        else if(startMatch != null)
        {
            matchConnect = false;
            startMatch.GetComponentInChildren<TMP_Text>().text = "�o�g���J�n";
        }
    }

    /// <summary>
    /// �{�^���������ꂽ�ۂɊ������E�񊈐����̐�����s�����\�b�h
    /// </summary>
    private void UpdateButtonInteractble()
    {
        selectCharacter1.interactable = selectCharacterId != 0;
        selectCharacter2.interactable = selectCharacterId != 1;
        GetPlayerState();
        if (playerState != null)
        {
            playerState.CmdSetCharacter(selectCharacterId);
            Debug.Log($"[Client] UpdateButtonInteractble");
        }    
    }
}
