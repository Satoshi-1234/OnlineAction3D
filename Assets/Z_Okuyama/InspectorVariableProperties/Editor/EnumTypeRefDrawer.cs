#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumTypeRef))]
public sealed class EnumTypeRefDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var aqnProp = property.FindPropertyRelative("assemblyQualifiedName");

		//[ProjectEnum]‚ÌenumŽæ“¾
		var enumTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<Enum>()
			.Where(t => t != null && t.IsEnum && t.IsDefined(typeof(ProjectEnumAttribute), false))
			.OrderBy(t => t.FullName)
			.ToArray();

		var values = enumTypes.Select(t => t.AssemblyQualifiedName).Prepend(string.Empty).ToArray();//ŽÀÛ‚Ì’l
		var displays = enumTypes.Select(t => t.FullName).Prepend("\u00A0").ToArray();//Œ©‚½–Ú‚Ì’l

		int index = string.IsNullOrEmpty(aqnProp.stringValue)
			? 0
			: Array.IndexOf(values, aqnProp.stringValue);
		if (index < 0)
		{
			index = 0;
		}

		EditorGUI.BeginProperty(position, label, property);
		int picked = EditorGUI.Popup(position, label.text, index, displays);
		if (picked != index)
		{
			aqnProp.stringValue = values[picked];
		}
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		=> EditorGUIUtility.singleLineHeight;
}
#endif
