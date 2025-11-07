//配列のElement名をLayer名にするEditor拡張

#if UNITY_EDITOR
using UnityEditor;

using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Array
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.LayerArrayAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.LayerArrayAttribute), true)]
	public sealed class LayerArrayDrawer : ArrayElementLabelDrawerBase<LayerArrayAttribute>
	{
		protected override string[] GetNames(LayerArrayAttribute attr)
		{
			return InspectorVariableProperties.Editor.Internal.EditorOptionsCache.GetLayerNames();
		}
	}
}
#endif
