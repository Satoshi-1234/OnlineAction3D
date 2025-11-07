using UnityEngine;

namespace InspectorVariableProperties.Attributes
{
	public class LayerArrayAttribute : PropertyAttribute
	{
		public readonly string[] names;

		public LayerArrayAttribute()
		{
			names = new string[32];
			for (int i = 0; i < 32; i++)
			{
				string name = LayerMask.LayerToName(i);
				names[i] = string.IsNullOrEmpty(name) ? $"Layer {i}" : name;
			}
		}
	}
}
