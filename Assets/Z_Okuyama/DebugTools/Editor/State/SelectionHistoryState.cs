using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	///Editor再起動後も設定を保持
	[FilePath("UserSettings/DebugToolState.asset", FilePathAttribute.Location.ProjectFolder)]
	public class SelectionHistoryState : ScriptableSingleton<SelectionHistoryState>
	{
		[SerializeField] int _activeTabIndex = 0;
		public const int MaxItems = 20;//最大保持件数

		[SerializeField] List<string> _objectHistory = new();
		[SerializeField] List<string> _objectBookmarks = new();

		[SerializeField] List<string> _assetHistory = new();
		[SerializeField] List<string> _assetBookmarks = new();

		public int ActiveTabIndex { get => _activeTabIndex; set { _activeTabIndex = value; Save(true); } }
		public int HistoryCapacity => MaxItems;

		public IReadOnlyList<string> ObjectHistory => _objectHistory;
		public IReadOnlyList<string> AssetHistory => _assetHistory;
		public IReadOnlyList<string> ObjectBookmarks => _objectBookmarks;
		public IReadOnlyList<string> AssetBookmarks => _assetBookmarks;

		public void SaveNow() => Save(true);

		public void PushObjectToHistory(GameObject go)
		{
			if (go == null) return;
			var gid = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
			_objectHistory.Remove(gid);
			_objectHistory.Insert(0, gid);
			if (_objectHistory.Count > MaxItems) _objectHistory.RemoveAt(_objectHistory.Count - 1);
			Save(true);
		}

		public void PushAssetToHistory(Object asset)
		{
			if (asset == null) return;
			var path = AssetDatabase.GetAssetPath(asset);
			if (string.IsNullOrEmpty(path)) return;
			var guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			_assetHistory.Remove(guid);
			_assetHistory.Insert(0, guid);
			if (_assetHistory.Count > MaxItems) _assetHistory.RemoveAt(_assetHistory.Count - 1);
			Save(true);
		}

		public void ToggleObjectBookmark(GameObject go)
		{
			if (go == null) return;
			var gid = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
			if (_objectBookmarks.Contains(gid)) _objectBookmarks.Remove(gid);
			else _objectBookmarks.Add(gid);
			Save(true);
		}

		public void ToggleAssetBookmark(Object asset)
		{
			if (asset == null) return;
			var path = AssetDatabase.GetAssetPath(asset);
			var guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			if (_assetBookmarks.Contains(guid)) _assetBookmarks.Remove(guid);
			else _assetBookmarks.Add(guid);
			Save(true);
		}

		public bool IsBookmarked(GameObject go)
		{
			if (go == null) return false;
			var gid = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
			return _objectBookmarks.Contains(gid);
		}

		public bool IsBookmarked(Object asset)
		{
			if (asset == null) return false;
			var path = AssetDatabase.GetAssetPath(asset);
			var guid = AssetDatabase.AssetPathToGUID(path);
			return _assetBookmarks.Contains(guid);
		}
	}
}
