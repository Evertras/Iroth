using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
	public float RotateX;
	public float RotateY;
	public float RotateZ;
	public float Speed = 1;

	public Quaternion test;

	void Update()
	{
		transform.Rotate (RotateX * Speed * Time.deltaTime, RotateY * Speed * Time.deltaTime, RotateZ * Speed * Time.deltaTime);
	}
}
