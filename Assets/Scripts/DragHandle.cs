using UnityEngine;
using System.Collections;

public enum DraggableBehavior
{
	Forward,
	Wheel
}

public class DragHandle : MonoBehaviour {
	public DraggableBehavior behavior = DraggableBehavior.Forward;
	public UnitMover parentUnitMover;

	public void Drag(Ray ray)
	{
		float clickDistance;
		Plane plane = new Plane(transform.up, transform.position);
		Vector3 clickPositionOnPlane;

		if (parentUnitMover.lastDragHandle != this)
		{
			parentUnitMover.lastDragHandle = this;

			parentUnitMover.lastPosition = parentUnitMover.transform.position;
			parentUnitMover.lastRotationAngle = parentUnitMover.transform.rotation.eulerAngles.y;
			parentUnitMover.currentRotationAngle = 0;

			parentUnitMover.movementRemaining = parentUnitMover.movementRemaining - parentUnitMover.movementUsed;
			parentUnitMover.movementUsed = 0;

			if (behavior == DraggableBehavior.Forward)
			{
				var trailStraight = Instantiate (parentUnitMover.parentUnit.movementTrailStraight) as GameObject;

				trailStraight.transform.parent = parentUnitMover.parentUnit.transform;
			}
			else
			{
			}
		}

		if (plane.Raycast(ray, out clickDistance))
		{
			clickPositionOnPlane = ray.GetPoint (clickDistance);

			switch (behavior)
			{
			case DraggableBehavior.Forward:
				Vector3 newOffset = Vector3.Project(clickPositionOnPlane - transform.position, transform.parent.transform.forward);

				transform.parent.Translate (newOffset, Space.World);

				if (Vector3.Dot (transform.parent.position - parentUnitMover.lastPosition, parentUnitMover.transform.forward) < 0)
				{
					transform.parent.position = parentUnitMover.lastPosition;
					parentUnitMover.movementUsed = 0;
				}
				else
				{
					parentUnitMover.movementUsed = (transform.parent.position - parentUnitMover.lastPosition).magnitude;

					if (parentUnitMover.movementUsed > parentUnitMover.movementRemaining)
					{
						float dif = parentUnitMover.movementUsed - parentUnitMover.movementRemaining;

						parentUnitMover.movementUsed = parentUnitMover.movementRemaining;

						transform.parent.Translate (-dif * transform.parent.forward, Space.World);
					}
				}

				break;

			case DraggableBehavior.Wheel:
				float radius = parentUnitMover.parentUnit.Files;
				Vector3 center;
				float angle;
				bool isLeft = transform.localPosition.x < 0;
				float maxAngle = parentUnitMover.movementRemaining * Mathf.Rad2Deg / (parentUnitMover.parentUnit.Files * 0.5f);

				if (maxAngle > 180)
				{
					maxAngle = 180;
				}

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

				parentUnitMover.currentRotationAngle += angle;

				if (isLeft)
				{
					if (parentUnitMover.currentRotationAngle < 0)
					{
						angle -= parentUnitMover.currentRotationAngle;
						parentUnitMover.currentRotationAngle = 0;
					}
					else if (parentUnitMover.currentRotationAngle > maxAngle)
					{
						angle -= parentUnitMover.currentRotationAngle - maxAngle;
						parentUnitMover.currentRotationAngle = maxAngle;
					}
				}
				else
				{
					if (parentUnitMover.currentRotationAngle > 0)
					{
						angle -= parentUnitMover.currentRotationAngle;
						parentUnitMover.currentRotationAngle = 0;
					}
					else if (parentUnitMover.currentRotationAngle < -maxAngle)
					{
						angle -= parentUnitMover.currentRotationAngle + maxAngle;
						parentUnitMover.currentRotationAngle = -maxAngle;
					}
				}

				parentUnitMover.movementUsed = parentUnitMover.parentUnit.Files * Mathf.Deg2Rad * Mathf.Abs (parentUnitMover.currentRotationAngle) * 0.5f;

				transform.parent.RotateAround(center, transform.up, angle);
				break;
			}
		}
	}
}
