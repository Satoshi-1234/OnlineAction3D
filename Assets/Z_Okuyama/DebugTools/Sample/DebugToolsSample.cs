using UnityEngine;
using DebugTools;

public class DebugToolsSample : MonoBehaviour
{
	//Watch
	[DebugVariable] int _watchVariableInt = 0;
	[DebugVariable] float _watchVariableFloat = 0;
	[DebugVariable] string _watchVariableString = "‚ ‚¢‚¤‚¦‚¨";

	private void Start()
	{
		_watchVariableString = gameObject.name;
	}
	private void FixedUpdate()
	{
		_watchVariableInt += 1;
		_watchVariableFloat += 0.2f;
	}
}
