using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	class HistoryTab : ToolTabBase
	{

		//Variable==================================================
		public override string Id => "history";
		public override string Title => "History";

		//Layout用
		const float kResetButtonWidth = 72f;
		const float kActionButtonWidth = 60f;
		const float kStarButtonWidth = 28f;
		const float kPingButtonWidth = 50f;

		Vector2 _scroll;


		//Function==================================================

		//Tab系共通部分
		public override void OnGUI()
		{
			EditorGUILayout.LabelField("Selection History and Bookmarks", EditorStyles.boldLabel);

			using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
			{
				_scroll = s.scrollPosition;

				//Objects
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField($"Objects (Max = {SelectionHistoryState.MaxItems})", EditorStyles.miniBoldLabel);
					GUILayout.FlexibleSpace();
					using (new EditorGUI.DisabledScope(SelectionHistoryState.instance.ObjectHistory.Count == 0))
					{
						if (GUILayout.Button("Reset", GUILayout.Width(kResetButtonWidth)))
						{
							ClearObjectHistorySerialized();
							return;
						}
					}
				}
				DrawObjectList(SelectionHistoryState.instance.ObjectHistory.ToList(), isBookmarkList: false);

				EditorGUILayout.Space(8);

				//Assets
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField($"Assets (Max = {SelectionHistoryState.MaxItems})", EditorStyles.miniBoldLabel);
					GUILayout.FlexibleSpace();
					using (new EditorGUI.DisabledScope(SelectionHistoryState.instance.AssetHistory.Count == 0))
					{
						if (GUILayout.Button("Reset", GUILayout.Width(kResetButtonWidth)))
						{
							ClearAssetHistorySerialized();
							return;
						}
					}
				}
				DrawAssetList(SelectionHistoryState.instance.AssetHistory.ToList(), isBookmarkList: false);

				EditorGUILayout.Space(8);
				HLine();

				//Bookmarks
				EditorGUILayout.LabelField("Object Bookmarks", EditorStyles.miniBoldLabel);
				DrawObjectList(SelectionHistoryState.instance.ObjectBookmarks.ToList(), isBookmarkList: true);

				EditorGUILayout.Space(8);
				EditorGUILayout.LabelField("Assets Bookmarks", EditorStyles.miniBoldLabel);
				DrawAssetList(SelectionHistoryState.instance.AssetBookmarks.ToList(), isBookmarkList: true);
			}
		}

		//描画==================================================
		void DrawObjectList(List<string> objectIdList, bool isBookmarkList)
		{
			if (objectIdList.Count == 0)
			{
				EditorGUILayout.HelpBox(isBookmarkList ? "ブックマークはありません" : "履歴はありません", MessageType.None);
				return;
			}

			foreach (var id in objectIdList.ToList())
			{
				if (!GlobalObjectId.TryParse(id, out var gid)) continue;
				var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
				var go = obj as GameObject;

				using (new EditorGUILayout.HorizontalScope("box"))
				{
					EditorGUILayout.ObjectField(go, typeof(GameObject), true);

					//ブックマークによって変化
					bool isBookmarked = go != null && SelectionHistoryState.instance.IsBookmarked(go);
					var starLabel = new GUIContent(isBookmarked ? "★" : "☆");
					if (GUILayout.Button(starLabel, GUILayout.Width(kStarButtonWidth)) && go != null)
					{
						SelectionHistoryState.instance.ToggleObjectBookmark(go);
					}

					if (GUILayout.Button("Ping", GUILayout.Width(kPingButtonWidth)) && go != null)
					{
						EditorGUIUtility.PingObject(go);
					}

					if (GUILayout.Button("Select", GUILayout.Width(kActionButtonWidth)) && go != null)
					{
						Selection.activeObject = go;
					}
				}
			}
		}

		void DrawAssetList(List<string> assetGuidList, bool isBookmarkList)
		{
			if (assetGuidList.Count == 0)
			{
				EditorGUILayout.HelpBox(isBookmarkList ? "ブックマークはありません" : "履歴はありません", MessageType.None);
				return;
			}

			foreach (var guid in assetGuidList.ToList())
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

				using (new EditorGUILayout.HorizontalScope("box"))
				{
					EditorGUILayout.ObjectField(asset, typeof(Object), false);

					bool isBookmarked = asset != null && SelectionHistoryState.instance.IsBookmarked(asset);
					var starLabel = new GUIContent(isBookmarked ? "★" : "☆");
					if (GUILayout.Button(starLabel, GUILayout.Width(kStarButtonWidth)) && asset != null)
					{
						SelectionHistoryState.instance.ToggleAssetBookmark(asset);
					}

					if (GUILayout.Button("Ping", GUILayout.Width(kPingButtonWidth)) && asset != null)
					{
						EditorGUIUtility.PingObject(asset);
					}

					if (GUILayout.Button("Select", GUILayout.Width(kActionButtonWidth)) && asset != null)
					{
						Selection.activeObject = asset;
						FocusProjectAndSelectContainingFolder(guid);
					}
				}
			}
		}

		//クリア
		void ClearObjectHistorySerialized()
		{
			var state = SelectionHistoryState.instance;
			var so = new SerializedObject(state);
			var listProp = so.FindProperty("_objectHistory"); // private フィールド名に一致
			if (listProp != null && listProp.isArray)
			{
				listProp.ClearArray();
				so.ApplyModifiedPropertiesWithoutUndo();
				state.SaveNow();
			}
		}

		void ClearAssetHistorySerialized()
		{
			var state = SelectionHistoryState.instance;
			var so = new SerializedObject(state);
			var listProp = so.FindProperty("_assetHistory"); // private フィールド名に一致
			if (listProp != null && listProp.isArray)
			{
				listProp.ClearArray();
				so.ApplyModifiedPropertiesWithoutUndo();
				state.SaveNow();
			}
		}

		//フォーカス
		void FocusProjectAndSelect(Object obj)
		{
			if (obj == null) return;
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = obj;
			EditorGUIUtility.PingObject(obj);
		}

		void FocusProjectAndSelectContainingFolder(string guid)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path)) { return; }

			if (AssetDatabase.IsValidFolder(path))
			{
				var folder = AssetDatabase.LoadAssetAtPath<Object>(path);
				FocusProjectAndSelect(folder);
				return;
			}

			var dir = System.IO.Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(dir)) { return; }
			dir = dir.Replace("\\", "/");
			var folderObj = AssetDatabase.LoadAssetAtPath<Object>(dir);
			FocusProjectAndSelect(folderObj);
		}
	}
}
