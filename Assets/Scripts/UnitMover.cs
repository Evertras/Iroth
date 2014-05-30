using UnityEngine;
using System.Collections;

public class UnitMover : MonoBehaviour {
	public GameObject leftCornerHandle;
	public GameObject rightCornerHandle;
	public Unit parentUnit;

	[HideInInspector]
	public float movementRemaining;

	[HideInInspector]
	public float movementUsed = 0;

	[HideInInspector]
	public float currentRotationAngle = 0;

	[HideInInspector]
	public Vector3 lastPosition;

	[HideInInspector]
	public float lastRotationAngle;

	[HideInInspector]
	public DragHandle lastDragHandle;

	void Start()
	{
		float cornerOffset = parentUnit.Files * 0.5f;

		leftCornerHandle.transform.localPosition = new Vector3(-cornerOffset, 0.5f, 0.0f);
		rightCornerHandle.transform.localPosition = new Vector3(cornerOffset, 0.5f, 0.0f);

		ResetMovement ();
	}

	void Update()
	{
	}

	public void ResetMovement()
	{
		movementUsed = 0;
		movementRemaining = parentUnit.maximumMovement;

		lastPosition = transform.position;
		lastRotationAngle = transform.rotation.eulerAngles.y;
	}
}
