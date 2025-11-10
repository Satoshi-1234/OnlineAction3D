using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DebugTools.EditorUI
{
	class RenameTab : ToolTabBase
	{
		//Variable==================================================
		public override string Id => "rename";
		public override string Title => "Rename";

		static class Ui
		{
			public static class Width
			{
				public const float Inspect = 100f;
				public const float SceneToggle = 100f;
				public const float AssetToggle = 50f;
				public const float ActionButton = 120f;
				public const float PrevNameMin = 100f;
				public const float PrevNameMax = 250f;
				public const float NewNameMin = 150f;
				public const float NewNameMax = 400f;
			}
			public static class Height
			{
				public const float Row = 24f;
				public const float Space = 2f;
			}

			public static class Text
			{
				public const string NoSelect = "Rename対象を選択してください";
			}
		}

		string _format = "{name}_{n:D2}";
		int _startNumber = 1;
		bool _applyToAssets = true;
		bool _applyToObjects = true;


		//Function==================================================

		//描画
		public override void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField("Batch Rename", EditorStyles.boldLabel);

				using (new EditorGUILayout.VerticalScope("box"))
				{
					//フォーマット
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Format", EditorStyles.label, GUILayout.Width(Ui.Width.Inspect));
						_format = EditorGUILayout.TextField(_format);
					}

					//値
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Start Number", EditorStyles.label, GUILayout.Width(Ui.Width.Inspect));
						_startNumber = EditorGUILayout.IntField(Mathf.Max(0, _startNumber));
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Option", EditorStyles.label, GUILayout.Width(Ui.Width.Inspect));
						_applyToObjects = EditorGUILayout.ToggleLeft("Scene Objects", _applyToObjects, EditorStyles.miniLabel, GUILayout.Width(Ui.Width.SceneToggle));
						_applyToAssets = EditorGUILayout.ToggleLeft("Assets", _applyToAssets, EditorStyles.miniLabel, GUILayout.Width(Ui.Width.AssetToggle));
					}
				}
			}

			HLine();

			var selection = Selection.objects;
			if (selection.Length == 0)
			{
				EditorGUILayout.HelpBox(Ui.Text.NoSelect, MessageType.None);
				return;
			}

			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
				var list = selection.ToList();

				int n = _startNumber;

				using (new EditorGUILayout.VerticalScope("box"))
				{
					foreach (var o in list)
					{
						string preview = BuildName(o, n);
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.ObjectField(o, typeof(Object), true, GUILayout.MinWidth(Ui.Width.PrevNameMin), GUILayout.MaxWidth(Ui.Width.PrevNameMax));
							EditorGUILayout.LabelField("→ " + preview, GUILayout.MinWidth(Ui.Width.NewNameMin), GUILayout.MaxWidth(Ui.Width.NewNameMax));
						}
						n++;
					}

					GUILayout.Space(Ui.Height.Space);
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Rename", GUILayout.Width(Ui.Width.ActionButton), GUILayout.Height(Ui.Height.Row)))
					{
						ApplyRename(list);
					}
				}
			}
		}

		string BuildName(Object obj, int n)
		{
			string name = obj.name;
			string parent = (obj is GameObject go && go.transform.parent != null)
				? go.transform.parent.name : "";
			string fmt = _format;

			fmt = Regex.Replace(fmt, @"\{n:(?<spec>[^}]+)\}", m =>
			{
				var spec = m.Groups["spec"].Value;
				return int.TryParse(spec.Trim('D', 'd'), out int nd)
					? n.ToString("D" + nd)
					: n.ToString();
			});

			fmt = fmt.Replace("{n}", n.ToString());
			fmt = fmt.Replace("{name}", name);
			fmt = fmt.Replace("{parent}", parent);

			return fmt;
		}

		//Rename
		void ApplyRename(List<Object> selection)
		{
			Undo.IncrementCurrentGroup();
			int group = Undo.GetCurrentGroup();

			int n = _startNumber;
			foreach (var o in selection)
			{
				if (o is GameObject go && go.scene.IsValid() && _applyToObjects)
				{
					Undo.RecordObject(go, "Rename Object");
					go.name = BuildName(go, n);
					EditorSceneManager.MarkSceneDirty(go.scene);
				}
				else if (_applyToAssets)
				{
					var path = AssetDatabase.GetAssetPath(o);
					if (!string.IsNullOrEmpty(path))
					{
						string newName = BuildName(o, n);
						//空でない場合エラー
						var err = AssetDatabase.RenameAsset(path, newName);
						if (!string.IsNullOrEmpty(err))
						{
							Debug.LogError($"RenameAsset failed: {err} (path: {path}, to: {newName})");
						}
					}
				}
				n++;
			}
			AssetDatabase.SaveAssets();
			Undo.CollapseUndoOperations(group);
			Debug.Log($"Renamed {selection.Count} items.");
		}
	}
}
