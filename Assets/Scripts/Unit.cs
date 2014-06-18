using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
	public enum Side
	{
		Frontal,
		Flank,
		Rear
	}

	public enum MovementMode
	{
		Unselected,
		March,
		Regroup,
		Charge
	}

	public GameObject Model;

	public int Files = 5;
	public int Count = 10;
	public bool Friendly = true;

	public delegate void CountChangedAction ();
	public event CountChangedAction CountChanged;

	public GameObject movementTrailStraight;
	public GameObject movementTrailCurved;

	public Material friendlyUnitTrayMaterial;
	public Material enemyUnitTrayMaterial;

	public LoSArcContainer losArcContainer;

	[HideInInspector]
	public float maximumMovement;

	[HideInInspector]
	public float lingeringDamage = 0;

	[HideInInspector]
	public MovementMode movementMode = MovementMode.Unselected;

	private ModelStats modelStats;

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

		modelStats = Model.GetComponent<ModelStats> ();

		maximumMovement = modelStats.movement * 5;

		var modelContainer = transform.Find ("Models");

		for (int row = 0; row < ranks; ++row)
		{
			for (int column = 0; column < Files && row*Files + column < Count; ++column)
			{
				var obj = Instantiate(Model) as GameObject;

				obj.transform.parent = modelContainer;
				obj.transform.localPosition = new Vector3(column - Files * 0.5f + 0.5f, obj.transform.localPosition.y, -row - 0.5f);
				obj.transform.localRotation = Quaternion.identity;
			}
		}

		var tray = transform.Find ("UnitMover/TrayContainer/Tray").gameObject;

		tray.renderer.material = Friendly ? friendlyUnitTrayMaterial : enemyUnitTrayMaterial;
	}

	public void SelectForMovement(bool selected)
	{
		GameObject cancelButton;

		switch (movementMode)
		{
		case MovementMode.Unselected:
			var modeSelectors = transform.Find ("MovementModeSelectors").gameObject;
			modeSelectors.SetActive(selected);
			break;

		case MovementMode.March:
			var handles = transform.Find ("UnitMover/MovementHandles").gameObject;
			handles.SetActive (selected);

			cancelButton = transform.Find ("UnitMover/CancelContainer").gameObject;
			cancelButton.SetActive (selected);
			break;

		case MovementMode.Charge:
			/*
			var selectParticles = transform.Find ("UnitMover/TrayContainer/SelectedParticles").gameObject;
			selectParticles.SetActive(selected);
			*/
			if (selected)
			{
				losArcContainer.Show(LoSArc.Direction.Front, maximumMovement * 1.5f);
			}
			else
			{
				losArcContainer.HideAll ();
			}

			cancelButton = transform.Find ("UnitMover/CancelContainer").gameObject;
			cancelButton.SetActive (selected);
			break;
		}
	}

	public bool Charge(Unit enemy)
	{
		if (Friendly ^ enemy.Friendly)
		{
			// Figure out if the enemy is in our arc
			// TODO: this is bad and wrong and bad and bad and wrong and also bad
			Vector3 toEnemy = enemy.transform.position - transform.position;
			float angle = Vector3.Angle (transform.forward, toEnemy);

			Debug.Log (angle);
		}

		return false;
	}

	public float GetTotalDamageInCombat(Side sideAttackingInto)
	{
		int attacks = Mathf.Min (Files * 4, Count);
		float hits = attacks * (0.5f + 0.1f * modelStats.finesse);
		float modifier = 1;

		if (sideAttackingInto == Side.Flank)
		{
			modifier = 1.5f;
		}
		else if (sideAttackingInto == Side.Rear)
		{
			modifier = 2;
		}

		return hits * modelStats.strength * modifier;
	}

	public void Damage(float damage)
	{
		if (modelStats.toughness <= 0)
		{
			throw new UnityException("Toughness must be >= 0");
		}

		lingeringDamage += damage;
		var modelContainer = transform.Find ("Models");

		int numToDestroy = 0;

		while (lingeringDamage > modelStats.toughness)
		{
			lingeringDamage -= modelStats.toughness;

			++numToDestroy;
		}

		if (numToDestroy < Count)
		{
			List<GameObject> toDestroy = new List<GameObject> ();

			for (int i = 0; i < numToDestroy; ++i)
			{
				toDestroy.Add (modelContainer.GetChild (modelContainer.childCount - 1 - i).gameObject);
			}

			foreach (var obj in toDestroy)
			{
				obj.GetComponent<Killable>().Kill ();
			}

			Count -= numToDestroy;

			if (CountChanged != null)
			{
				CountChanged();
			}
		}
		else
		{
			// TODO: Destroy the unit
		}
	}

	public void CancelMovement()
	{
		var unitMoverObject = transform.Find ("UnitMover").gameObject;

		SelectForMovement(false);

		unitMoverObject.transform.localPosition = Vector3.zero;
		unitMoverObject.transform.localRotation = Quaternion.identity;

		unitMoverObject.GetComponent<UnitMover> ().ResetMovement ();

		movementMode = Unit.MovementMode.Unselected;
	}
}
