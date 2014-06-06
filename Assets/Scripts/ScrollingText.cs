using UnityEngine;
using System.Collections;

public class ScrollingText : MonoBehaviour {
	public Color color = Color.white;
	public float scrollVelocity = 0.05f;
	public float duration = 1.5f;
	public float alpha;
	
	void Start()
	{
		guiText.material.color = color;
		alpha = 1;
	}

	void Update()
	{
		if (alpha > 0)
		{
			transform.position += new Vector3(0, scrollVelocity * Time.deltaTime, 0);
			alpha -= Time.deltaTime / duration;
			guiText.material.color = new Color(guiText.material.color.r, guiText.material.color.g, guiText.material.color.b, alpha);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
