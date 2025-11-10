#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

using InspectorVariableProperties.Attributes;

namespace InspectorVariableProperties.Editor.Array
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.EnumArrayFromAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.EnumArrayFromAttribute), true)]
	public sealed class EnumArrayFromDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = (EnumArrayFromAttribute)attribute;

			//EnumTypeRef取得
			SerializedProperty typeRefProp = property.serializedObject.FindProperty(attr.enumTypeRefFieldName);
			Type enumType = null;

			if (typeRefProp != null)
			{
				var aqnProp = typeRefProp.FindPropertyRelative("assemblyQualifiedName");
				if (aqnProp != null && !string.IsNullOrEmpty(aqnProp.stringValue))
				{
					//型復元
					enumType = Type.GetType(aqnProp.stringValue);
				}
			}

			//Enum配列取得
			string[] names = (enumType != null && enumType.IsEnum) ? Enum.GetNames(enumType) : null;

			//Elementラベルを差し替え
			int index = TryGetIndex(property);
			if (names != null && index >= 0 && index < names.Length)
			{
				label = new GUIContent(names[index]);
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
					return idx;
			}
			return -1;
		}
	}
}
#endif
