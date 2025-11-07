//String型でLayer名をプルダウン選択するEditor拡張(※正直LayerMaskで良い)

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LayerSelectAttribute))]
public sealed class LayerSelectDrawer : StringSelectDrawerBase<LayerSelectAttribute>
{
	private const string FallBackLayer = "Default";

	protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
	{
		//Layerから配列生成
		var names = new System.Collections.Generic.List<string>(32);
		for (int i = 0; i < 32; i++)
		{
			var name = LayerMask.LayerToName(i);
			names.Add(string.IsNullOrEmpty(name) ? $"Layer {i}" : name);
		}
		options = names.ToArray();
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
		if (options != null && System.Array.IndexOf(options, FallBackLayer) >= 0) return FallBackLayer;
		return base.GetFallback(options);
	}
}
#endif
