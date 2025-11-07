//String型でTag名をプルダウン選択するEditor拡張

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagSelectAttribute))]
public sealed class TagSelectDrawer : StringSelectDrawerBase<TagSelectAttribute>
{
	private const string FallBackTag = "Untagged";

	protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
	{
		options = InternalEditorUtility.tags ?? System.Array.Empty<string>();
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
		if (options != null && System.Array.IndexOf(options, FallBackTag) >= 0) return FallBackTag;
		return base.GetFallback(options);
	}
}
#endif
