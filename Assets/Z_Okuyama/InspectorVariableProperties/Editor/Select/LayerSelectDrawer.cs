//String型でLayer名をプルダウン選択するEditor拡張(※正直LayerMaskで良い)

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using InspectorVariableProperties.Attributes;
using InspectorVariableProperties.Editor.Base;

namespace InspectorVariableProperties.Editor.Select
{
	[CustomPropertyDrawer(typeof(InspectorVariableProperties.Attributes.LayerSelectAttribute), true)]
	[CustomPropertyDrawer(typeof(IVP.LayerSelectAttribute), true)]
	public sealed class LayerSelectDrawer : StringSelectDrawerBase<LayerSelectAttribute>
	{
		const string kFallBackLayer = "Default";
		protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
		{
			options = InspectorVariableProperties.Editor.Internal.EditorOptionsCache.GetLayerNames();
			emptyMessage = "No layer";
			return true;
		}


		protected override string DrawAndPick(Rect position, GUIContent label, string current, string[] options)
		{
			int currentIndex = LayerMask.NameToLayer(current);
			if (currentIndex < 0) currentIndex = 0;

			//LayerField使用
			int pickedIndex = EditorGUI.LayerField(position, label, currentIndex);

			var pickedName = LayerMask.LayerToName(pickedIndex);
			//未命名レイヤーの場合
			if (string.IsNullOrEmpty(pickedName))
			{
				pickedName = $"Layer {pickedIndex}";
			}
			return pickedName;
		}

		protected override string GetFallback(string[] options)
		{
			//FallBackLayerが存在する場合はそれを優先、なければ先頭を返す
			if (options != null && System.Array.IndexOf(options, kFallBackLayer) >= 0)
			{
				return kFallBackLayer;
			}
			return base.GetFallback(options);
		}
	}
}
#endif
