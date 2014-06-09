using UnityEngine;
using System.Collections;

public class Killable : MonoBehaviour {
	public void Kill()
	{
		transform.parent = null;
		StartCoroutine (KillCoroutine ());
	}

	private IEnumerator KillCoroutine()
	{
		while (transform.position.y > -2.0f)
		{
			transform.Translate (0, -Time.deltaTime, 0);

			yield return null;
		}

		Destroy (gameObject);
	}
}
