using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	public abstract class ToolTabBase : IToolTab
	{
		public abstract string Id { get; }
		public abstract string Title { get; }
		public virtual Texture Icon => null;

		protected EditorWindow Host;

		public virtual void OnEnable(EditorWindow host) => Host = host;
		public virtual void OnDisable() { }
		public abstract void OnGUI();

		protected void HLine(float height = 1f)
		{
			var rect = EditorGUILayout.GetControlRect(false, height);
			rect.height = height;
			EditorGUI.DrawRect(
				rect,
				EditorGUIUtility.isProSkin
					? new Color(1, 1, 1, 0.15f)
					: new Color(0, 0, 0, 0.2f)
			);
		}
	}
}
