using System;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	//GlobalObjectId用
	public static class WatchStoreUtil
	{
		public static bool TryMakeGlobalId(GameObject go, out string id)
		{
			id = default;
			if (go == null) { return false; }

			var gid = GlobalObjectId.GetGlobalObjectIdSlow(go);
			id = gid.ToString();

			return !gid.Equals(default);
		}

		//パス
		public static string BuildHierarchyPath(GameObject go)
		{
			if (go == null) return null;
			var path = go.name;
			var t = go.transform;
			while (t.parent != null)
			{
				t = t.parent;
				path = t.name + "/" + path;
			}
			return path;
		}

	}
}
