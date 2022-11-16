using DungeonAutomata._Project.Scripts.Utilities;
using UnityEngine;

namespace DungeonAutomata._Project.Editor
{
	//[CustomEditor(typeof(TileMapGenerator))]
	public class MapGeneratorEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Generate Map"))
			{
				var generator = target as MapGenerator;
				if (generator != null)
				{
					generator.GenerateSpriteMap();
				} 
			}
			if (GUILayout.Button("Clear Map"))
			{
				var generator = target as MapGenerator;
				if (generator != null) generator.ClearMap();
			}
			base.OnInspectorGUI();
		}
	}
}