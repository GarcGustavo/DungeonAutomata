using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class ShapeParser
	{
		// Consider replacing tilemap with cell data array
		public List<Vector3Int> GetTileLocations(Vector3Int abilityPos, TextAsset shapeFile, Tilemap map,
			GameObject activeUnit)
		{
			var targetTiles = new List<Vector3Int>();
			int[,] shapeMap = new int[10,10];
			var shape = shapeFile.text.Split('\n');
			var i = 0;
			foreach (var row in shape)
			{
				var j = 0;
				foreach (var col in row.Trim().Split(" "))
				{
					shapeMap[i, j] = int.Parse(col.Trim());
					j++;
				}
				i++;
			}
			var empty = FindLocationsInArray(shapeMap, 0);
			var origin = FindLocationsInArray(shapeMap, 1);
			var effect = FindLocationsInArray(shapeMap, 2);
			//If 1 is found in shapeMap add the position to list
			//TODO: rework actual location logic
			if (origin != null)
			{
				foreach (var pos in origin)
				{
					var x = pos.x - 5;
					var y = pos.y - 5;
					var targetPos = new Vector3Int(abilityPos.x + x, abilityPos.y + y, 0);
					if (map.GetTile(targetPos) != null)
					{
						targetTiles.Add(targetPos);
					}
				}
			}
			else
			{
				Debug.Log("No origin found in shape file");
			}
			//shape parser logic, import text file and parse it into targeted tiles (1=tile, 0=no tile)
			return targetTiles;
		}

		private List<Vector3Int> FindLocationsInArray(int[,] shapeMap, int i)
		{
			throw new System.NotImplementedException();
		}
	}
}