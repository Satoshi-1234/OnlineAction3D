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

		string _format = "{name}_{n:D2}";
		int _startNumber = 1;
		bool _applyToAssets = true;
		bool _applyToObjects = true;


		//Function==================================================

		//ï`âÊ
		public override void OnGUI()
		{
			EditorGUILayout.LabelField("Batch Rename", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope("box"))
			{
				_format = EditorGUILayout.TextField(new GUIContent("Format"), _format);
				_startNumber = EditorGUILayout.IntField(new GUIContent("Start Number"), Mathf.Max(0, _startNumber));
				using (new EditorGUILayout.HorizontalScope())
				{
					_applyToObjects = EditorGUILayout.ToggleLeft("Scene Objects", _applyToObjects, GUILayout.Width(130));
					_applyToAssets = EditorGUILayout.ToggleLeft("Assets", _applyToAssets, GUILayout.Width(100));
				}
			}

			HLine();

			var selection = Selection.objects;
			if (selection.Length == 0)
			{
				EditorGUILayout.HelpBox("RenameëŒè€ÇëIëÇµÇƒÇ≠ÇæÇ≥Ç¢", MessageType.None);
				return;
			}

			EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
			var list = selection.ToList();

			int n = _startNumber;
			foreach (var o in list)
			{
				string preview = BuildName(o, n);
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.ObjectField(o, typeof(Object), true);
					EditorGUILayout.LabelField("Å® " + preview);
				}
				n++;
			}

			EditorGUILayout.Space();
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Rename", GUILayout.Width(120), GUILayout.Height(24)))
				{
					ApplyRename(list);
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
						AssetDatabase.RenameAsset(path, newName);
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
