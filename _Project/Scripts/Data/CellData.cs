using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
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
		[FormerlySerializedAs("isEmpty")] public bool isWalkable;
		public IUnit Occupant;
		public bool isometric = false;

		public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
		{
			//Used to update tilemap data with Cell object data
			tileData.sprite = cellSprite;
			gridPosition = isometric ? GridUtils.GetCartesianPos(position): position;
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