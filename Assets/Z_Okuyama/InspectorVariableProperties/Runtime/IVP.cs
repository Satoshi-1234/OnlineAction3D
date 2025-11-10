using System;
using UnityEngine;

namespace IVP
{
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
	public sealed class ProjectEnumAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class EnumArrayAttribute : Attribute
	{
		public EnumArrayAttribute(Type enumType) { }
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class EnumArrayFromAttribute : Attribute
	{
		public EnumArrayFromAttribute(string enumTypeRefFieldName) { }
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class LayerArrayAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TagArrayAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class LayerSelectAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TagSelectAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class SceneNameSelectAttribute : Attribute { }


	[Serializable]
	public sealed class EnumTypeRef
	{
		[SerializeField] private string assemblyQualifiedName;

		public string AssemblyQualifiedName
		{
			get => assemblyQualifiedName;
			set => assemblyQualifiedName = value;
		}
		public Type Type =>
			string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);
	}
}
