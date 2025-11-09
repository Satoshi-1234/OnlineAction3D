using UnityEngine;
// Resources/Editor/ フォルダを指定しています
[CreateAssetMenu(fileName = "MySceneSettings.local.asset", menuName = "Game/Scene Settings")]
public class LocalSceneSettings : ScriptableObject
{
    [Header("このアセット自体は .gitignore で管理対象外にします")]
    [Tooltip("ローカル環境でのみ使用する次シーン設定")]
    public GameScene nextSceneRequest = GameScene.Home;

    [Tooltip("ローカル環境でのみ使用する次シーンアドレス")]
    public string nextSceneAddress = GameScene.Home.ToString();
}