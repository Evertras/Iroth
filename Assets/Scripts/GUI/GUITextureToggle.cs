using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUITexture))]
public class GUITextureToggle : MonoBehaviour {

	public Texture off;
	public Texture on;

	public void SetState(bool state)
	{
		if (state)
		{
			guiTexture.texture = on;
		}
		else
		{
			guiTexture.texture = off;
		}
	}
}
