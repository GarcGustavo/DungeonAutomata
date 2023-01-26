using System;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities.SuspensionSystem
{
	[RequireComponent(typeof(Rigidbody))]
	public class Suspension : MonoBehaviour
	{
		private Rigidbody _rigidbody;
		[SerializeField] private Transform[] wheelTransforms; // vector force origins
		[SerializeField] private float distance = 1.0f; // distance to maintain from point of collision
		[SerializeField] private float springForce = 10.0f; // spring force
		[SerializeField] private float springDamping = 0.1f; // spring damping
		[SerializeField] private Vector3 velocity; // velocity of the spring movement
		[SerializeField] private float maxVelocity = 2.0f; // max velocity of the spring movement
		[SerializeField] private float maxDistance = 2.0f; // max distance of the spring movement

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void Start()
		{
			//throw new NotImplementedException();
		}

		private void FixedUpdate()
		{
			RaycastHit hit;
			foreach (var wheel in wheelTransforms)
			{
				var wheelPosition = wheel.position;
				if (Physics.Raycast(wheelPosition, Vector3.down, out hit))
				{
					Debug.DrawRay(wheelPosition, Vector3.down * hit.distance, Color.red);
					if (hit.distance <= maxDistance)
					{
						Vector3 targetPosition = hit.point + hit.normal * distance;
						Vector3 springForceVector = (targetPosition - wheelPosition) * springForce;
						velocity += springForceVector * Time.deltaTime;
						velocity -= velocity * springDamping * Time.deltaTime;
						velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
						_rigidbody.AddForceAtPosition(-velocity, wheelPosition);
					}
					//transform.position += velocity * Time.deltaTime;
				}
			}
		}
		private void OnDrawGizmos()
		{
			if (wheelTransforms == null) return;
			Gizmos.color = Color.green;
			foreach (Transform t in wheelTransforms)
			{
				Gizmos.DrawWireSphere(t.position, 0.1f);
				Gizmos.DrawRay(t.position, Vector3.down);
			}
		}
	}
}