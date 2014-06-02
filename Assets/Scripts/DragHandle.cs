using UnityEngine;
using System.Collections;
using System.Linq;

public enum DraggableBehavior
{
	Forward,
	Wheel
}

public class DragHandle : MonoBehaviour {
	public DraggableBehavior behavior = DraggableBehavior.Forward;
	public UnitMover parentUnitMover;
	public GameObject movementTrailContainer;

	private GameObject movementTrail;

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
				movementTrail = Instantiate (parentUnitMover.parentUnit.movementTrailStraight) as GameObject;

				movementTrail.transform.localPosition = parentUnitMover.transform.position;
				movementTrail.transform.localRotation = parentUnitMover.transform.rotation;

				movementTrail.transform.parent = movementTrailContainer.transform;
			}
			else
			{
				bool isLeft = transform.localPosition.x < 0;

				movementTrail = Instantiate (parentUnitMover.parentUnit.movementTrailCurved) as GameObject;

				var data = movementTrail.GetComponent<MovementTrailCurved>();

				data.direction = isLeft ? CurveDirection.Left : CurveDirection.Right;
				data.radius = parentUnitMover.parentUnit.Files * 0.5f;

				movementTrail.transform.parent = movementTrailContainer.transform;

				movementTrail.transform.localPosition = parentUnitMover.transform.localPosition;
				movementTrail.transform.localRotation = parentUnitMover.transform.localRotation;
			}
		}

		if (plane.Raycast(ray, out clickDistance))
		{
			clickPositionOnPlane = ray.GetPoint (clickDistance);

			float radius;

			switch (behavior)
			{
			case DraggableBehavior.Forward:
				Vector3 newOffset = Vector3.Project(clickPositionOnPlane - transform.position, parentUnitMover.transform.forward);

				float? maxDistance = null;
				float magnitude = newOffset.magnitude;
				radius = parentUnitMover.parentUnit.Files * 0.5f;

				if (magnitude > 0 && Vector3.Dot (newOffset, parentUnitMover.transform.forward) > 0)
				{
					Vector3 leftCorner = parentUnitMover.transform.position - parentUnitMover.transform.right * radius;
					float step = magnitude * 0.1f;

					if (step < 0.1f)
					{
						step = magnitude * 0.5f;
					}
					else if (step > 0.5f)
					{
						step = 0.5f;
					}

					for (float offsetFromFront = step; offsetFromFront <= magnitude; offsetFromFront += step)
					{
						Vector3 origin = leftCorner + parentUnitMover.transform.forward * offsetFromFront;
						Ray testRay = new Ray(origin, parentUnitMover.transform.right);

						if (Physics.Raycast (testRay, radius * 2))
						{
							maxDistance = offsetFromFront - step;
							break;
						}
					}

					if (maxDistance.HasValue)
					{
						newOffset = newOffset * (maxDistance.Value / magnitude);
					}
				}

				parentUnitMover.transform.Translate (newOffset, Space.World);

				if (Vector3.Dot (parentUnitMover.transform.position - parentUnitMover.lastPosition, parentUnitMover.transform.forward) < 0)
				{
					parentUnitMover.transform.position = parentUnitMover.lastPosition;
					parentUnitMover.movementUsed = 0;
				}
				else
				{
					parentUnitMover.movementUsed = (parentUnitMover.transform.position - parentUnitMover.lastPosition).magnitude;

					if (parentUnitMover.movementUsed > parentUnitMover.movementRemaining)
					{
						float dif = parentUnitMover.movementUsed - parentUnitMover.movementRemaining;

						parentUnitMover.movementUsed = parentUnitMover.movementRemaining;

						parentUnitMover.transform.Translate (-dif * parentUnitMover.transform.forward, Space.World);
					}
				}

				movementTrail.transform.localScale = new Vector3(1, 1, parentUnitMover.movementUsed);

				break;

			case DraggableBehavior.Wheel:
				radius = parentUnitMover.parentUnit.Files;
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
					center = parentUnitMover.transform.TransformPoint (transform.localPosition + new Vector3(radius, 0, 0));
				}
				else
				{
					center = parentUnitMover.transform.TransformPoint (transform.localPosition + new Vector3(-radius, 0, 0));
				}

				Vector3 newVec = clickPositionOnPlane - center;
				Vector3 oldVec = transform.position - center;

				angle = Vector3.Angle (clickPositionOnPlane - center, transform.position - center);

				Vector3 cross = Vector3.Cross(oldVec, newVec);

				if (cross.y < 0)
				{
					angle = -angle;
				}

				float degModifier = (isLeft ? -1.0f : 1.0f);

				if ((degModifier < 0 && angle > 0) || (degModifier > 0 && angle < 0))
				{
					for (int degrees = 0; degrees < maxAngle; ++degrees)
					{
						Ray testRay = new Ray(center, Quaternion.AngleAxis (degrees * -degModifier, parentUnitMover.transform.up) * parentUnitMover.transform.right * degModifier);

						var hits = Physics.RaycastAll (testRay, radius);

						if (hits.Any (hit => !hit.collider.transform.IsChildOf(parentUnitMover.parentUnit.transform)))
						{
							if (Mathf.Abs (angle) > degrees)
							{
								angle = -degModifier * degrees;
							}

							break;
						}
					}
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

				parentUnitMover.transform.RotateAround(center, transform.up, angle);

				movementTrail.GetComponent<MovementTrailCurved>().SetDegrees(Mathf.Abs ((int)parentUnitMover.currentRotationAngle));
				break;
			}
		}
	}
}
