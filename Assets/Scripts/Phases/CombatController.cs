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
			float totalPlayerDamage = 0;
			float totalEnemyDamage = 0;

			// For now we just assume damage is done to the front
			totalPlayerDamage = playerUnits.Sum (u => u.GetTotalDamageInCombat (Unit.Side.Frontal));
			totalEnemyDamage = enemyUnits.Sum (u => u.GetTotalDamageInCombat (Unit.Side.Frontal));

			float playerDamagePerEnemyUnit = totalPlayerDamage / enemyUnits.Count ();
			float enemyDamagePerPlayerUnit = totalEnemyDamage / playerUnits.Count ();

			foreach (var playerUnit in playerUnits)
			{
				playerUnit.Damage (enemyDamagePerPlayerUnit);
			}

			foreach (var enemyUnit in enemyUnits)
			{
				enemyUnit.Damage (playerDamagePerEnemyUnit);
			}
		}
	}

	public void RunCombatForTurn ()
	{
		// For testing purposes we're going to start with just assuming everyone's in combat with each other in one big melee,
		// this obviously needs to change soon but it'll be enough for early testing.
		var units = GameObject.FindGameObjectsWithTag ("Unit").Select (u => u.GetComponent<Unit>());

		var playerUnits = units.Where (u => u.Friendly);
		var enemyUnits = units.Where (u => !u.Friendly);

		Combat testCombat = new Combat (playerUnits, enemyUnits);

		testCombat.Run ();
	}
}
