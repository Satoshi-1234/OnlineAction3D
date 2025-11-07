using UnityEngine;

public class EnumArrayFromAttribute : PropertyAttribute
{
	public readonly string enumTypeRefFieldName;
	public EnumArrayFromAttribute(string enumTypeRefFieldName)
	{
		this.enumTypeRefFieldName = enumTypeRefFieldName;
	}
}
