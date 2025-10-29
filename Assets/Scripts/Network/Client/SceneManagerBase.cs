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
    [Header("シーン遷移設定")]
    public GameScene nextSceneRequest = GameScene.Home; // インスペクターで次のシーンを設定

    //アドレス (Address): 各アセットに付けられた文字列（通常はアセットのパス、例: "Assets/Scenes/BattleStage.unity"）
    [Header("DebugSceneAddress")]
    public string nextSceneAddress = GameScene.Home.ToString();

    protected void RequestSceneTransition()
    {
        // ClientGameManagerにサーバーへのシーン遷移リクエストを依頼
        ClientGameManager.Instance.RequestServerSceneChange(nextSceneRequest, nextSceneAddress);
    }

    // Addressablesからシーンをロードする共通メソッドなどもここに実装できる
    //protected IEnumerator LoadSceneAddressable(GameScene scene)
    //{
    //    if(!ClientGameManager.Instance.GetInitialized())
    //    {
    //        Debug.LogWarning($"[Client] ClientGameManager Not GetInitialized");
    //        yield break;
    //    }
    //    string sceneLabel = scene.ToString(); // Enum名をラベルとして使う
    //    AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneLabel, LoadSceneMode.Additive, true); // またはAdditive
    //    yield return handle;
    //    if (handle.Status == AsyncOperationStatus.Succeeded)
    //    {
    //        Debug.Log($"{sceneLabel} のロード完了");
    //        NetworkClient.Send(new ClientSceneReadyRequest());
    //    }
    //    else
    //    {
    //        Debug.LogError($"{sceneLabel} のロード失敗");
    //    }
    //}

    // 各シーン固有の初期化処理などは派生クラスで実装
    //protected abstract void InitializeScene();
}
