using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;
public class HomeSceneManager : SceneManagerBase
{
    private int selectCharacterId = 0;      // キャラクターID
    private bool matchConnect = false;      // マッチング開始フラグ
    private PlayerState playerState = null; // プレイヤー設定
    [Header("キャラクター１ボタン")]
    public Button selectCharacter1;         // 接続ボタンを紐づける
    [Header("キャラクター２ボタン")]
    public Button selectCharacter2;         // ホームへ移行するボタンを紐づける
    [Header("マッチング開始ボタン")]
    public Button startMatch;               // マッチング開始ボタンを紐づける

    // StartをIEnumeratorに変更
    private IEnumerator Start()
    {
        // ★★★ プレイヤーオブジェクトが準備されるのを待つ ★★★
        yield return GetPlayerState();

        if (startMatch != null)
        {
            Debug.Log($"[Client/Home] Start Success");
            startMatch.GetComponentInChildren<TMP_Text>().text = "バトル開始";
        }
        UpdateButtonInteractble();
    }

    // IEnumeratorに変更し、PlayerStateが取得できるまで待機するように修正
    private IEnumerator GetPlayerState()
    {
        // localPlayerがnullの間、サーバーにプレイヤー追加を要求し続ける
        while (!NetworkClient.ready)
        {
            // 準備ができていない間は、何もせず次のフレームを待つ
            yield return null;
        }

        // 2. 接続の準備ができた後、もしプレイヤーオブジェクトがまだなければ、一度だけ要求する
        if (NetworkClient.localPlayer == null)
        {
            Debug.Log("Connection is ready. Requesting player object from server...");
            NetworkClient.AddPlayer();
        }

        // 3. サーバーがプレイヤーオブジェクトを割り当てるのを待つ
        while (NetworkClient.localPlayer == null)
        {
            Debug.Log("Waiting for server to assign player object...");
            yield return new WaitForSeconds(0.1f); // ログが大量に出ないように少し待つ
        }

        // localPlayerが見つかったら、playerStateにキャッシュする
        playerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
        Debug.Log("PlayerStateの取得に成功しました！");
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
    /// キャラクター１ボタンが押されたときに呼び出されるメソッド
    /// </summary>
    public void SelectCharacter1()
    {
        selectCharacterId = 0;
        UpdateButtonInteractble();
    }
    /// <summary>
    /// キャラクター２ボタンが押されたときに呼び出されるメソッド
    /// </summary>
    public void SelectCharacter2()
    {
        selectCharacterId = 1;
        UpdateButtonInteractble();
    }

    /// <summary>
    /// キャラクター２ボタンが押されたときに呼び出されるメソッド
    /// </summary>
    public void StartMatching()
    {
        if (startMatch != null && !matchConnect)
        {
            matchConnect = true;
            //ClientGameManager.Instance.SetSceneToServer("BattleScene");
            RequestSceneTransition();
            startMatch.GetComponentInChildren<TMP_Text>().text = "キャンセル";
        }
        else if(startMatch != null)
        {
            matchConnect = false;
            startMatch.GetComponentInChildren<TMP_Text>().text = "バトル開始";
        }
    }

    /// <summary>
    /// ボタンが押された際に活性化・非活性化の制御を行うメソッド
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
