using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Mirror;
public enum GameScene 
{ 
    Title, 
    Home, 
    BattleForest, 
    BattleCastle 
}
public abstract class SceneManagerBase : MonoBehaviour
{
    [Header("�V�[���J�ڐݒ�")]
    public GameScene nextSceneRequest = GameScene.Home; // �C���X�y�N�^�[�Ŏ��̃V�[����ݒ�

    protected void RequestSceneTransition(GameScene scene)
    {
        // ClientGameManager�ɃT�[�o�[�ւ̃V�[���J�ڃ��N�G�X�g���˗�
        ClientGameManager.Instance.RequestServerSceneChange(scene);
    }

    // Addressables����V�[�������[�h���鋤�ʃ��\�b�h�Ȃǂ������Ɏ����ł���
    protected IEnumerator LoadSceneAddressable(GameScene scene)
    {
        if(!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
            yield break;
        }
        string sceneLabel = scene.ToString(); // Enum�������x���Ƃ��Ďg��
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneLabel, LoadSceneMode.Additive, true); // �܂���Additive
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"{sceneLabel} �̃��[�h����");
            NetworkClient.Send(new ClientSceneReadyRequest());
        }
        else
        {
            Debug.LogError($"{sceneLabel} �̃��[�h���s");
        }
    }

    // �e�V�[���ŗL�̏����������Ȃǂ͔h���N���X�Ŏ���
    protected abstract void InitializeScene();
}
