using DungeonAutomata._Project.Scripts._Common;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Data
{
	[ManageableData]
	[CreateAssetMenu(fileName = "ShapeData", menuName = "ShapeData", order = 0)]
	public class ShapeDataEditor : SerializedScriptableObject
	{
		[SerializeField] private int range;
		[TableMatrix(HorizontalTitle = "Square Celled Matrix", SquareCells = true, DrawElementMethod = "DrawMatrix")]
		[SerializeField] private int[,] _shapeMatrix;
		public int[,] ShapeMatrix => _shapeMatrix;
		
		[OnInspectorInit]
		public void CreateData()
		{
			var radius = range * 2 + 1;
			_shapeMatrix ??= new int[radius, radius];
			
			var rect = new Rect(0, 0, radius, radius);
			for (int i = 0; i < radius; i++)
			{
				for (int j = 0; j < radius; j++)
				{
					var cell = (i == radius/2 && j == radius/2)? 2 : _shapeMatrix[i, j];
					_shapeMatrix[i, j] = DrawMatrix(rect, cell);
				}
			}
		}

		[Button]
		private void Clear()
		{
			var radius = range * 2 + 1;
			_shapeMatrix = new int[radius, radius];
			CreateData();
		}
		private static int DrawMatrix(Rect rect, int value)
		{
			var mouseInput = Event.current.type;
			
			if (rect.Contains(Event.current.mousePosition)
			    && mouseInput == EventType.MouseDown)
			{
				value = value switch
				{
					0 => 1,
					1 => 0,
					_ => 2
				};
				GUI.changed = true; 
				Event.current.Use();
			}
			var currentColor = (value switch
			{
				0 => Color.black,
				1 => Color.red,
				_ => Color.green
			});
			EditorGUI.DrawRect(rect.Padding(1), currentColor);
			return value;
		}

		private static void Redraw(Rect rect)
		{
			GUI.changed = true; 
			Event.current.Use();
			EditorGUI.DrawRect(rect.Padding(1), Color.black);
		}
	}
}