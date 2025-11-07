#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;


using InspectorVariableProperties.Types;
using InspectorVariableProperties.Attributes;

namespace InspectorVariableProperties.Editor.TypeRef
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Types.EnumTypeRef))]
	[CustomPropertyDrawer(typeof(IVP.EnumTypeRef))]
	public sealed class EnumTypeRefDrawer : PropertyDrawer
	{
		//キャッシュ
		static readonly string kDisplayEmpty = "(none)";
		static readonly (string Display, string Value)[] s_Options = BuildOptions();

		static (string, string)[] BuildOptions()
		{
			var types = TypeCache.GetTypesDerivedFrom<Enum>().Where(t =>
				t.IsEnum &&
				(t.IsDefined(typeof(InspectorVariableProperties.Attributes.ProjectEnumAttribute), false) ||
				 t.IsDefined(typeof(IVP.ProjectEnumAttribute), false))) // ← 追加
				.OrderBy(t => t.FullName).ToArray();
			/*
			var types = TypeCache.GetTypesDerivedFrom<Enum>()
				.Where(t => t != null && t.IsEnum && t.IsDefined(typeof(ProjectEnumAttribute), false))
				.OrderBy(t => t.FullName)
				.ToArray();*/

			//先頭は空
			var arr = new (string, string)[types.Length + 1];
			arr[0] = (kDisplayEmpty, string.Empty);
			for (int i = 0; i < types.Length; i++)
				arr[i + 1] = (types[i].FullName, types[i].AssemblyQualifiedName);

			return arr;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var aqnProp = property.FindPropertyRelative("assemblyQualifiedName");

			int index = 0;
			if (!string.IsNullOrEmpty(aqnProp.stringValue))
			{
				index = System.Array.FindIndex(s_Options, o => o.Value == aqnProp.stringValue);
				if (index < 0) index = 0;
			}

			EditorGUI.BeginProperty(position, label, property);
			int picked = EditorGUI.Popup(position, label.text, index, s_Options.Select(o => o.Display).ToArray());
			if (picked != index)
				aqnProp.stringValue = s_Options[picked].Value;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			=> EditorGUIUtility.singleLineHeight;
	}
}
#endif
