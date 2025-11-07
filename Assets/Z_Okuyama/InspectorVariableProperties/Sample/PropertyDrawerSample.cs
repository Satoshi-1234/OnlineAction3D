using UnityEngine;

public class PropertyDrawerSample : MonoBehaviour
{
	//TestópEnum
	[ProjectEnum]
	public enum ENUM
	{
		ENUM_A,
		ENUM_B,
		ENUM_C,
		ENUM_D,
		ENUM_E,
	}

	//Arrayån
	[Header("Array")]
	[SerializeField, EnumArray(typeof(ENUM))] int[] _enumArray = new int[8];
	[SerializeField] EnumTypeRef _enumTypeRef;
	[SerializeField, EnumArrayFrom(nameof(_enumTypeRef))] int[] _enumArrayFrom = new int[8];
	[SerializeField, LayerArray] int[] _layerArray = new int[8];
	[SerializeField, TagArray] int[] _tagArray = new int[8];

	//Selectån(stringÇÃÇ›)
	[Header("Select")]
	[SerializeField, LayerSelect] string _layerSelect;
	[SerializeField, TagSelect] string _tagSelect;
	[SerializeField, SceneNameSelect] string _sceneNameSelect;
}
