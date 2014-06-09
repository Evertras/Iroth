using UnityEngine;
using System.Collections;

public class MovementSelector : MonoBehaviour {
	public Unit.MovementMode mode;
	public Unit parentUnit;

	public void Activate()
	{
		parentUnit.SelectForMovement (false);
		parentUnit.movementMode = mode;
		parentUnit.SelectForMovement (true);
	}
}
