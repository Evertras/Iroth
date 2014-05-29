using UnityEngine;
using System.Collections;

public class MouseDragging : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit))
		{
			if (hit.collider.gameObject.tag == "DragHandle")
			{
				Debug.Log ("SLDKJFD");
				Draggable draggable = hit.collider.gameObject.GetComponent<Draggable>();

				draggable.Drag (ray);
			}
		}
	}
}
