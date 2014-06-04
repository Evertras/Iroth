using UnityEngine;
using System.Collections;

public class PhaseController : MonoBehaviour {
	public enum Phase
	{
		Movement,
		Shooting,
		Combat
	}

	public Phase currentPhase = Phase.Movement;

	void Awake()
	{
		GameObject.Find ("GUIPhases").GetComponent<PhaseIconManager> ().ChangeTo (Phase.Movement);
	}
}
