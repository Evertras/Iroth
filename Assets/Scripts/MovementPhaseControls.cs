using UnityEngine;
using System.Collections;

public class MovementPhaseControls : MonoBehaviour {
	DragHandle selectedDragHandle = null;
	
	// Update is called once per frame
	void Update () {
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (selectedDragHandle != null)
		{
			if (Input.GetMouseButton(0) == false)
			{
				selectedDragHandle = null;
			}
			else
			{
				selectedDragHandle.Drag (ray);
			}
		}
		else if (Input.GetMouseButtonDown(0) && Physics.Raycast (ray, out hit))
		{
			if (hit.collider.gameObject.tag == "DragHandle")
			{
				selectedDragHandle = hit.collider.gameObject.GetComponent<DragHandle>();
				
				selectedDragHandle.Drag (ray);
			}
		}
	}
}
