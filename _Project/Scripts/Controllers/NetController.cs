using System.Security.Cryptography;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;

namespace DungeonAutomata._Project.Scripts.Controllers
{
	public class NetController : NetworkBehaviour
	{
		private void Awake() 
		{
			_rb = GetComponent<Rigidbody>();
			_rb.isKinematic = false;
			_cam = Camera.main;
			_cam.transform.SetParent(cameraParent);
			_cam.transform.localPosition = Vector3.zero;
			//TODO: refactor to get cinemachine virtual camera and set its params
		}
		
		public override void OnNetworkSpawn() 
		{
			if (!IsOwner) 
			{
				Destroy(this);
			}
		}

		private void Update() 
		{
			_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		}

		private void FixedUpdate() 
		{
			HandleMovement();
			HandleRotation();
		}

		#region Movement

		[SerializeField] private Transform cameraParent;
		[SerializeField] private float _acceleration = 80;
		[SerializeField] private float _maxVelocity = 10;
		private Vector3 _input;
		private Rigidbody _rb;

		private void HandleMovement() 
		{
			_rb.velocity += _input.normalized * (_acceleration * Time.deltaTime);
			_rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxVelocity);
		}

		#endregion

		#region Rotation

		[SerializeField] private float _rotationSpeed = 450;
		private Plane _groundPlane = new(Vector3.up, Vector3.zero);
		private Camera _cam;
		private CinemachineVirtualCamera _virtualCam;

		private void HandleRotation() 
		{
			var ray = _cam.ScreenPointToRay(Input.mousePosition);

			if (_groundPlane.Raycast(ray, out var enter)) {
				var hitPoint = ray.GetPoint(enter);

				var dir = hitPoint - transform.position;
				var rot = Quaternion.LookRotation(dir);

				transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
			}
		}

		#endregion
	}
}