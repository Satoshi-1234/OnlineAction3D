using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	//監視対象参照
	[Serializable]
	public sealed class WatchedMemberRef
	{
		//Variable==================================================

		[SerializeField] public string ownerGlobalId;//GlobalObjectId
		[SerializeField] public string componentTypeName;//AssemblyQualifiedName
		[SerializeField] public string memberName;

		//実体
		[NonSerialized] public GameObject _ownerGo;
		[NonSerialized] public Component _component;
		[NonSerialized] public Func<object> _getter;


		//Function==================================================

		//取得用
		public bool IsResolved => _getter != null;
		[SerializeField] public string ownerHierarchyPath; // Root/Child/... を保持

		public bool TryResolve()
		{
			_getter = null; _ownerGo = null; _component = null;

			if (string.IsNullOrEmpty(componentTypeName) || string.IsNullOrEmpty(memberName))
				return false;

			//IdからObject
			if (!string.IsNullOrEmpty(ownerGlobalId) &&
				GlobalObjectId.TryParse(ownerGlobalId, out var gid))
			{
				var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
				_ownerGo = obj as GameObject;
			}

			//フォールバック
			if (_ownerGo == null && !string.IsNullOrEmpty(ownerHierarchyPath))
			{
				_ownerGo = GameObject.Find(ownerHierarchyPath);
			}
			if (_ownerGo == null) return false;

			//型調整
			var type = Type.GetType(componentTypeName, throwOnError: false);
			if (type == null) return false;

			_component = _ownerGo.GetComponent(type);
			if (_component == null) return false;

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var f = type.GetField(memberName, flags);
			if (f != null) { _getter = () => f.GetValue(_component); return true; }

			var p = type.GetProperty(memberName, flags);
			if (p?.GetGetMethod(true) is MethodInfo get && get.GetParameters().Length == 0)
			{ _getter = () => get.Invoke(_component, null); return true; }

			return false;
		}

	}
}
