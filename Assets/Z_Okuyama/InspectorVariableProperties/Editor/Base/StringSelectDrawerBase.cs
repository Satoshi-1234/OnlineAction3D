//String型をプルダウンで選択するEditor拡張の基底クラス

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace InspectorVariableProperties.Editor.Base
{
	public abstract class StringSelectDrawerBase<TAttr> : PropertyDrawer where TAttr : PropertyAttribute
	{
		private const string kMsgStringOnly = "String only";
		private const string kMsgNoOptions = "No options";

		public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			//string型以外の場合
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, kMsgStringOnly);
				EditorGUI.EndProperty();
				return;
			}

			//候補取得、空の場合
			if (!TryGetOptions(property, out var options, out var emptyMessage) || options == null || options.Length == 0)
			{
				EditorGUI.LabelField(position, label.text, string.IsNullOrEmpty(emptyMessage) ? kMsgNoOptions : emptyMessage);
				EditorGUI.EndProperty();
				return;
			}

			//存在しない場合
			var current = property.stringValue;
			if (string.IsNullOrEmpty(current) || System.Array.IndexOf(options, current) < 0)
			{
				current = GetFallback(options);
			}

			//複数選択
			EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

			EditorGUI.BeginChangeCheck();
			var picked = DrawAndPick(position, label, current, options);
			var changed = EditorGUI.EndChangeCheck();

			//変更があった場合
			if (changed)
			{
				if (System.Array.IndexOf(options, picked) < 0)
				{
					picked = GetFallback(options);
				}
				if (property.stringValue != picked || property.hasMultipleDifferentValues)
				{
					property.stringValue = picked;
				}
			}

			EditorGUI.showMixedValue = false;
			EditorGUI.EndProperty();
		}

		//候補取得
		protected abstract bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage);

		//描画
		protected virtual string DrawAndPick(Rect position, GUIContent label, string current, string[] options)
		{
			int index = Mathf.Max(0, System.Array.IndexOf(options, current));
			int selected = EditorGUI.Popup(position, label.text, index, options);
			if (0 <= selected && selected < options.Length) return options[selected];
			return GetFallback(options);
		}

		//フォールバック
		protected virtual string GetFallback(string[] options)
		{
			return (options != null && options.Length > 0) ? options[0] : string.Empty;
		}
	}
}
#endif
