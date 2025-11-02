using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
public class BattleSceneManager : SceneManagerBase
{
    //protected override IEnumerator Start()
    //{
    //    Debug.Log($"[Client] BattleSceneManager: START");
    //    // localPlayerが設定されるまで1フレーム待つ
    //    while (NetworkClient.localPlayer == null)
    //    {
    //        yield return null;
    //    }
    //    // このシーンがロードされたら、自分のPlayerStateオブジェクトを探して
    //    // サーバーに準備完了を報告するコマンドを呼び出す
    //    if (NetworkClient.localPlayer != null)
    //    {
    //        Debug.Log($"[Client] discovery : NetworkClient.localPlayer");
    //        PlayerState playerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
    //        if (playerState != null)
    //        {
    //            Debug.Log($"[Client] Send:PlayerReady");
    //            playerState.CmdPlayerReadyInBattle();
    //        }
    //    }
    //}
    protected override void InitializeScene()
    {
        Debug.Log($"[{thisScene}] 初期化を開始します。");
        if (NetworkClient.localPlayer != null && localPlayerState != null)
        {
            Debug.Log($"[{thisScene}] discovery : NetworkClient.localPlayer");
            if (localPlayerState != null)
            {
                Debug.Log($"[{thisScene}] Send:PlayerReady");
                localPlayerState.CmdPlayerReadyInBattle();
            }
        }
    }
}