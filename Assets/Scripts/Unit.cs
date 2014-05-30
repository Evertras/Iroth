using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public int Files = 5;

	public float maximumMovement = 5;
	public float movementRemaining = 5;
	public float movementUsed = 0;
	public float currentRotationAngle = 0;
	public Vector3 lastPosition;
	public float lastRotationAngle;
	public DragHandle lastDragHandle;

	void Start()
	{
		Transform leftCornerHandle = transform.FindChild ("LeftCornerHandle");
		Transform rightCornerHandle = transform.FindChild ("RightCornerHandle");

		float cornerOffset = Files * 0.5f;

		leftCornerHandle.localPosition = new Vector3(-cornerOffset, 0.25f, 0.0f);
		rightCornerHandle.localPosition = new Vector3(cornerOffset, 0.25f, 0.0f);

		lastPosition = transform.position;
		lastRotationAngle = transform.rotation.eulerAngles.y;
	}

	void Update()
	{
		var text = GameObject.FindObjectOfType(typeof(GUIText)) as GUIText;

		text.text = (movementRemaining - movementUsed).ToString();
	}
}
