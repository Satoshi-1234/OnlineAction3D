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

		static class Ui
		{
			public static class Width
			{
				public const float ResetButton = 72f;
				public const float ActionButton = 60f;
				public const float StarButton = 28f;
				public const float PingButton = 50f;
			}
			
			public static class Height
			{
				public const float Space = 8f;
			}

			public static class Text
			{
				public const string NoBookmarks = "ブックマークはありません";
				public const string NoHistory = "履歴はありません";
				public const string BookmarkStarLight = "★";
				public const string BookmarkStarDark = "☆";
			}
		}

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
				using (new EditorGUILayout.VerticalScope("box"))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField($"Objects (Max = {SelectionHistoryState.MaxItems})", EditorStyles.miniBoldLabel);
						GUILayout.FlexibleSpace();
						using (new EditorGUI.DisabledScope(SelectionHistoryState.instance.ObjectHistory.Count == 0))
						{
							if (GUILayout.Button("Reset", GUILayout.Width(Ui.Width.ResetButton)))
							{
								SelectionHistoryState.instance.ClearObjectHistory();
								return;
							}
						}
					}
					DrawObjectList(SelectionHistoryState.instance.ObjectHistory.ToList(), isBookmarkList: false);
				}
				EditorGUILayout.Space(Ui.Height.Space);

				//Assets
				using (new EditorGUILayout.VerticalScope("box"))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField($"Assets (Max = {SelectionHistoryState.MaxItems})", EditorStyles.miniBoldLabel);
						GUILayout.FlexibleSpace();
						using (new EditorGUI.DisabledScope(SelectionHistoryState.instance.AssetHistory.Count == 0))
						{
							if (GUILayout.Button("Reset", GUILayout.Width(Ui.Width.ResetButton)))
							{
								SelectionHistoryState.instance.ClearAssetHistory();
								return;
							}
						}
					}
					DrawAssetList(SelectionHistoryState.instance.AssetHistory.ToList(), isBookmarkList: false);
				}
				EditorGUILayout.Space(Ui.Height.Space);
				HLine();

				//Bookmarks
				using (new EditorGUILayout.VerticalScope("box"))
				{
					EditorGUILayout.LabelField("Object Bookmarks", EditorStyles.miniBoldLabel);
					DrawObjectList(SelectionHistoryState.instance.ObjectBookmarks.ToList(), isBookmarkList: true);
				}
				EditorGUILayout.Space(Ui.Height.Space);

				using (new EditorGUILayout.VerticalScope("box"))
				{
					EditorGUILayout.LabelField("Assets Bookmarks", EditorStyles.miniBoldLabel);
					DrawAssetList(SelectionHistoryState.instance.AssetBookmarks.ToList(), isBookmarkList: true);
				}
			}
		}

		//描画==================================================
		void DrawObjectList(List<string> objectIdList, bool isBookmarkList)
		{
			//何もない時
			if (objectIdList.Count == 0)
			{
				EditorGUILayout.HelpBox(isBookmarkList ? Ui.Text.NoBookmarks : Ui.Text.NoHistory, MessageType.None);
				return;
			}

			foreach (var id in objectIdList)
			{
				if (!GlobalObjectId.TryParse(id, out var gid)) continue;
				var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
				var go = obj as GameObject;

				using (new EditorGUILayout.HorizontalScope("box"))
				{
					EditorGUILayout.ObjectField(go, typeof(GameObject), true);

					//ブックマークによって変化
					bool isBookmarked = go != null && SelectionHistoryState.instance.IsBookmarked(go);
					var starLabel = new GUIContent(isBookmarked ? Ui.Text.BookmarkStarLight : Ui.Text.BookmarkStarDark);
					if (GUILayout.Button(starLabel, GUILayout.Width(Ui.Width.StarButton)) && go != null)
					{
						SelectionHistoryState.instance.ToggleObjectBookmark(go);
					}

					if (GUILayout.Button("Ping", GUILayout.Width(Ui.Width.PingButton)) && go != null)
					{
						EditorGUIUtility.PingObject(go);
					}

					if (GUILayout.Button("Select", GUILayout.Width(Ui.Width.ActionButton)) && go != null)
					{
						Selection.activeObject = go;
					}
				}
			}
		}

		void DrawAssetList(List<string> assetGuidList, bool isBookmarkList)
		{
			//何もない時
			if (assetGuidList.Count == 0)
			{
				EditorGUILayout.HelpBox(isBookmarkList ? Ui.Text.NoBookmarks : Ui.Text.NoHistory, MessageType.None);
				return;
			}

			foreach (var guid in assetGuidList)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

				using (new EditorGUILayout.HorizontalScope("box"))
				{
					EditorGUILayout.ObjectField(asset, typeof(Object), false);

					bool isBookmarked = asset != null && SelectionHistoryState.instance.IsBookmarked(asset);
					var starLabel = new GUIContent(isBookmarked ? Ui.Text.BookmarkStarLight : Ui.Text.BookmarkStarDark);
					if (GUILayout.Button(starLabel, GUILayout.Width(Ui.Width.StarButton)) && asset != null)
					{
						SelectionHistoryState.instance.ToggleAssetBookmark(asset);
					}

					if (GUILayout.Button("Ping", GUILayout.Width(Ui.Width.PingButton)) && asset != null)
					{
						EditorGUIUtility.PingObject(asset);
					}

					if (GUILayout.Button("Select", GUILayout.Width(Ui.Width.ActionButton)) && asset != null)
					{
						Selection.activeObject = asset;
						FocusProjectAndSelectContainingFolder(guid);
					}
				}
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
