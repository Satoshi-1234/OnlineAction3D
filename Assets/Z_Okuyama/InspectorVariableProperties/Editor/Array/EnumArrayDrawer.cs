//配列のElement名をEnumの要素にするEditor拡張

#if UNITY_EDITOR
using UnityEditor;
using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Array
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.EnumArrayAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.EnumArrayAttribute), true)]
	public sealed class EnumArrayDrawer : ArrayElementLabelDrawerBase<EnumArrayAttribute>
	{
		protected override string[] GetNames(EnumArrayAttribute attr) => attr.names;
	}
}

#endif
