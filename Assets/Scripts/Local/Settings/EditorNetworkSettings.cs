using UnityEngine;
// Resources/Editor/ フォルダを指定しています
[CreateAssetMenu(fileName = "Editor.Network.asset", menuName = "Game/Editor Network Settings")]
public class EditorNetworkSettings : ScriptableObject
{
    [Header("エディタでネットワーク環境を使用する場合、サーバ情報を設定")]
    [Tooltip("IPv4")]
    public string serverIPv4 = "";
    [Tooltip("Port")]
    public ushort serverPort = 0;
}