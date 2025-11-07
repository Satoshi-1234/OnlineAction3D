using System;

namespace InspectorVariableProperties.Attributes
{
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
	public sealed class ProjectEnumAttribute : Attribute
	{
	}
}
