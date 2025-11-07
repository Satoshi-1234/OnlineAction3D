//”z—ñ‚ÌElement–¼‚ðTag–¼‚É‚·‚éEditorŠg’£

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(TagArrayAttribute))]
public sealed class TagArrayDrawer : ArrayElementLabelDrawerBase<TagArrayAttribute>
{	
	protected override string[] GetNames(TagArrayAttribute attr)
	{
		var tags = InternalEditorUtility.tags;
		if (tags == null || tags.Length == 0)
		{
			return System.Array.Empty<string>();
		}
		return tags;
	}
}
#endif
