using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {

	void Awake()
	{
		Update ();
	}
	
	void Update () {
		transform.LookAt (Camera.main.transform.position);
	}
}
