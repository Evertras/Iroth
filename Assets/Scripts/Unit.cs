using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public int Files = 5;

	void Start()
	{
		Transform leftCornerHandle = transform.FindChild ("LeftCornerHandle");
		Transform rightCornerHandle = transform.FindChild ("RightCornerHandle");

		float cornerOffset = Files * 0.5f;

		leftCornerHandle.localPosition = new Vector3(-cornerOffset, 0.25f, 0.0f);
		rightCornerHandle.localPosition = new Vector3(cornerOffset, 0.25f, 0.0f);
	}
}
