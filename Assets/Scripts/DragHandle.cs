using UnityEngine;
using System.Collections;

public enum DraggableBehavior
{
	Forward,
	Wheel
}

public class DragHandle : MonoBehaviour {
	public DraggableBehavior behavior = DraggableBehavior.Forward;
	public Unit parentUnit;

	public void Drag(Ray ray)
	{
		float clickDistance;
		Plane plane = new Plane(transform.up, transform.position);
		Vector3 clickPositionOnPlane;

		if (parentUnit.lastDragHandle != this)
		{
			parentUnit.lastDragHandle = this;

			parentUnit.lastPosition = parentUnit.transform.position;
			parentUnit.lastRotationAngle = parentUnit.transform.rotation.eulerAngles.y;
			parentUnit.currentRotationAngle = 0;

			parentUnit.movementRemaining = parentUnit.movementRemaining + parentUnit.movementUsed;
			parentUnit.movementUsed = 0;
		}

		if (plane.Raycast(ray, out clickDistance))
		{
			clickPositionOnPlane = ray.GetPoint (clickDistance);

			switch (behavior)
			{
			case DraggableBehavior.Forward:
				Vector3 newOffset = Vector3.Project(clickPositionOnPlane - transform.position, transform.parent.transform.forward);

				transform.parent.Translate (newOffset, Space.World);

				if (Vector3.Dot (transform.parent.position - parentUnit.lastPosition, parentUnit.transform.forward) < 0)
				{
					transform.parent.position = parentUnit.lastPosition;
				}

				break;

			case DraggableBehavior.Wheel:
				float radius = transform.parent.GetComponent<Unit>().Files;
				Vector3 center;
				float angle;
				bool isLeft = transform.localPosition.x < 0;

				if (isLeft)
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

				parentUnit.currentRotationAngle += angle;

				if (isLeft)
				{
					if (parentUnit.currentRotationAngle < 0)
					{
						angle -= parentUnit.currentRotationAngle;
						parentUnit.currentRotationAngle = 0;
					}
					else if (parentUnit.currentRotationAngle > 180)
					{
						angle -= parentUnit.currentRotationAngle - 180;
						parentUnit.currentRotationAngle = 180;
					}
				}
				else
				{
					if (parentUnit.currentRotationAngle > 0)
					{
						angle -= parentUnit.currentRotationAngle;
						parentUnit.currentRotationAngle = 0;
					}
					else if (parentUnit.currentRotationAngle < -180)
					{
						angle -= parentUnit.currentRotationAngle - 180;
						parentUnit.currentRotationAngle = -180;
					}
				}

				transform.parent.RotateAround(center, transform.up, angle);
				break;
			}
		}
	}
}
