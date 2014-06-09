using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
	public enum Side
	{
		Frontal,
		Flank,
		Rear
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

	[HideInInspector]
	public float maximumMovement;

	[HideInInspector]
	public float lingeringDamage = 0;

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
}
