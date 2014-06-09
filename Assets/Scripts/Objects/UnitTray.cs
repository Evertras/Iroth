using UnityEngine;
using System.Collections;

public class UnitTray : MonoBehaviour {
	public UnitMover parentUnitMover;

	// Use this for initialization
	void Start () {
		Resize ();

		parentUnitMover.parentUnit.CountChanged += Resize;
	}

	public void Resize()
	{
		int files = parentUnitMover.parentUnit.Files;
		int ranks = parentUnitMover.parentUnit.Ranks;
		
		transform.localScale = new Vector3(files, 0.1f, ranks);
	}
}
