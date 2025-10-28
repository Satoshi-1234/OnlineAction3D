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
    [Header("シーン遷移設定")]
    public GameScene nextSceneRequest = GameScene.Home; // インスペクターで次のシーンを設定

    protected void RequestSceneTransition(GameScene scene)
    {
        // ClientGameManagerにサーバーへのシーン遷移リクエストを依頼
        ClientGameManager.Instance.RequestServerSceneChange(scene);
    }

    // Addressablesからシーンをロードする共通メソッドなどもここに実装できる
    protected IEnumerator LoadSceneAddressable(GameScene scene)
    {
        if(!ClientGameManager.Instance.GetInitialized())
        {
            Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
            yield break;
        }
        string sceneLabel = scene.ToString(); // Enum名をラベルとして使う
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneLabel, LoadSceneMode.Additive, true); // またはAdditive
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"{sceneLabel} のロード完了");
            NetworkClient.Send(new ClientSceneReadyRequest());
        }
        else
        {
            Debug.LogError($"{sceneLabel} のロード失敗");
        }
    }

    // 各シーン固有の初期化処理などは派生クラスで実装
    protected abstract void InitializeScene();
}
