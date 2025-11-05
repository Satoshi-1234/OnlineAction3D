using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
public class HomeSceneManager : SceneManagerBase
{
    private int selectCharacterId = 0;      // キャラクターID
    private bool matchConnect = false;      // マッチング開始フラグ
    //private PlayerState playerState = null; // プレイヤー設定
    [Header("キャラクター１ボタン")]
    public Button selectCharacter1;         // 接続ボタンを紐づける
    [Header("キャラクター２ボタン")]
    public Button selectCharacter2;         // ホームへ移行するボタンを紐づける
    [Header("マッチング開始ボタン")]
    public Button startMatch;               // マッチング開始ボタンを紐づける

    /// <summary>
    /// 【Homeシーン固有の処理】
    /// プレイヤー準備ロジックをオーバーライド
    /// </summary>
    protected override IEnumerator EnsurePlayerIsReady()
    {
        // Homeシーンは、localPlayer がまだ存在しないはず
        if (NetworkClient.localPlayer != null)
        {
            Debug.LogWarning("[Client-Home] 既に localPlayer が存在します。");
            localPlayerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
            yield break;
        }
        // サーバーにプレイヤー ("魂") の生成を要求
        Debug.Log("[Client-Home] サーバーに Player オブジェクトの生成を要求します。");
        NetworkClient.AddPlayer();
        // サーバーが localPlayer を割り当てるのを待つ
        while (NetworkClient.localPlayer == null)
        {
            Debug.Log("[Client-Home] サーバーが Player オブジェクトを割り当てるのを待機中...");
            yield return new WaitForSeconds(0.1f); // ログ連打防止
        }

        // 基底クラスの localPlayerState にキャッシュ
        localPlayerState = NetworkClient.localPlayer.GetComponent<PlayerState>();
        Debug.Log("[Client-Home] PlayerStateの取得に成功しました！");
    }

    /// <summary>
    /// 【Homeシーン固有の処理】
    /// シーンの初期化（UIの有効化など）
    /// </summary>
    protected override void InitializeScene()
    {
        Debug.Log($"[Client/Home] UIの初期化を開始します。");
        if (startMatch != null)
        {
            startMatch.GetComponentInChildren<TMP_Text>().text = "バトル開始";
        }
        UpdateButtonInteractble();
    }

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
        if (localPlayerState != null)
        {
            localPlayerState.CmdSetCharacter(selectCharacterId);
            Debug.Log($"[Client] UpdateButtonInteractble");
        }
        else
        {
            // InitializeScene より前に呼ばれる可能性があるため Warning に変更
            Debug.LogWarning("[Client-Home] UpdateButtonInteractble 呼び出し時に localPlayerState が null です。");
        }
    }
}
