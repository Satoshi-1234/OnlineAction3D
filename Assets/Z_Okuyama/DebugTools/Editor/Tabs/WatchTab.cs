using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	//WatchTab==================================================
	public sealed class WatchTab : ToolTabBase
	{
		//Variable==================================================
		public override string Id => "watch";
		public override string Title => "Watch";

		static class Ui
		{
			public static class Width
			{
				public const float TopButton = 80f;
				public const float MemberLabelMin = 100f;
				public const float MemberLabelMax = 250f;
				public const float WatchValueFieldMin = 150f;
				public const float ButtonMini = 29f;
				public const float ButtonAction = 58f;
			}
			public static class Height
			{
				public const float WatchListMax = 200f;
				public const float Space = 2f;
			}
			public static class Text
			{
				public const string Null = "<null>";
				public const string Error = "<err>";
				public const string Missing = "(missing)";
				public const string Nodata = "No data";

				public const string NoWatch = "何もWatchしていません";
				public const string NoSelect = "何も選択されていません";
				public const string NoDebugVariable = "[DebugVariable]がありません";
				public const string UpdateMark = "↻";
				public const string DeleteMark = "✕";
			}
		}

		//表示用
		Vector2 _scrollWatch;
		Vector2 _scrollList;
		bool _suppressPersist;

		//Watch
		readonly List<WatchedMemberRef> _watch = new();
		readonly HashSet<string> _watchKeySet = new(); // 重複防止キー


		//Function==================================================
		string MakeKey(in WatchedMemberRef r)
			=> $"{r.ownerGlobalId}|{r.componentTypeName}|{r.memberName}";



		//Tab共通部分
		public override void OnEnable(EditorWindow host)
		{
			base.OnEnable(host);
			RestoreFromStore();
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			Selection.selectionChanged += RepaintHost;
		}

		public override void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			Selection.selectionChanged -= RepaintHost;
		}


		//状態遷移
		void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
			{
				RestoreFromStore();
			}
		}

		void RepaintHost() { Host?.Repaint(); }


		//描画用
		public override void OnGUI()
		{
			EditorGUILayout.LabelField("Watch of Variables", EditorStyles.boldLabel);

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Refresh", GUILayout.MinWidth(Ui.Width.TopButton)))
				{
					RefreshAll();
				}
				using (new EditorGUI.DisabledScope(_watch.Count == 0))
				{
					if (GUILayout.Button("Reset", GUILayout.MinWidth(Ui.Width.TopButton)))
					{
						ClearWatch();
					}
				}
			}

			DrawWatchList();
			HLine();
			DrawSelectableList();
		}


		void DrawWatchList()
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField($"WatchList (Max = {WatchStore.MaxItems})", EditorStyles.boldLabel);

				if (_watch.Count == 0)
				{
					EditorGUILayout.HelpBox(Ui.Text.NoWatch, MessageType.None);
					return;
				}

				using var sv = new EditorGUILayout.ScrollViewScope(_scrollWatch, GUILayout.MaxHeight(Ui.Height.WatchListMax));
				_scrollWatch = sv.scrollPosition;

				//グループ化(描画のみ)
				foreach (var byGo in _watch.GroupBy(w => w._ownerGo ? w._ownerGo.name : Ui.Text.Missing))
				{
					using (new EditorGUILayout.VerticalScope("box"))
					{
						EditorGUILayout.LabelField(byGo.Key, EditorStyles.label);
						using (new EditorGUI.IndentLevelScope())
						{
							foreach (var byComp in byGo.GroupBy(w => w._component != null ? w._component.GetType().Name : Ui.Text.Nodata))
							{
								EditorGUILayout.LabelField(byComp.Key, EditorStyles.miniBoldLabel);
								using (new EditorGUI.IndentLevelScope())
								{
									foreach (var r in byComp)
									{
										using (new EditorGUILayout.HorizontalScope())
										{
											//変数名
											EditorGUILayout.LabelField(r.memberName, GUILayout.MinWidth(Ui.Width.MemberLabelMin), GUILayout.MaxWidth(Ui.Width.MemberLabelMax));

											//値
											string val = r.IsResolved ? SafeGetValue(r._getter) : Ui.Text.Null;
											EditorGUILayout.LabelField(val, EditorStyles.textField, GUILayout.MinWidth(Ui.Width.WatchValueFieldMin));

											//最新化
											using (new EditorGUI.DisabledScope(!r.IsResolved))
											{
												if (GUILayout.Button(new GUIContent(Ui.Text.UpdateMark),
													EditorStyles.miniButton, GUILayout.Width(Ui.Width.ButtonMini)))
												{
													MoveToHead(r);
												}
											}

											//解除
											if (GUILayout.Button(new GUIContent(Ui.Text.DeleteMark),
												EditorStyles.miniButton, GUILayout.Width(Ui.Width.ButtonMini)))
											{
												RemoveWatch(r);
												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		static string SafeGetValue(Func<object> getter)
		{
			try
			{
				var v = getter?.Invoke();
				return v switch
				{
					null => Ui.Text.Null,
					UnityEngine.Object uo when uo == null => Ui.Text.Null,
					_ => v.ToString()
				};
			}
			catch { return Ui.Text.Error; }
		}

		void MoveToHead(WatchedMemberRef r)
		{
			string key = MakeKey(r);
			int idx = _watch.FindIndex(x => MakeKey(x) == key);
			if (idx <= 0) { return; }
			_watch.RemoveAt(idx);
			_watch.Insert(0, r);
			PersistNow();
		}


		void DrawSelectableList()
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{

				EditorGUILayout.LabelField("DebugVariableList", EditorStyles.boldLabel);

				GameObject go = Selection.activeGameObject;
				if (go == null)
				{
					EditorGUILayout.HelpBox(Ui.Text.NoSelect, MessageType.None);
					return;
				}

				//[DebugVariable]抽出
				var entries = BuildDebugVariableEntries(go);

				using var sv = new EditorGUILayout.ScrollViewScope(_scrollList);
				_scrollList = sv.scrollPosition;

				if (entries.Length <= 0)
				{
					EditorGUILayout.HelpBox("[DebugVariable]がありません", MessageType.None);
					return;
				}

				foreach (var byGo in entries.GroupBy(e => e.owner))
				{
					using (new EditorGUILayout.VerticalScope("box"))
					{
						// ObjectName
						EditorGUILayout.LabelField(byGo.Key.name, EditorStyles.label);
						using (new EditorGUI.IndentLevelScope())
						{
							foreach (var byComp in byGo.GroupBy(e => e.component.GetType().Name))
							{
								// ComponentName
								EditorGUILayout.LabelField(byComp.Key, EditorStyles.miniBoldLabel);
								using (new EditorGUI.IndentLevelScope())
								{
									foreach (var e in byComp)
									{
										foreach (var (memberName, getter) in e.members)
										{
											using (new EditorGUILayout.HorizontalScope())
											{
												EditorGUILayout.LabelField(memberName,
													GUILayout.MinWidth(Ui.Width.MemberLabelMin),
													GUILayout.MaxWidth(Ui.Width.MemberLabelMax));

												var preview = SafeGetValue(getter);
												EditorGUILayout.LabelField(preview, EditorStyles.textField,
													GUILayout.MinWidth(Ui.Width.WatchValueFieldMin));

												var tmpRef = MakeRef(e.owner, e.component, memberName);
												var key = MakeKey(tmpRef);
												var isWatched = _watchKeySet.Contains(key);

												if (!isWatched)
												{
													if (GUILayout.Button("Watch", GUILayout.Width(Ui.Width.ButtonAction)))
													{
														AddWatch(tmpRef);
													}
												}
												else
												{
													if (GUILayout.Button("Remove", GUILayout.Width(Ui.Width.ButtonAction)))
													{
														RemoveByKey(key);
													}
												}
											}
										}
									}
								}
							}
						}
					}
					EditorGUILayout.Space(Ui.Height.Space);
				}

			}
		}

		//データ構築
		(GameObject owner, Component component, List<(string member, Func<object> getter)> members)[] BuildDebugVariableEntries(GameObject go)
		{
			var list = new List<(GameObject, Component, List<(string, Func<object>)>)>();
			foreach (var comp in go.GetComponents<Component>())
			{
				if (comp == null) continue;

				var members = new List<(string, Func<object>)>();
				var type = comp.GetType();

				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

				//Field
				foreach (var f in type.GetFields(flags))
				{
					if (!Attribute.IsDefined(f, typeof(DebugVariableAttribute), inherit: true)) { continue; }
					members.Add((f.Name, () => f.GetValue(comp)));
				}
				//Property
				foreach (var p in type.GetProperties(flags))
				{
					if (!Attribute.IsDefined(p, typeof(DebugVariableAttribute), inherit: true)) { continue; }
					var get = p.GetGetMethod(true);
					if (get == null || get.GetParameters().Length != 0) { continue; }
					members.Add((p.Name, () => get.Invoke(comp, null)));
				}

				if (members.Count > 0)
				{
					list.Add((go, comp, members));
				}
			}
			return list.ToArray();
		}

		WatchedMemberRef MakeRef(GameObject go, Component comp, string member)
		{
			WatchStoreUtil.TryMakeGlobalId(go, out var gid);
			return new WatchedMemberRef
			{
				ownerGlobalId = gid,
				ownerHierarchyPath = WatchStoreUtil.BuildHierarchyPath(go), // ← 追加
				componentTypeName = comp.GetType().AssemblyQualifiedName,
				memberName = member
			};
		}


		//追加
		void AddWatch(WatchedMemberRef r)
		{
			r.TryResolve();
			string key = MakeKey(r);
			if (_watchKeySet.Contains(key))
			{
				MoveToHead(r);
				return;
			}

			_watch.Insert(0, r);
			_watchKeySet.Add(key);

			//超過分は削る
			while (_watch.Count > WatchStore.MaxItems)
			{
				var tail = _watch[^1];
				_watchKeySet.Remove(MakeKey(tail));
				_watch.RemoveAt(_watch.Count - 1);
			}
			PersistNow();
		}

		//削除
		void RemoveWatch(WatchedMemberRef r)
		{
			RemoveByKey(MakeKey(r));
		}

		void RemoveByKey(string key)
		{
			int idx = _watch.FindIndex(x => MakeKey(x) == key);
			if (idx >= 0)
			{
				_watch.RemoveAt(idx);
			}
			_watchKeySet.Remove(key);
			PersistNow();
		}

		void ClearWatch()
		{
			_watch.Clear();
			_watchKeySet.Clear();
			PersistNow();
		}

		void RefreshAll()
		{
			foreach (var r in _watch) r.TryResolve();
			RepaintHost();
		}


		void PersistNow()
		{
			if (_suppressPersist) return;
			WatchStore.instance.SetFromRefs(_watch);
		}

		void RestoreFromStore()
		{
			var store = WatchStore.instance;
			_watch.Clear();
			_watchKeySet.Clear();

			if (store.items == null || store.items.Count == 0)
			{
				RepaintHost();
				return;
			}

			//スナップショット
			_suppressPersist = true;
			try
			{
				foreach (var it in store.items.ToList())
				{
					var r = new WatchedMemberRef
					{
						ownerGlobalId = it.ownerGlobalId,
						ownerHierarchyPath = it.ownerHierarchyPath,
						componentTypeName = it.componentTypeName,
						memberName = it.memberName,
					};
					r.TryResolve();
					AddWatch(r);

				}
			}
			finally
			{
				_suppressPersist = false;
			}
			RepaintHost();
		}

	}
}
