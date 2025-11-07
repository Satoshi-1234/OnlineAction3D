using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DebugTools.EditorUI
{
	//äƒéãëŒè€éQè∆
	[Serializable]
	public sealed class WatchedMemberRef
	{
		//Variable==================================================

		[SerializeField] public string ownerGlobalId;//GlobalObjectId
		[SerializeField] public string componentTypeName;//AssemblyQualifiedName
		[SerializeField] public string memberName;

		//é¿ëÃ
		[NonSerialized] public GameObject _ownerGo;
		[NonSerialized] public Component _component;
		[NonSerialized] public Func<object> _getter;


		//Function==================================================

		//éÊìæóp
		public bool IsResolved => _getter != null;

		public bool TryResolve()
		{
			_getter = null;
			_ownerGo = null;
			_component = null;

			if (string.IsNullOrEmpty(ownerGlobalId) ||
				string.IsNullOrEmpty(componentTypeName) ||
				string.IsNullOrEmpty(memberName))
			{
				return false;
			}

			//GlobalIdÅÀObject
			if (!GlobalObjectId.TryParse(ownerGlobalId, out var gid)) { return false; }
			var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
			_ownerGo = obj as GameObject;
			if (_ownerGo == null) { return false; }

			//å^âåà
			var type = Type.GetType(componentTypeName, throwOnError: false);
			if (type == null) { return false; }

			_component = _ownerGo.GetComponent(type);
			if (_component == null) { return false; }

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var f = type.GetField(memberName, flags);
			if (f != null)
			{
				_getter = () => f.GetValue(_component);
				return true;
			}

			var p = type.GetProperty(memberName, flags);
			if (p?.GetGetMethod(true) is MethodInfo get && get.GetParameters().Length == 0)
			{
				_getter = () => get.Invoke(_component, null);
				return true;
			}

			return false;
		}

		public string OwnerLabel =>
			_ownerGo != null
				? $"{_ownerGo.name}.{(_component != null ? _component.GetType().Name : "?")}"
				: "(missing)";
	}
}
