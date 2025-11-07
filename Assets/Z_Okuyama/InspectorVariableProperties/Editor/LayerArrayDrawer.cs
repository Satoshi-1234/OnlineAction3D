//”z—ñ‚ÌElement–¼‚ðLayer–¼‚É‚·‚éEditorŠg’£

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(LayerArrayAttribute))]
public sealed class LayerArrayDrawer : ArrayElementLabelDrawerBase<LayerArrayAttribute>
{
	protected override string[] GetNames(LayerArrayAttribute attr) => attr.names;
}
#endif
