using UnityEngine;
using System.Collections;

public class PhaseController : MonoBehaviour {
	public enum Phase
	{
		Charge,
		Move
	}

	public Phase currentPhase = Phase.Charge;

	void Awake()
	{
		GameObject.Find ("GUIPhases").GetComponent<PhaseIconManager> ().ChangeTo (Phase.Charge);
	}
}
