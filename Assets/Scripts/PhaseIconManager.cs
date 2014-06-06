using UnityEngine;
using System.Collections;

public class PhaseIconManager : MonoBehaviour {
	public Texture movementIcon;
	public Texture shootingIcon;
	public Texture combatIcon;

	public void ChangeTo(PhaseController.Phase phase)
	{
		Texture target = null;

		switch (phase)
		{
		case PhaseController.Phase.Movement:
			target = movementIcon;
			break;

		case PhaseController.Phase.Shooting:
			target = shootingIcon;
			break;

		case PhaseController.Phase.Combat:
			target = combatIcon;
			break;

		default:
			throw new UnityException("Given unknown phase");
		}

		transform.Find ("PhaseIcon").guiTexture.texture = target;
	}
}
