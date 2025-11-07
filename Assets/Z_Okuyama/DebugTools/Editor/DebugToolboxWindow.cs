using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DebugTools.EditorUI;
using UnityEditor.Search;

public class DebugToolboxWindow : EditorWindow
{
	List<IToolTab> _tabs;
	int _tabIndex;

	[MenuItem("Tools/Debug Toolbox")]
	public static void Open()
	{
		var w = GetWindow<DebugToolboxWindow>("Debug Toolbox");
		w.minSize = new Vector2(420, 260);
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
			var next = GUILayout.Toolbar(_tabIndex, labels, EditorStyles.toolbarButton, GUILayout.Height(22));
			if (next != _tabIndex)
			{
				_tabIndex = next;
				SelectionHistoryState.instance.ActiveTabIndex = _tabIndex;
			}
			GUILayout.FlexibleSpace();
		}

		EditorGUILayout.Space(4);
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
