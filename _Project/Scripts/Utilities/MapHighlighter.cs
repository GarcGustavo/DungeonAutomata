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
			MapManager.Instance.HighLightCell(GridUtils.GetMouseCellPosition(_camera, true));
		}

	}
}