using UnityEngine;
using System.Collections;

public class AdvancePhaseButton : MonoBehaviour {
	void OnMouseDown()
	{
		var units = GameObject.FindGameObjectsWithTag ("Unit");

		foreach (var unit in units)
		{
			var unitMover = unit.GetComponentsInChildren<UnitMover>()[0];

			Vector3 posDif = unitMover.transform.position - unit.transform.position;

			unit.transform.position += posDif;
			unitMover.transform.position -= posDif;

			unit.transform.rotation = unitMover.transform.rotation;
			unitMover.transform.localRotation = Quaternion.identity;
		}
	}
}
