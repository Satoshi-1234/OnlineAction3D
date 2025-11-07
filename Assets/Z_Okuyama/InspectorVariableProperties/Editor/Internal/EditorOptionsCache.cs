#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InspectorVariableProperties.Editor.Internal
{
	//Editor用の軽量キャッシュ
	[InitializeOnLoad]
	internal static class EditorOptionsCache
	{
		static string[] s_Tags;
		static string[] s_SceneNames;
		static (int id, string name)[] s_Layers;

		static EditorOptionsCache()
		{
			//変更があり得るタイミングでキャッシュを無効化
			EditorApplication.projectChanged += ClearAll;
			EditorBuildSettings.sceneListChanged += ClearScenes;
		}

		public static void ClearAll()
		{
			s_Tags = null;
			s_Layers = default;
			s_SceneNames = null;
		}

		public static void ClearScenes() => s_SceneNames = null;

		//Tags==================================================

		public static string[] GetTags()
		{
			if (s_Tags == null)
			{
				// nullでも安心なフォールバック
				var src = UnityEditorInternal.InternalEditorUtility.tags ?? System.Array.Empty<string>();
				// 外部で弄られても平気なようにコピー
				s_Tags = src.ToArray();
			}
			return s_Tags;
		}

		//Layers==================================================

		public static (int id, string name)[] GetLayers()
		{
			if (s_Layers == default)
			{
				const int kMin = 0;
				const int kMax = 31;
				var arr = new (int, string)[kMax - kMin + 1];
				for (int i = kMin; i <= kMax; i++)
				{
					var nm = LayerMask.LayerToName(i);
					arr[i] = (i, string.IsNullOrEmpty(nm) ? $"Layer {i}" : nm);
				}
				s_Layers = arr;
			}
			return s_Layers;
		}

		public static string[] GetLayerNames() => GetLayers().Select(t => t.name).ToArray();

		//Scenes==================================================

		public static string[] GetBuildSceneNames()
		{
			if (s_SceneNames == null)
			{
				var scenes = EditorBuildSettings.scenes ?? System.Array.Empty<EditorBuildSettingsScene>();
				s_SceneNames = scenes
					.Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
					.Select(s => Path.GetFileNameWithoutExtension(s.path))
					.ToArray();
			}
			return s_SceneNames;
		}
	}
}
#endif
