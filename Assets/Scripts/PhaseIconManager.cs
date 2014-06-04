using UnityEngine;
using System.Collections;

public class PhaseIconManager : MonoBehaviour {
	GUITextureToggle lastPhase;

	public void ChangeTo(PhaseController.Phase phase)
	{
		GUITextureToggle target;

		switch (phase)
		{
			case PhaseController.Phase.Movement:
				target = transform.Find ("PhaseMove").GetComponent<GUITextureToggle>();
				break;

			default:
				target = null;
			break;
		}

		if (lastPhase != null)
		{
			lastPhase.SetState (false);
		}

		lastPhase = target;

		lastPhase.SetState (true);
	}
}
