using UnityEngine;
using System.Collections;

public class LoSArc : MonoBehaviour {
	public enum Direction
	{
		Front,
		Right,
		Left,
		Rear
	}

	public Direction side;
	public Unit parentUnit;

	private Transform middleSection;
	private Transform leftSection;
	private Transform rightSection;

	// Use this for initialization
	void Start () {
		int width = 1;

		switch (side)
		{
		case Direction.Front:
			width = parentUnit.Files;
			break;

		case Direction.Right:
			width = parentUnit.Ranks;
			transform.localPosition = new Vector3(parentUnit.Files * 0.5f, 0, -parentUnit.Ranks * 0.5f);
			break;

		case Direction.Left:
			width = parentUnit.Ranks;
			transform.localPosition = new Vector3(-parentUnit.Files * 0.5f, 0, -parentUnit.Ranks * 0.5f);
			break;

		case Direction.Rear:
			width = parentUnit.Files;
			transform.localPosition = new Vector3(0, 0, -parentUnit.Ranks);
			break;
		}

		middleSection = transform.Find ("Middle");
		middleSection.localScale = new Vector3 (width, 0.1f, 1);

		rightSection = transform.Find ("Right");
		rightSection.Translate (width * 0.5f - 0.5f, 0, 0, transform);

		leftSection = transform.Find ("Left");
		leftSection.Translate (-(width * 0.5f - 0.5f), 0, 0, transform);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void SetLength(float length)
	{
		var middleScale = middleSection.localScale;
		var middlePos = middleSection.localPosition;

		middleSection.localScale = new Vector3 (middleScale.x, middleScale.y, length);
		middleSection.localPosition = new Vector3 (middlePos.x, middlePos.y, 0.5f * middleSection.localScale.z);

		var cornerScale = new Vector3 (length, length, leftSection.localScale.z);

		leftSection.localScale = cornerScale;
		rightSection.localScale = cornerScale;
	}
}
