//”z—ñ‚ÌElement–¼‚ðEnum‚Ì—v‘f‚É‚·‚éEditorŠg’£

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumArrayAttribute))]
public sealed class EnumArrayDrawer : ArrayElementLabelDrawerBase<EnumArrayAttribute>
{
	protected override string[] GetNames(EnumArrayAttribute attr) => attr.names;
}


#endif
