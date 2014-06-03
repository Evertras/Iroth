using UnityEngine;
using System.Collections;

public class PhaseController : MonoBehaviour {
	public enum Phase
	{
		Charge,
		Move
	}

	void Awake()
	{
		GameObject.Find ("GUIPhases").GetComponent<PhaseIconManager> ().ChangeTo (Phase.Charge);
	}
}
