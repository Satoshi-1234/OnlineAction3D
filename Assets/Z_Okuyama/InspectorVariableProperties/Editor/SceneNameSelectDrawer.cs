//String型でScene名をプルダウン選択するEditor拡張

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomPropertyDrawer(typeof(SceneNameSelectAttribute))]
public sealed class SceneNameSelectDrawer : StringSelectDrawerBase<SceneNameSelectAttribute>
{
	protected override bool TryGetOptions(SerializedProperty property, out string[] options, out string emptyMessage)
	{
		var count = SceneManager.sceneCountInBuildSettings;
		options = Enumerable.Range(0, count)
			.Select(i => System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)))
			.ToArray();

		emptyMessage = "No scene";
		return true;
	}
}
#endif
