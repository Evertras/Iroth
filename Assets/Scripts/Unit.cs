using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public GameObject Model;
	public int Files = 5;
	public int Count = 10;

	public float maximumMovement = 5;

	public int Ranks
	{
		get
		{
			return (Count - 1) / Files + 1;
		}
	}

	void Start()
	{
		int ranks = Ranks;

		for (int row = 0; row < ranks; ++row)
		{
			for (int column = 0; column < Files && row*Files + column < Count; ++column)
			{
				var obj = Instantiate(Model) as GameObject;

				obj.transform.parent = transform;
				obj.transform.localPosition = new Vector3(column - Files * 0.5f + 0.5f, obj.transform.localPosition.y, -row - 0.5f);
			}
		}
	}
}
