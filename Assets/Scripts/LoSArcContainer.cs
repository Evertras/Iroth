using UnityEngine;
using System.Collections;

public class LoSArcContainer : MonoBehaviour {
	public LoSArc front;
	public LoSArc right;
	public LoSArc left;
	public LoSArc rear;

	// Use this for initialization
	void Start () {
	}
	
	public void Show(LoSArc.Direction direction, float length)
	{
		HideAll ();

		LoSArc arc = front;

		switch (direction)
		{
		case LoSArc.Direction.Front:
			arc = front;
			break;

		case LoSArc.Direction.Right:
			arc = right;
			break;

		case LoSArc.Direction.Left:
			arc = left;
			break;

		case LoSArc.Direction.Rear:
			arc = rear;
			break;
		}

		arc.gameObject.SetActive(true);
		arc.SetLength (length);
	}

	public void HideAll()
	{
		front.gameObject.SetActive (false);
		right.gameObject.SetActive (false);
		left.gameObject.SetActive (false);
		rear.gameObject.SetActive (false);
	}
}
