using System;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Controllers
{
	public class TopDownController : MonoBehaviour
	{
		[SerializeField] float _speed = 5f;
		[SerializeField] float _rotationSpeed = 5f;
		
		
		private void Awake()
		{
			
		}
		
		private void Update()
		{
			Move();
		}

		private void Move()
		{
			var input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
			var direction = transform.TransformDirection(input);
			var velocity = direction * _speed;
			var movement = velocity * Time.deltaTime;
			transform.position += movement;
		}

		private void Rotate()
		{
			var rotation = Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime;
			transform.Rotate(0, rotation, 0);
		}
	}
}