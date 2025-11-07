using UnityEngine;
using System;

namespace InspectorVariableProperties.Types
{
	[Serializable]

	public class EnumTypeRef
	{
		[SerializeField] private string assemblyQualifiedName;//保存用

		public Type Type
		{
			get => string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);
			set => assemblyQualifiedName = value?.AssemblyQualifiedName;
		}
	}
}
