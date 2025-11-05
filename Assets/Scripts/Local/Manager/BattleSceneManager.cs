using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
public class BattleSceneManager : SceneManagerBase
{
    protected override void InitializeScene()
    {
        Debug.Log($"[{thisScene}] èâä˙âªÇäJénÇµÇ‹Ç∑ÅB");
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