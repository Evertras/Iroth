using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatController : MonoBehaviour {
	private class Combat
	{
		IEnumerable<Unit> playerUnits;
		IEnumerable<Unit> enemyUnits;

		public Combat(IEnumerable<Unit> playerUnits, IEnumerable<Unit>enemyUnits)
		{
			this.playerUnits = playerUnits;
			this.enemyUnits = enemyUnits;
		}

		public void Run()
		{
		}
	}

	public void Start()
	{
		// For testing purposes we're going to start with just assuming everyone's in combat with each other
		var units = GameObject.FindGameObjectsWithTag ("Unit");

		var playerUnits = units.Where (u => u.GetComponent<Unit> ().Friendly);
		var enemyUnits = units.Where (u => !u.GetComponent<Unit> ().Friendly);
	}
}
