using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.DialogueSystem.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public static class Utils
	{
		// Return or cache scene's main camera
		private static Camera _camera;

		// Return or cache new WaitForSeconds object (creating new ones is performance-expensive)
		private static readonly Dictionary<float, WaitForSeconds> _waitDict = new();

		// Check if mouse is over a UI element
		private static PointerEventData _currentPositionEvent;
		private static List<RaycastResult> _results;

		public static Camera MainCamera()
		{
			if (_camera == null) _camera = Camera.main;
			return _camera;
		}

		public static WaitForSeconds GetWaitForSeconds(float seconds)
		{
			if (_waitDict.TryGetValue(seconds, out var wait_for_seconds))
				return wait_for_seconds;
			_waitDict[seconds] = new WaitForSeconds(seconds);
			return _waitDict[seconds];
		}

		public static bool IsMouseOverUI()
		{
			_currentPositionEvent = new PointerEventData(EventSystem.current)
			{
				position = Input.mousePosition
			};
			_results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(_currentPositionEvent, _results);
			return _results.Count > 0;
		}

		// Translate a point from canvas screen space to world space (useful for spawning/dragging/etc)
		public static Vector2 GetCanvasElementWorldPosition(RectTransform canvas_element)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas_element,
				canvas_element.position, MainCamera(), out var position);
			return position;
		}

		// Destroy all children of a transform
		public static void DestroyChildren(this Transform transform)
		{
			foreach (Transform child in transform)
				Object.Destroy(child.gameObject);
		}

		// Sets layers for child objects according to hierarchy in scene
		public static void SetLayerRecursively(GameObject obj, int layer)
		{
			obj.layer = layer;

			foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, layer);
		}
		
		//Displays info next to mouse position while keeping it on-screen
		public static Vector3 GetMousePopup(float height, float width)
		{
			var popupPosition = Input.mousePosition;
			var offset = Vector3.zero;
			offset.x = width / 2 + 5;
			offset.y = height / 2 + 5;
			if (popupPosition.x + width > Screen.width)
				offset.x = -offset.x;
			if (popupPosition.y + height > Screen.height)
				offset.y = -offset.y;
			popupPosition += offset;
			return popupPosition;
		}
		public static IActor CastInteractionRay(Vector3 start, Vector3 direction, int interactableLayerMask)
		{
			Ray ray = new Ray(start, direction);
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);
		
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, interactableLayerMask))
			{
				var hitObject = hit.transform.gameObject;
				var actor = hitObject.GetComponent<IActor>();
				if (actor != null)
				{
					Debug.Log("Hit " + actor);
					return actor;
				}
			}
			return null;
		}
		
	}
}