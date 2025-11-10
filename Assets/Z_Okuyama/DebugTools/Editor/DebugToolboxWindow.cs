using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DebugTools.EditorUI
{
	public class DebugToolboxWindow : EditorWindow
	{
		static class Ui
		{
			public static class Width
			{
				public const float Min = 420f;
			}
			public static class Height
			{
				public const float Min = 260f;
				public const float Toolbar = 22f;
				public const float Space = 4f;
			}
		}

		List<IToolTab> _tabs;
		int _tabIndex;

		[MenuItem("Tools/Debug Toolbox")]
		public static void Open()
		{
			var w = GetWindow<DebugToolboxWindow>("Debug Toolbox");
			w.minSize = new Vector2(Ui.Width.Min, Ui.Height.Min);
			w.Show();
		}

		void OnEnable()
		{
			_tabs = new List<IToolTab>
		{
			new RenameTab(),
			new SearchTab(),
			new HistoryTab(),
			new WatchTab(),
		};
			foreach (var t in _tabs) t.OnEnable(this);

			_tabIndex = Mathf.Clamp(SelectionHistoryState.instance.ActiveTabIndex, 0, _tabs.Count - 1);

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
		}

		void OnDisable()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			foreach (var t in _tabs) t.OnDisable();
		}

		void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				var labels = _tabs.Select(t => new GUIContent(t.Title, t.Icon)).ToArray();
				var next = GUILayout.Toolbar(_tabIndex, labels, EditorStyles.toolbarButton, GUILayout.Height(Ui.Height.Toolbar));
				if (next != _tabIndex)
				{
					_tabIndex = next;
					SelectionHistoryState.instance.ActiveTabIndex = _tabIndex;
				}
				GUILayout.FlexibleSpace();
			}

			EditorGUILayout.Space(Ui.Height.Space);
			_tabs[_tabIndex].OnGUI();
		}

		void OnSelectionChanged()
		{
			var objs = Selection.objects;
			if (objs == null || objs.Length == 0) return;

			var last = objs.Last();
			if (last is GameObject go && go.scene.IsValid())
				SelectionHistoryState.instance.PushObjectToHistory(go);
			else
				SelectionHistoryState.instance.PushAssetToHistory(last);
		}
	}
}