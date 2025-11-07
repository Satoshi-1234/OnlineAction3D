using System;

namespace DebugTools
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class DebugVariableAttribute : Attribute { }
}
