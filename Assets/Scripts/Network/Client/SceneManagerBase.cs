using UnityEngine;
public enum GameScene 
{ 
    Debug,
    Title, 
    Home, 
    BattleForest, 
    BattleCastle 
}
public abstract class SceneManagerBase : MonoBehaviour
{
    [Header("�V�[���J�ڐݒ�")]
    public GameScene nextSceneRequest = GameScene.Home; // �C���X�y�N�^�[�Ŏ��̃V�[����ݒ�

    //�A�h���X (Address): �e�A�Z�b�g�ɕt����ꂽ������i�ʏ�̓A�Z�b�g�̃p�X�A��: "Assets/Scenes/BattleStage.unity"�j
    [Header("DebugSceneAddress")]
    public string nextSceneAddress = GameScene.Home.ToString();

    protected void RequestSceneTransition()
    {
        // ClientGameManager�ɃT�[�o�[�ւ̃V�[���J�ڃ��N�G�X�g���˗�
        ClientGameManager.Instance.RequestServerSceneChange(nextSceneRequest, nextSceneAddress);
    }

    // Addressables����V�[�������[�h���鋤�ʃ��\�b�h�Ȃǂ������Ɏ����ł���
    //protected IEnumerator LoadSceneAddressable(GameScene scene)
    //{
    //    if(!ClientGameManager.Instance.GetInitialized())
    //    {
    //        Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
    //        yield break;
    //    }
    //    string sceneLabel = scene.ToString(); // Enum�������x���Ƃ��Ďg��
    //    AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneLabel, LoadSceneMode.Additive, true); // �܂���Additive
    //    yield return handle;
    //    if (handle.Status == AsyncOperationStatus.Succeeded)
    //    {
    //        Debug.Log($"{sceneLabel} �̃��[�h����");
    //        NetworkClient.Send(new ClientSceneReadyRequest());
    //    }
    //    else
    //    {
    //        Debug.LogError($"{sceneLabel} �̃��[�h���s");
    //    }
    //}

    // �e�V�[���ŗL�̏����������Ȃǂ͔h���N���X�Ŏ���
    //protected abstract void InitializeScene();
}
