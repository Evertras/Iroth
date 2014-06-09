using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CurveDirection
{
	Left,
	Right
}

public class MovementTrailCurved : MonoBehaviour {
	public GameObject segment;
	public int degreesPerSegment = 5;
	public float radius;
	public CurveDirection direction;

	private List<GameObject> segments = new List<GameObject>();

	public void SetDegrees(int degrees)
	{
		int totalSegments = degrees / degreesPerSegment;

		if (segments.Count > totalSegments)
		{
			int start = totalSegments;
			int remaining = segments.Count - totalSegments;
			var toDestroy = segments.GetRange(start, remaining);

			toDestroy.ForEach (o => Destroy(o));

			segments.RemoveRange (start, remaining);
		}
		else
		{
			for (int angle = segments.Count * degreesPerSegment; angle < degrees; angle += degreesPerSegment)
			{
				float modifier = 1.0f;
				var newSegment = Instantiate(segment) as GameObject;

				if (direction == CurveDirection.Right)
				{
					modifier = -1.0f;
				}

				var oldPos = newSegment.transform.localPosition;
				var oldRotate = newSegment.transform.localRotation;

				newSegment.transform.parent = transform;

				newSegment.transform.localPosition = oldPos;
				newSegment.transform.localRotation = oldRotate;

				Vector3 rotateAround = transform.position + transform.right * modifier * radius;
				newSegment.transform.RotateAround (rotateAround, transform.up, angle * modifier);

				segments.Add (newSegment);
			}
		}
	}
}
