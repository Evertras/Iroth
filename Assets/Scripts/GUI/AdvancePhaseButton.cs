using UnityEngine;
using System.Collections;

public class AdvancePhaseButton : MonoBehaviour {
	public PhaseController phaseController;
	public CombatController combatController;

	void OnMouseDown()
	{
		switch (phaseController.currentPhase)
		{
		case PhaseController.Phase.Movement:
			var units = GameObject.FindGameObjectsWithTag ("Unit");

			foreach (var unit in units)
			{
				unit.GetComponent<Unit>().movementMode = Unit.MovementMode.Unselected;

				var unitMover = unit.GetComponentsInChildren<UnitMover>()[0];

				Vector3 posDif = unitMover.transform.position - unit.transform.position;

				unit.transform.position += posDif;
				unitMover.transform.position -= posDif;

				unit.transform.rotation = unitMover.transform.rotation;
				unitMover.transform.localRotation = Quaternion.identity;

				unitMover.ResetMovement();
			}
			break;

		case PhaseController.Phase.Combat:
			combatController.RunCombatForTurn ();
			break;
		}
	}
}
