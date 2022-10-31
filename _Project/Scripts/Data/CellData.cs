using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonAutomata._Project.Scripts.Data
{
	[ManageableData]
	[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "CellData", order = 0)]
	public class CellData : TileBase
	{
		[PreviewField(75, ObjectFieldAlignment.Left)]
		public Sprite cellSprite;
		public Vector3Int gridPosition;
		public CellTypes cellType;
		public bool isEmpty;
		public IUnit Occupant;

		public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
		{
			//Used to update tilemap data with Cell object data
			tileData.sprite = cellSprite;
			gridPosition = position;
		}

	}
	public enum CellTypes
	{
		Ground,
		Wall,
		Water,
		Door,
		Exit,
		Hazard,
		PlayerSpawn,
		KeySpawn,
		ItemSpawn,
		EnemySpawn
	}
}