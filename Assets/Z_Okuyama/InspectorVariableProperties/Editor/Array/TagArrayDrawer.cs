//配列のElement名をTag名にするEditor拡張

#if UNITY_EDITOR
using UnityEditor;

using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Array
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.TagArrayAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.TagArrayAttribute), true)]
	public sealed class TagArrayDrawer : ArrayElementLabelDrawerBase<TagArrayAttribute>
	{
		protected override string[] GetNames(TagArrayAttribute attr)
		{
			return InspectorVariableProperties.Editor.Internal.EditorOptionsCache.GetTags();
		}
	}
}
#endif
