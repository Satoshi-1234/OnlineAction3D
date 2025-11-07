using System;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	//GlobalObjectId—p
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

		//•\Ž¦—p(ObjectName.ComponentName)
		public static string BuildOwnerLabel(GameObject go, Component comp)
		{

			return (go == null)
				? "(missing)"
				: $"{go.name}.{((comp != null) ? comp.GetType().Name : "?")}";
		}

		//GlobalId‚ÆŒ^–¼‚©‚çŽÀ‘Ì
		public static bool TryResolveOwner(string ownerGlobalId, string componentTypeName, out GameObject go, out Component comp)
		{
			go = null;
			comp = null;

			if (!GlobalObjectId.TryParse(ownerGlobalId, out var gid)) { return false; }

			var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
			go = obj as GameObject;
			if (go == null) { return false; }

			var type = Type.GetType(componentTypeName, throwOnError: false);
			if (type == null) { return false; }

			comp = go.GetComponent(type);
			return comp != null;
		}
	}
}
