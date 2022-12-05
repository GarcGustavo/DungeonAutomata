using System;
using DG.Tweening;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Controllers
{
	[RequireComponent(typeof(Rigidbody))]
	public class TopDownController : MonoBehaviour
	{
		[SerializeField] float _speed = 5f;
		[SerializeField] float _rotationSpeed = 5f;
		[SerializeField] Rigidbody _rb;
		private Vector3 _direction;
		
		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			_direction = GetInput();
			MoveUnit(_direction);
		}

		public void MoveUnit(Vector3 input)
		{
			Debug.Log("input: " + input);
			_rb.MovePosition( _rb.position + input * _speed * Time.deltaTime);
		}

		public void Rotate()
		{
			var rotation = Input.GetAxisRaw("Horizontal") * _rotationSpeed * Time.deltaTime;
			transform.Rotate(0, rotation, 0);
		}

		private Vector3 GetInput()
		{
			return new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
		}
	}
}