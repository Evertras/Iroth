using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {

	void Awake()
	{
		LateUpdate ();
	}
	
	void LateUpdate () {
		transform.LookAt (Camera.main.transform.position);
	}
}
