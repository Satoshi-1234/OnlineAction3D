//配列のElement名を変更するEditor拡張の基底クラス

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public abstract class ArrayElementLabelDrawerBase<TAttr> : PropertyDrawer where TAttr : PropertyAttribute
{
	protected abstract string[] GetNames(TAttr attr);

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var names = GetNames((TAttr)attribute);
		int index = TryGetIndex(property);

		if (names != null && index >= 0 && index < names.Length)
		{
			label.text = names[index];
		}

		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.PropertyField(position, property, label, includeChildren: true);
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
	}

	private static int TryGetIndex(SerializedProperty property)
	{
		var tokens = property.propertyPath.Split('[', ']');
		for (int i = tokens.Length - 1; i >= 0; i--)
		{
			if (int.TryParse(tokens[i], out int idx))
			{
				return idx;
			}
		}
		return -1;
	}
}
#endif
