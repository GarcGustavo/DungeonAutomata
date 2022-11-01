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

		//Testing Bresenhams Line Algorithm
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
		//TODO: Refactor/fix - bug where cellgrid positions not being the same as world positions
		private static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
		
		//TODO: delete after testing custom implementation
		/// <summary>
		///     The plot function delegate
		/// </summary>
		/// <param name="x">The x co-ord being plotted</param>
		/// <param name="y">The y co-ord being plotted</param>
		/// <returns>True to continue, false to stop the algorithm</returns>
		public delegate bool PlotFunction(int x, int y);

		/// <summary>
		///     Plot the line from (x0, y0) to (x1, y1)
		/// </summary>
		/// <param name="x0">The start x</param>
		/// <param name="y0">The start y</param>
		/// <param name="x1">The end x</param>
		/// <param name="y1">The end y</param>
		/// <param name="plot">The plotting function (if this returns false, the algorithm stops early)</param>
		public static void Line(int x0, int y0, int x1, int y1, PlotFunction plot)
		{
			var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				Swap(ref x0, ref y0);
				Swap(ref x1, ref y1);
			}

			if (x0 > x1)
			{
				Swap(ref x0, ref x1);
				Swap(ref y0, ref y1);
			}

			int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX / 2, ystep = y0 < y1 ? 1 : -1, y = y0;

			for (var x = x0; x <= x1; ++x)
			{
				if (!(steep ? plot(y, x) : plot(x, y))) return;
				err = err - dY;
				if (err < 0)
				{
					y += ystep;
					err += dX;
				}
			}
		}
		
		#endregion
	}
}