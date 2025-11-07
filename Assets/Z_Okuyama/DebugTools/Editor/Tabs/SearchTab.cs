using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugTools.EditorUI
{
	class SearchTab : ToolTabBase
	{
		//Variable==================================================
		public override string Id => "search";
		public override string Title => "Search";

		static class Ui
		{
			public static class Width
			{
				public const float ModeDropdown = 260f;
				public const float ActiveSceneOnly = 140f;
				public const float IncludeInactive = 125f;
				public const float UnderSelection = 150f;

				public const float SearchButton = 70f;
				public const float ShowOnlyButton = 140f;
				public const float RestoreButton = 130f;
				public const float Hits = 80f;

				public const float ResultMin = 200f;
				public const float ResultMax = 500f;
				public const float LayerAndTag = 150f;
				public const float ShortButton = 50f;
			}
			public static class Height
			{
				public const float Space = 0f;
			}
			public static class Text
			{
				public const string NoSearch = "結果はここに表示されます";
			}
		}

		const int kLayerMin = 0;
		const int kLayerMax = 31;

		enum Mode { Prefab, Component, Tag, Layer }
		Mode _mode = Mode.Component;

		string _query = "";

		//オプション
		bool _includeInactive = true;//非アクティブ含む
		bool _underSelectionOnly = false;//選択配下のみ
		bool _activeSceneOnly = true;// アクティブシーン限定

		Vector2 _scroll;
		readonly List<GameObject> _results = new();


#if UNITY_2020_1_OR_NEWER
		//可視状態のスナップショット
		class VisibilitySnapshot
		{
			public Dictionary<GameObject, bool> Hidden = new();//※true⇒元々非表示
			public Dictionary<GameObject, bool> PickingDisabled = new();
		}
		VisibilitySnapshot _lastSnapshot;
#endif

		//Function==================================================

		//描画
		public override void OnGUI()
		{
			EditorGUILayout.LabelField("Search of Scene Objects", EditorStyles.boldLabel);

			//モード選択
			using (new EditorGUILayout.HorizontalScope())
			{
				_mode = (Mode)EditorGUILayout.EnumPopup(new GUIContent("Mode"), _mode, GUILayout.Width(Ui.Width.ModeDropdown));
				GUILayout.FlexibleSpace();
			}

			//入力
			using (new EditorGUILayout.HorizontalScope())
			{
				_query = EditorGUILayout.TextField(new GUIContent("Query"), _query);
			}

			//オプション
			using (new EditorGUILayout.HorizontalScope())
			{
				_activeSceneOnly = EditorGUILayout.ToggleLeft("Active Scene Only", _activeSceneOnly, GUILayout.Width(Ui.Width.ActiveSceneOnly));
				_includeInactive = EditorGUILayout.ToggleLeft("Include Inactive", _includeInactive, GUILayout.Width(Ui.Width.IncludeInactive));
				_underSelectionOnly = EditorGUILayout.ToggleLeft("Under Selection Only", _underSelectionOnly, GUILayout.Width(Ui.Width.UnderSelection));

			}

			//操作ボタン群
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Search", GUILayout.Width(Ui.Width.SearchButton)))
					RunSearch();

				if (GUILayout.Button("Show Only Results", GUILayout.Width(Ui.Width.ShowOnlyButton)))
					ShowOnlyResults();

				if (GUILayout.Button("Restore Visibility", GUILayout.Width(Ui.Width.RestoreButton)))
					RestoreVisibility();

				EditorGUILayout.LabelField($"Hits: {_results.Count}", GUILayout.Width(Ui.Width.Hits));
			}

			HLine();

			//結果一覧
			using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
			{
				_scroll = s.scrollPosition;
				
				if (_results.Count == 0)
				{
					EditorGUILayout.HelpBox(Ui.Text.NoSearch, MessageType.None);
				}
				else
				{
					foreach (var go in _results)
					{
						if (go == null) continue;

						using (new EditorGUILayout.HorizontalScope("box"))
						{
							using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(Ui.Width.ResultMin), GUILayout.MaxWidth(Ui.Width.ResultMax)))
							{
								EditorGUILayout.ObjectField(go, typeof(GameObject), true);
								GUILayout.Space(Ui.Height.Space);
							}

							using (new EditorGUILayout.VerticalScope())
							{
								using (new EditorGUILayout.HorizontalScope())//1行目
								{
									EditorGUILayout.LabelField($"Layer: {LayerMask.LayerToName(go.layer)}({go.layer})", EditorStyles.miniLabel, GUILayout.Width(Ui.Width.LayerAndTag));
									GUILayout.FlexibleSpace();
									if (GUILayout.Button("Select", GUILayout.Width(Ui.Width.ShortButton)))
										Selection.activeObject = go;
								}

								using (new EditorGUILayout.HorizontalScope())//2行目
								{
									EditorGUILayout.LabelField($"Tag: {go.tag}", EditorStyles.miniLabel, GUILayout.Width(Ui.Width.LayerAndTag));
									GUILayout.FlexibleSpace();
									if (GUILayout.Button("Frame", GUILayout.Width(Ui.Width.ShortButton)))
										FrameGameObject(go);
								}
							}
						}
					}
				}
			}
		}

		//検索==================================================
		void RunSearch()
		{
			_results.Clear();

			Transform scope = null;
			if (_underSelectionOnly && Selection.activeTransform != null)
				scope = Selection.activeTransform;

			var candidates = EnumerateScene(_includeInactive, scope, _activeSceneOnly);

			switch (_mode)
			{
				case Mode.Prefab:
					{
						if (string.IsNullOrWhiteSpace(_query)) break;
						var q = _query.Trim();
						_results.AddRange(candidates.Where(g =>
							g.name.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0));
						break;
					}

				case Mode.Component:
					{
						if (string.IsNullOrWhiteSpace(_query)) break;
						var q = _query.Trim();
						_results.AddRange(candidates.Where(g => HasComponentByName(g, q)));
						break;
					}

				case Mode.Tag:
					{
						if (string.IsNullOrWhiteSpace(_query)) break;
						var q = _query.Trim();

						//部分一致
						_results.AddRange(candidates.Where(g =>
						{
							string tag = string.IsNullOrEmpty(g.tag) ? "Untagged" : g.tag;
							return tag.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0;
						}));
						break;
					}

				case Mode.Layer:
					{
						if (string.IsNullOrWhiteSpace(_query)) break;

						string q = _query.Trim();
						int? layerNum = null;

						//数値チェック
						if (int.TryParse(q, out int parsed))
						{
							if (parsed >= kLayerMin && parsed <= kLayerMax)
							{
								layerNum = parsed;
							}
						}

						//Layer名
						int layerByName = LayerMask.NameToLayer(q);
						if (layerByName >= 0)
							layerNum = layerNum ?? layerByName;

						if (layerNum.HasValue)
						{
							int layer = layerNum.Value;
							_results.AddRange(candidates.Where(g => g.layer == layer));
						}
						else
						{
							//部分一致
							_results.AddRange(candidates.Where(g =>
							{
								string layerName = LayerMask.LayerToName(g.layer);
								return layerName.IndexOf(q, System.StringComparison.OrdinalIgnoreCase) >= 0;
							}));
						}

						break;
					}
			}
		}

		//GameObject列挙==================================================
		IEnumerable<GameObject> EnumerateScene(bool includeInactive, Transform scope, bool activeSceneOnly)
		{
			if (scope != null)
			{
				foreach (var t in scope.GetComponentsInChildren<Transform>(includeInactive))
				{
					var go = t.gameObject;
					if (FilterByActiveScene(go, activeSceneOnly))
						yield return go;
				}
				yield break;
			}

			if (activeSceneOnly)
			{
				var scene = SceneManager.GetActiveScene();
				foreach (var root in scene.GetRootGameObjects())
				{
					foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive))
					{
						yield return t.gameObject;
					}
				}
			}
			else
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					var sc = SceneManager.GetSceneAt(i);
					if (!sc.IsValid() || !sc.isLoaded) continue;

					foreach (var root in sc.GetRootGameObjects())
					{
						foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive))
						{
							yield return t.gameObject;
						}
					}
				}
			}
		}

		bool FilterByActiveScene(GameObject go, bool activeSceneOnly)
		{
			if (!activeSceneOnly) { return true; }
			var s = go.scene;
			return s.IsValid() && s == SceneManager.GetActiveScene();
		}

		bool HasComponentByName(GameObject go, string query)
		{
			var comps = go.GetComponents<Component>();
			for (int i = 0; i < comps.Length; i++)
			{
				var c = comps[i];
				if (c == null) { continue; }
				var t = c.GetType();
				var name = t.Name;
				var full = t.FullName ?? name;

				if (name.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0) { return true; }
				if (full.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0) { return true; }
			}
			return false;
		}
		//SceneViewでフレーム
		void FrameGameObject(GameObject go)
		{
			if (go == null) { return; }
			Selection.activeObject = go;
			var sv = SceneView.lastActiveSceneView;
			if (sv != null)
			{
				sv.FrameSelected();
				sv.Focus();
			}
		}

		//可視制御==================================================
		void ShowOnlyResults()
		{
#if UNITY_2020_1_OR_NEWER
			var svm = SceneVisibilityManager.instance;

			//状態保存
			_lastSnapshot = TakeVisibilitySnapshot();

			//一旦全部隠した後表示
			svm.HideAll();
			foreach (var go in _results.Where(r => r != null))
			{
				svm.Show(go, true);
			}

			svm.EnableAllPicking();
#endif
		}

		void RestoreVisibility()
		{
#if UNITY_2020_1_OR_NEWER
			if (_lastSnapshot == null)
			{
				//スナップショットが無い場合は全表示
				var svm = SceneVisibilityManager.instance;
				svm.ShowAll();
				svm.EnableAllPicking();
				return;
			}

			var svm2 = SceneVisibilityManager.instance;

			//一旦全表示
			svm2.ShowAll();
			svm2.EnableAllPicking();

			//隠れていたものを隠す
			foreach (var kv in _lastSnapshot.Hidden)
			{
				var go = kv.Key;
				if (go == null) continue;
				if (kv.Value) svm2.Hide(go, true);
			}
			//ピッキング不可
			foreach (var kv in _lastSnapshot.PickingDisabled)
			{
				var go = kv.Key;
				if (go == null) continue;
				if (kv.Value) svm2.DisablePicking(go, true);
			}

			_lastSnapshot = null;
#endif
		}

#if UNITY_2020_1_OR_NEWER
		VisibilitySnapshot TakeVisibilitySnapshot()
		{
			var snap = new VisibilitySnapshot();
			var svm = SceneVisibilityManager.instance;

			//状態保存
			Transform scope = null;
			if (_underSelectionOnly && Selection.activeTransform != null)
				scope = Selection.activeTransform;

			foreach (var go in EnumerateScene(true, scope, _activeSceneOnly))
			{
				if (go == null) { continue; }
				//重複キー避け
				if (!snap.Hidden.ContainsKey(go))
				{
					snap.Hidden.Add(go, svm.IsHidden(go, true));
				}
				if (!snap.PickingDisabled.ContainsKey(go))
				{
					snap.PickingDisabled.Add(go, svm.IsPickingDisabled(go, true));
				}
			}
			return snap;
		}

#endif
	}
}
