using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Controllers.ParkourSystem
{
	public class RotateGun : MonoBehaviour
	{
		public GrappleHook grappling;

		private Quaternion desiredRotation;
		private float rotationSpeed = 5f;

		void Update() {
			if (!grappling.IsGrappling()) {
				desiredRotation = transform.parent.rotation;
			}
			else {
				desiredRotation = Quaternion.LookRotation(grappling.GetGrapplePoint() - transform.position);
			}

			transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
		}
	}
}