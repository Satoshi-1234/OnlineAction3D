//String型でScene名をプルダウン選択するEditor拡張

#if UNITY_EDITOR
using UnityEditor;

using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Select
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.SceneNameSelectAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.SceneNameSelectAttribute), true)]
	public sealed class SceneNameSelectDrawer : StringSelectDrawerBase<SceneNameSelectAttribute>
	{
		protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
		{
			options = InspectorVariableProperties.Editor.Internal.EditorOptionsCache.GetBuildSceneNames();
			emptyMessage = "No scene";
			return true;
		}

	}
}
#endif
