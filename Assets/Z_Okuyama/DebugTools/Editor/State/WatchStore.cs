using System;
using System.Collections.Generic;
using UnityEditor;

namespace DebugTools.EditorUI
{
	//WatchList永続用
	[FilePath("UserSettings/DebugTools_WatchStore.asset", FilePathAttribute.Location.ProjectFolder)]
	public sealed class WatchStore : ScriptableSingleton<WatchStore>
	{
		//Sturct==================================================
		[Serializable]
		public struct Item
		{
			public string ownerGlobalId;
			public string componentTypeName;
			public string memberName;
			public string ownerHierarchyPath;
		}

		//Variable==================================================

		public const int MaxItems = 10;

		public List<Item> items = new List<Item>();

		//Function==================================================

		public void SaveNow() => Save(true);

		public void SetFromRefs(IReadOnlyList<WatchedMemberRef> refs)
		{
			items.Clear();
			foreach (var r in refs)
			{
				items.Add(new Item
				{
					ownerGlobalId = r.ownerGlobalId,
					componentTypeName = r.componentTypeName,
					memberName = r.memberName,
					ownerHierarchyPath = r.ownerHierarchyPath,
				});
				if (items.Count >= MaxItems) break;
			}
			SaveNow();
		}
	}
}
