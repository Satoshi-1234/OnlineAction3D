using System.Collections.Generic;
using System.Linq;
using Mirror;

// InterestManagementBaseを継承することで、NetworkManagerのAOI(興味範囲)ルールとして機能する
public class MatchInterestManagement : InterestManagementBase
{
    // [Server]
    // このオブジェクトを誰が見るべきかを判定する「ルールブック」
    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        // 観察者(クライアント)のプレイヤーオブジェクトがなければ、何も見せない
        if (newObserver.identity == null) return false;

        // チェック対象のオブジェクトがPlayerConnectionの場合
        if (identity.TryGetComponent<PlayerState>(out var targetPlayerState))
        {
            // PlayerConnectionは、その所有者自身にしか見えない
            return targetPlayerState.connectionToClient == newObserver;
        }

        // チェック対象のオブジェクトがキャラクターの場合
        if (identity.TryGetComponent<NetworkPlayerController>(out var targetCharacter))
        {
            // 観察者(クライアント)もキャラクターでなければ、見せない
            if (!newObserver.identity.TryGetComponent<NetworkPlayerController>(out var observerCharacter))
                return false;

            // 試合中のキャラクターは、同じ試合IDを持つ人にだけ見せる
            return targetCharacter.matchId == observerCharacter.matchId && targetCharacter.matchId != 0;
        }

        // 上記以外のネットワークオブジェクトは、デフォルトで見える
        return true;
    }

    // [Server]
    // オブジェクトの現在の観察者リストを再構築する「アクション」
    public override void Rebuild(NetworkIdentity identity, bool initialize)
    {
        // 新しい観察者リストを作成
        HashSet<NetworkConnectionToClient> newObservers = new HashSet<NetworkConnectionToClient>();

        // 接続している全てのクライアントをチェックし、ルールブック(OnCheckObserver)に従ってリストに追加
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.identity != null && OnCheckObserver(identity, conn))
            {
                newObservers.Add(conn);
            }
        }

        // 古いリストと新しいリストを比較し、差分を適用する

        // 新しく見えるようになった人を探す
        foreach (var newObserver in newObservers)
        {
            if (!identity.observers.ContainsKey(newObserver.connectionId))
            {
                // リストに追加し、スポーンメッセージを送信
                AddObserver(newObserver, identity);
            }
        }

        // 見えなくなった人を探す
        List<NetworkConnection> oldObservers = new List<NetworkConnection>(identity.observers.Values);
        foreach (var oldObserver in oldObservers)
        {
            if (!newObservers.Contains(oldObserver))
            {
                // リストから削除し、非表示メッセージを送信
                RemoveObserver(oldObserver as NetworkConnectionToClient, identity);
            }
        }
    }
}