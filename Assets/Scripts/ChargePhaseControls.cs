using UnityEngine;
using System.Collections;

public class ChargePhaseControls : MonoBehaviour {
	Unit selectedUnit = null;
	
	// Update is called once per frame
	void Update ()
	{
		if (gameObject.GetComponent<PhaseController> ().currentPhase == PhaseController.Phase.Charge)
		{
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast (ray, out hit))
			{
				if (hit.collider.tag == "Tray")
				{
					selectedUnit = hit.collider.transform.root.gameObject.GetComponent<Unit> ();
					
					selectedUnit.SelectForMovement (true);
				}
			}
		}
	}
}
