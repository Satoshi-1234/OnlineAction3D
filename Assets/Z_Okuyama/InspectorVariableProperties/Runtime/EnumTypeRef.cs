using UnityEngine;
using System;

[Serializable]
public class EnumTypeRef
{
	[SerializeField] private string assemblyQualifiedName;//•Û‘¶—p

	public Type Type
	{
		get => string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);
		set => assemblyQualifiedName = value?.AssemblyQualifiedName;
	}
}
