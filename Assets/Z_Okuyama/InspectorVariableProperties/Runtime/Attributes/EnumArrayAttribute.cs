using System;
using UnityEngine;

namespace InspectorVariableProperties.Attributes
{
	public class EnumArrayAttribute : PropertyAttribute
	{
		public readonly string[] names;
		public EnumArrayAttribute(Type enumType) => names = Enum.GetNames(enumType);
	}
}