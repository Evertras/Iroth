using UnityEngine;
using System.Collections;

public enum DraggableBehavior
{
	Forward,
	Wheel
}

public class DragHandle : MonoBehaviour {
	public DraggableBehavior behavior = DraggableBehavior.Forward;

	public void Drag(Ray ray)
	{
		float clickDistance;
		Plane plane = new Plane(transform.up, transform.position);
		Vector3 clickPositionOnPlane;

		if (plane.Raycast(ray, out clickDistance))
		{
			clickPositionOnPlane = ray.GetPoint (clickDistance);

			switch (behavior)
			{
			case DraggableBehavior.Forward:
				Vector3 newOffset = Vector3.Project(clickPositionOnPlane - transform.position, transform.parent.transform.forward);

				transform.parent.Translate (newOffset, Space.World);
				break;

			case DraggableBehavior.Wheel:
				float radius = transform.parent.GetComponent<Unit>().Files;
				Vector3 center;
				float angle;

				if (transform.localPosition.x < 0) // left
				{
					center = transform.parent.TransformPoint (transform.localPosition + new Vector3(radius, 0, 0));
				}
				else
				{
					center = transform.parent.TransformPoint (transform.localPosition + new Vector3(-radius, 0, 0));
				}

				Vector3 newVec = clickPositionOnPlane - center;
				Vector3 oldVec = transform.position - center;

				angle = Vector3.Angle (clickPositionOnPlane - center, transform.position - center);

				Vector3 cross = Vector3.Cross(oldVec, newVec);

				if (cross.y < 0)
				{
					angle = -angle;
				}

				transform.parent.RotateAround(center, transform.up, angle);
				break;
			}
		}
	}
}
