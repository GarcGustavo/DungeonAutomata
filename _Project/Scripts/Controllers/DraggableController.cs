using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Controllers
{
 
	class DraggableController : MonoBehaviour
	{
		private Color _mouseOverColor = Color.green;
		private Color _originalColor = Color.white;
		private bool _dragging = false;
		private float _distance;
		private Vector3 _startDist;
		//private IUnit _unit;
		private IUnit _unit;
		private SpriteRenderer _renderer;
		private Vector3 _rayPoint = Vector3.zero;
		void Start()
		{
			_unit = GetComponent<IUnit>();
			_renderer = GetComponentInChildren<SpriteRenderer>();
		}
   
		void OnMouseEnter()
		{
			_renderer.material.color = _mouseOverColor;
		}
 
		void OnMouseExit()
		{
			_renderer.material.color = _originalColor;
		}
 
		void OnMouseDown()
		{
			_distance = Vector3.Distance(transform.position, Camera.main.transform.position);
			_dragging = true;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 rayPoint = ray.GetPoint(_distance);
			_startDist = transform.position - rayPoint;
		}
 
		void OnMouseUp()
		{
			_dragging = false;
			_unit.SetPosition(Vector3Int.FloorToInt(_rayPoint + _startDist));
		}
 
		void Update()
		{
			if (_dragging)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				_rayPoint = ray.GetPoint(_distance);
				transform.position = _rayPoint + _startDist;
			}
		}
	}
}