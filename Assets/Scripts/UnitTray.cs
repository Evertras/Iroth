using UnityEngine;
using System.Collections;

public class UnitTray : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Unit parentUnit = transform.parent.gameObject.GetComponent<Unit>();

		int files = parentUnit.Files;

		transform.localScale = new Vector3(files, 0.1f, 1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
