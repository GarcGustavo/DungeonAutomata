using System;
using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.GridComponents;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class MapHighlighter : MonoBehaviour
	{
		[SerializeField] private bool debug = false;
		private Camera _camera;

		// Unity event functions
		//-------------------------------------------------------------------
		private void Awake()
		{
			_camera = Camera.main;
		}

		private void FixedUpdate()
		{
			//Highlight tile under mouse
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (debug)
			{
				Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
			}
			if (Physics.Raycast(ray, out hit))
			{
				var mouseWorldPos = hit.point;
				MapManager.Instance.HighLightCell(Vector3Int.FloorToInt(mouseWorldPos));
			}
		}
	}
}