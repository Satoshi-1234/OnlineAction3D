#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(EnumArrayFromAttribute))]
public sealed class EnumArrayFromDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var attr = (EnumArrayFromAttribute)attribute;

		//EnumTypeRef‚©‚çTypeŽæ“¾
		var typeRefProp = property.serializedObject.FindProperty(attr.enumTypeRefFieldName);
		Type enumType = null;
		if (typeRefProp != null)
		{
			var aqnProp = typeRefProp.FindPropertyRelative("assemblyQualifiedName");
			if (aqnProp != null && !string.IsNullOrEmpty(aqnProp.stringValue))
			{
				enumType = Type.GetType(aqnProp.stringValue);
			}
		}

		//Enum–¼ƒŠƒXƒg
		string[] names = (enumType != null && enumType.IsEnum) ? Enum.GetNames(enumType) : null;

		int index = TryGetIndex(property);
		if (names != null && 0 <= index && index < names.Length)
		{
			label.text = names[index]; //Element–¼•ÏX
		}

		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.PropertyField(position, property, label, includeChildren: true);
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		=> EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

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
