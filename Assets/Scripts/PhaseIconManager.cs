using UnityEngine;
using System.Collections;

public class PhaseIconManager : MonoBehaviour {
	public void ChangeTo(PhaseController.Phase phase)
	{
		GUITexture target;

		switch (phase)
		{
			case PhaseController.Phase.Charge:
				target = transform.Find ("PhaseCharge").guiTexture;
				break;

			case PhaseController.Phase.Move:
				target = transform.Find ("PhaseMove").guiTexture;
				break;

			default:
				target = null;
			break;
		}

		var bg = transform.Find ("PhaseBG");

		bg.transform.localPosition = target.transform.localPosition;
		bg.guiTexture.pixelInset = target.guiTexture.pixelInset;
	}
}
