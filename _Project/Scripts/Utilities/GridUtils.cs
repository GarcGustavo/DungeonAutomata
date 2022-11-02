using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public static class GridUtils
	{
		/// <summary>
		///     Static cell grid methods commonly used by grid components
		/// </summary>
		
		#region DOTWEENFACADE
		//Tween facade for grid movement
		public static IEnumerator PunchToPosition(Transform transform, Vector3 src, Vector3 target, float duration)
		{
			if(DOTween.IsTweening(transform))
				DOTween.Kill(transform);
			var direction = Vector3.zero;
			direction.x = (int)(target.x - src.x);
			direction.y = (int)(target.y - src.y);

			transform.DOPunchPosition(direction, duration);
			yield return null;
		}
		
		public static IEnumerator MoveToPosition(Transform transform, Vector3 targetPosition, float duration)
		{
			if(DOTween.IsTweening(transform))
				DOTween.Kill(transform);
			transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
			yield return null;
		}
		
		#endregion
		
		#region GRIDCELLAPI
		//General cell search methods
		public static int GetCellDistance(Vector3Int pos1, Vector3Int pos2)
		{
			var x = Mathf.Abs(pos1.x - pos2.x);
			var y = Mathf.Abs(pos1.y - pos2.y);
			return x + y;
		}

		public static List<Vector3Int> GetPositionsInRadius(Vector3Int pos, int radius)
		{
			var cellList = new List<Vector3Int>();
			for (var i = pos.x - radius; i <= pos.x + radius; i++)
			{
				for (var j = pos.y - radius; j <= pos.y + radius; j++)
				{
					if (i != pos.x || j != pos.y)
					{
						cellList.Add(new Vector3Int(i, j, 0));
					}
				}
			}
			return cellList;
		}

		public static List<CellData> GetCellsInRadius(Vector3Int pos, int radius, CellData[,] cellMap)
		{
			var cellList = new List<CellData>();
			for (var i = pos.x - radius; i <= pos.x + radius; i++)
			{
				for (var j = pos.y - radius; j <= pos.y + radius; j++)
				{
					if (i != pos.x || j != pos.y)
					{
						if(IsInMapRange(pos, cellMap))
							cellList.Add(cellMap[pos.x, pos.y]);
					}
				}
			}
			return cellList;
		}

		public static List<Vector3Int> GetPositionsInShape(Vector3Int origin, int[,] shape)
		{
			List<Vector3Int> shapeCells = new List<Vector3Int> ();
			for(int i = 0; i < shape.GetLength(0); i++)
			{
				for(int j = 0; j < shape.GetLength(1); j++)
				{
					if(shape[i,j] != 0)
					{
						shapeCells.Add(new Vector3Int(origin.x + i, origin.y + j, 0));
					}
				}
			}
			return shapeCells;
		}
		public static List<CellData> GetCellsInShape(Vector3Int origin, int[,] shape, CellData[,] cellMap)
		{
			List<CellData> shapeCells = new List<CellData> ();
			for (int i = 0; i < shape.GetLength(0); i++)
			{
				for (int j = 0; j < shape.GetLength(1); j++)
				{
					if (shape[i,j] != 0 && IsInMapRange(origin.x + i, origin.y + j, cellMap))
					{
						shapeCells.Add(cellMap[origin.x + i, origin.y + j]);
					}
				}
			}
			return shapeCells;
		}

		public static List<CellData> GetAdjacentCells(Vector3Int position, CellData[,] cellMap)
		{
			var neighbors = new List<CellData>();
			var up = position + Vector3Int.up;
			var down = position + Vector3Int.down;
			var left = position + Vector3Int.left;
			var right = position + Vector3Int.right;
			
			if(IsInMapRange(up, cellMap))
				neighbors.Add(cellMap[up.x, up.y]);
			if(IsInMapRange(down, cellMap))
				neighbors.Add(cellMap[down.x, down.y]);
			if(IsInMapRange(left, cellMap))
				neighbors.Add(cellMap[left.x, left.y]);
			if(IsInMapRange(right, cellMap))
				neighbors.Add(cellMap[right.x, right.y]);
			
			return neighbors;
		}

		//Gets neighbor cells, including diagonals
		public static List<CellData> GetNeighborCells(Vector3Int pos, CellData[,] cellMap, CellTypes cellType)
		{
			var cellList = new List<CellData>();
			for (var i = pos.x - 1; i <= pos.x + 1; i++)
			{
				for (var j = pos.y - 1; j <= pos.y + 1; j++)
				{
					if (i != pos.x || j != pos.y)
					{
						if(IsInMapRange(pos, cellMap) && cellMap[i,j].cellType == cellType)
							cellList.Add(cellMap[i,j]);
					}
				}
			}
			return cellList;
		}

		public static bool IsInMapRange<T>(Vector3Int position, T[,] map) 
		{
			return position.x >= 0 
			       && position.x < map.GetLength(0) 
			       && position.y >= 0 
			       && position.y < map.GetLength(1);
		}

		public static bool IsInMapRange<T>(int x, int y, T[,] map) 
		{
			return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
		}

		//Bresenhams Line Algorithm
		public static List<Vector3Int> GetLineOfSight(Vector3Int source, Vector3Int target)
		{
			List<Vector3Int> line = new List<Vector3Int> ();

			int x = source.x;
			int y = source.y;

			int dx = target.x - source.x;
			int dy = target.y - source.y;

			bool inverted = false;
			int step = Math.Sign (dx);
			int gradientStep = Math.Sign (dy);

			int longest = Mathf.Abs (dx);
			int shortest = Mathf.Abs (dy);

			if (longest < shortest) {
				inverted = true;
				longest = Mathf.Abs(dy);
				shortest = Mathf.Abs(dx);

				step = Math.Sign (dy);
				gradientStep = Math.Sign (dx);
			}

			int gradientAccumulation = longest / 2;
			for (int i =0; i < longest; i ++) {
				line.Add(new Vector3Int(x,y));

				if (inverted) {
					y += step;
				}
				else {
					x += step;
				}

				gradientAccumulation += shortest;
				if (gradientAccumulation >= longest) {
					if (inverted) {
						x += gradientStep;
					}
					else {
						y += gradientStep;
					}
					gradientAccumulation -= longest;
				}
			}

			return line;
		}
		
		#endregion
	}
}