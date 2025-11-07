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
				if (GUILayout.Button("Refresh", GUILayout.MinWidth(80)))
				{
					RefreshAll();
				}
				using (new EditorGUI.DisabledScope(_watch.Count == 0))
				{
					if (GUILayout.Button("Reset", GUILayout.MinWidth(80)))
					{
						ClearWatch();
					}
				}
			}

			DrawWatchList();
			HLine(1);
			DrawSelectableList();
		}


		void DrawWatchList()
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField($"WatchList (Max = {WatchStore.MaxItems})", EditorStyles.boldLabel);

				if (_watch.Count == 0)
				{
					EditorGUILayout.HelpBox("何もWatchしていません", MessageType.None);
					return;
				}

				using var sv = new EditorGUILayout.ScrollViewScope(_scrollWatch, GUILayout.MaxHeight(200));
				_scrollWatch = sv.scrollPosition;

				//グループ化(描画のみ)
				foreach (var byGo in _watch.GroupBy(w => w._ownerGo ? w._ownerGo.name : "(missing)"))
				{
					EditorGUILayout.LabelField(byGo.Key, EditorStyles.label);
					using (new EditorGUI.IndentLevelScope())
					{
						foreach (var byComp in byGo.GroupBy(w => w._component != null ? w._component.GetType().Name : "?"))
						{
							EditorGUILayout.LabelField(byComp.Key, EditorStyles.miniBoldLabel);
							using (new EditorGUI.IndentLevelScope())
							{
								foreach (var r in byComp)
								{
									using (new EditorGUILayout.HorizontalScope())
									{
										//変数名
										EditorGUILayout.LabelField(r.memberName, GUILayout.MinWidth(50), GUILayout.MaxWidth(250));

										//値
										string val = r.IsResolved ? SafeGetValue(r._getter) : "<null>";
										EditorGUILayout.LabelField(val, EditorStyles.textField, GUILayout.MinWidth(150));

										//最新化
										using (new EditorGUI.DisabledScope(!r.IsResolved))
										{
											if (GUILayout.Button(new GUIContent("↻", "Make most recent"),
												EditorStyles.miniButton, GUILayout.Width(25)))
											{
												MoveToHead(r);
											}
										}

										//解除
										if (GUILayout.Button(new GUIContent("✕", "Remove"),
											EditorStyles.miniButton, GUILayout.Width(25)))
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

		static string SafeGetValue(Func<object> getter)
		{
			try
			{
				var v = getter?.Invoke();
				return v switch
				{
					null => "<null>",
					UnityEngine.Object uo when uo == null => "<null>",
					_ => v.ToString()
				};
			}
			catch { return "<err>"; }
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
					EditorGUILayout.HelpBox("何も選択されていません", MessageType.None);
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

				foreach (var e in entries)
				{
					//ObjectName.ComponentName
					EditorGUILayout.LabelField($"{e.owner.name}.{e.component.GetType().Name}", EditorStyles.miniBoldLabel);

					using (new EditorGUI.IndentLevelScope())
					{
						foreach (var (memberName, getter) in e.members)
						{
							using (new EditorGUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField(memberName, GUILayout.MinWidth(50), GUILayout.MaxWidth(250));

								//今の値
								var preview = SafeGetValue(getter);
								EditorGUILayout.LabelField(preview, EditorStyles.textField, GUILayout.MinWidth(100));

								//Watch/Remove
								var tmpRef = MakeRef(e.owner, e.component, memberName);
								var key = MakeKey(tmpRef);
								var isWatched = _watchKeySet.Contains(key);

								if (!isWatched)
								{
									if (GUILayout.Button("Watch", GUILayout.Width(60)))
									{
										AddWatch(tmpRef);
									}
								}
								else
								{
									if (GUILayout.Button("Remove", GUILayout.Width(60)))
									{
										RemoveByKey(key);
									}
								}
							}
						}
					}
					EditorGUILayout.Space(2);
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
