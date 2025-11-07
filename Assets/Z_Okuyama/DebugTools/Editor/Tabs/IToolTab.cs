using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	public interface IToolTab
	{
		string Id { get; }//タブID
		string Title { get; }//ラベル
		Texture Icon { get; }//アイコン

		void OnEnable(EditorWindow host);//ホストに乗ったタイミング
		void OnGUI();//描画
		void OnDisable();//購読解除など
	}
}
