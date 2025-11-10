//String型でTag名をプルダウン選択するEditor拡張

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Select
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.TagSelectAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.TagSelectAttribute), true)]
	public sealed class TagSelectDrawer : StringSelectDrawerBase<TagSelectAttribute>
	{
		const string kFallBackTag = "Untagged";

		protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
		{
			options = InspectorVariableProperties.Editor.Internal.EditorOptionsCache.GetTags();
			emptyMessage = "No tag";
			return true;
		}


		protected override string DrawAndPick(Rect position, GUIContent label, string current, string[] options)
		{
			//TagField使用
			var picked = EditorGUI.TagField(position, label, current);
			return picked;
		}

		protected override string GetFallback(string[] options)
		{
			//FallBackTagが存在する場合はそれを優先、なければ先頭を返す
			if (options != null && System.Array.IndexOf(options, kFallBackTag) >= 0)
			{
				return kFallBackTag;
			}
			return base.GetFallback(options);
		}
	}
}
#endif
