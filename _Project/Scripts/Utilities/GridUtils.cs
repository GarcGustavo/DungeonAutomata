using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class GridUtils
	{
		/// <summary>
		///     The plot function delegate
		/// </summary>
		/// <param name="x">The x co-ord being plotted</param>
		/// <param name="y">The y co-ord being plotted</param>
		/// <returns>True to continue, false to stop the algorithm</returns>
		public delegate bool PlotFunction(int x, int y);

		private static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

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
		
		public static IEnumerator MoveToPosition(Transform transform, Vector3 targetPosition, float duration)
		{
			if(DOTween.IsTweening(transform))
				DOTween.Kill(transform);
			transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
			yield return null;
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
	}
}