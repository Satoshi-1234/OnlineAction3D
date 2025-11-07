using System;
using UnityEngine;

public class EnumArrayAttribute : PropertyAttribute
{
	public readonly string[] names;
	public EnumArrayAttribute(Type enumType) => names = Enum.GetNames(enumType);
}
