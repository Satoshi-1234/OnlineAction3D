using UnityEngine;
// Resources/Editor/ フォルダを指定しています
[CreateAssetMenu(fileName = "MySceneSettings.local.asset", menuName = "Game/Scene Settings")]
public class LocalSceneSettings : ScriptableObject
{
    [Tooltip("ローカル環境でのみ使用する次シーン設定")]
    public GameScene nextSceneRequest = GameScene.Home;
    [Header("↑”Debug”を使用する場合のみアドレス入力必須！")]
    [Tooltip("ローカル環境でのみ使用する次シーンアドレス")]
    public string nextSceneAddress = GameScene.Home.ToString();
}