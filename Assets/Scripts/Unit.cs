using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public GameObject Model;

	public int Files = 5;
	public int Count = 10;
	public bool Friendly = true;

	public GameObject movementTrailStraight;
	public GameObject movementTrailCurved;

	public Material friendlyUnitTrayMaterial;
	public Material enemyUnitTrayMaterial;

	[HideInInspector]
	public float maximumMovement;

	public int Ranks
	{
		get
		{
			return (Count - 1) / Files + 1;
		}
	}

	void Awake()
	{
		int ranks = Ranks;

		maximumMovement = Model.GetComponent<ModelStats> ().movement * 5;

		var modelContainer = transform.Find ("Models");

		for (int row = 0; row < ranks; ++row)
		{
			for (int column = 0; column < Files && row*Files + column < Count; ++column)
			{
				var obj = Instantiate(Model) as GameObject;

				obj.transform.parent = modelContainer;
				obj.transform.localPosition = new Vector3(column - Files * 0.5f + 0.5f, obj.transform.localPosition.y, -row - 0.5f);
			}
		}

		var tray = transform.Find ("UnitMover/TrayContainer/Tray").gameObject;

		tray.renderer.material = Friendly ? friendlyUnitTrayMaterial : enemyUnitTrayMaterial;
	}

	public void SelectForMovement(bool selected)
	{
		var handles = transform.Find ("UnitMover/MovementHandles").gameObject;

		handles.SetActive (selected);
	}
}
