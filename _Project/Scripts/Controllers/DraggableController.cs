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
		private Vector3 _mapPos = Vector3.zero;
		private Camera _camera;
		private Vector3 _spriteOffset = Vector3.zero;
		void Start()
		{
			_unit = GetComponent<IUnit>();
			_renderer = GetComponentInChildren<SpriteRenderer>();
			_camera = Camera.main;
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
			var cam = Camera.main;
			var pos = transform.position;
			_distance = Vector3.Distance(pos, cam.transform.position);
			_dragging = true;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			Vector3 rayPoint = ray.GetPoint(_distance);
			_startDist = pos - rayPoint;
		}
 
		void OnMouseUp()
		{
			_dragging = false;
			//_unit.SetPosition(Vector3Int.FloorToInt(_rayPoint + _startDist));
			//_unit.SetPosition(Vector3Int.RoundToInt(_rayPoint + _startDist));
			
			_unit.SetPosition(Vector3Int.FloorToInt(_mapPos));
		}
 
		void FixedUpdate()
		{
			if (_dragging)
			{
				Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit))
				{
					var mouseWorldPos = hit.point;
					var worldPos = new Vector3(mouseWorldPos.x - .5f, mouseWorldPos.y - .5f, 0);
					_mapPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
					transform.position = worldPos;
					Debug.Log("dropping at mapPos: " + _mapPos);
				}
				//_rayPoint = ray.GetPoint(_distance);
				//var finalPos = _rayPoint + _startDist;
				//transform.position = new Vector3(finalPos.x, finalPos.y, _zAxisLock);
			}
		}
	}
}