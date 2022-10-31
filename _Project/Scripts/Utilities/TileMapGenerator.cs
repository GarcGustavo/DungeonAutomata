using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class TileMapGenerator : MonoBehaviour
	{
		[SerializeField] private Tilemap targetMap;
		[SerializeField] private Tilemap highlightTilemap;
		[SerializeField] private float randomFillPercent = 50f;
		[SerializeField] private int width;
		[SerializeField] private int height;
		[SerializeField] private int seed;
		[SerializeField] private int circleRadius = 5;
		[SerializeField] private bool randomSeed = true;

		[SerializeField] private CellData wallCell;
		[SerializeField] private CellData groundCell;
		[SerializeField] private CellData playerSpawnCell;
		[SerializeField] private CellData spawnCell;
		[SerializeField] private CellData exitCell;
		[SerializeField] private CellData itemCell;
		[SerializeField] private CellData keyCell;
		private Vector3Int _playerSpawnPosition;
		private List<Vector3Int> _enemySpawnPositions;
		private List<Vector3Int> _itemSpawnPositions;
		
		[SerializeField] private  int spawnRadius = 4;
		[SerializeField] private  int enemyAmount = 10;
		[SerializeField] private  int itemAmount = 10;

		//Cellular automata method
		private CellData[,] _map;
		private CellData[,] _mapBuffer;
		//Generating base ground/wall map to initialize main CellData map
		private int[,] _rooms;
		//private int[,] _obstacles;
		//private int[,] _playerSpawnPoints;
		//private int[,] _enemySpawnPoints;
		//private int[,] _itemSpawnPoints;
		//private int[,] _exitPoints;
		//private int[,] _groundMap;

		/// <summary>
		///     Cellular automata generation
		/// </summary>
		//[ContextMenu("Generate Map")]
		[Button]
		public void GenerateMap()
		{
			if (randomSeed)
			{
				seed = (int)DateTime.Now.Ticks;
			}
			_map = new CellData[width,height];
			targetMap.size = new Vector3Int(width, height, 1);
			targetMap.origin = new Vector3Int(0, 0, 0);
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			// Adding 2 to width and height to account for the border walls
			RandomFillMap();
			ApplySmooth();
			ProcessMap();
			CreateMapBorders();
			SetSpawns();
			//DrawBaseTilemap();
		}
		
		public Tilemap GetTileMap()
		{
			return targetMap;
		}
		
		public CellData[,] GetGridMap()
		{
			return _map;
		}
		
		public Tilemap GetHighlightMap()
		{
			return highlightTilemap;
		}
		
		
		private void RandomFillMap()
		{
			var prng = new Random(seed);
			//var vectorPosition = new Vector3Int(0, 0, 0);
			
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					if (x == 0 || x == width-1 || y == 0 || y == height-1) 
					{
						var newTile = Instantiate(wallCell);
						_map[x,y] = newTile;
						//newTile.gridPosition = new Vector3Int(x, y, 0);
						targetMap.SetTile(new Vector3Int(x, y, 0), newTile);
					}
					else
					{
						var newTile = Instantiate(prng.Next(0, 100) < randomFillPercent ? wallCell : groundCell);
						_map[x, y] = newTile;
						//newTile.gridPosition = new Vector3Int(x, y, 0);
						targetMap.SetTile(new Vector3Int(x, y, 0), newTile);
					}
				}
			}
		}

		//Cellular Automata ruleset
		private void ApplySmooth()
		{
			_mapBuffer = new CellData[width, height];
			_mapBuffer = _map;
			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					var neighborWalls = GetNeighborWalls(i, j);
					if (neighborWalls > 4)
					{
						var newTile = Instantiate(wallCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = new Vector3Int(i, j, 0);
						targetMap.SetTile(new Vector3Int(i, j, 0), newTile);
					}
					else if (neighborWalls < 4)
					{
						var newTile = Instantiate(groundCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = new Vector3Int(i, j, 0);
						targetMap.SetTile(new Vector3Int(i, j, 0), groundCell);
					}
				}
			}
			_map = _mapBuffer;
		}
		
		private void SetSpawns()
		{
			_mapBuffer = _map;
			_playerSpawnPosition = new Vector3Int(0,0,0);
			_enemySpawnPositions = new List<Vector3Int>();
			_itemSpawnPositions = new List<Vector3Int>();
			var playerSet = false;
			var exitSet = false;
			var keySet = false;
			var exitPos = new Vector3Int(0,0,0);
			var enemyCount = 0;
			var itemCount = 0;
			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					if (playerSet 
					    && exitSet 
					    && keySet 
					    && enemyCount >= enemyAmount 
					    && itemCount >= itemAmount)
					{
						break;
					}

					if (_mapBuffer[i, j].cellType != CellTypes.Ground) continue;
					var pos = new Vector3Int(i, j, 0);
					var neighbors = GetCellsInRadius(pos, spawnRadius);
					var playerDist = playerSet?GetCellDistance(_playerSpawnPosition, pos):0;
					var exitDist = exitSet?GetCellDistance(exitPos, pos):0;
						
					if (!playerSet)
					{
						var newTile = Instantiate(playerSpawnCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						targetMap.SetTile(pos, newTile);
						_playerSpawnPosition = pos;
						playerSet = true;
					}
					else if (!exitSet 
					         && playerDist > width/2 
					         && playerDist > height/2)
					{
						var newTile = Instantiate(exitCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						targetMap.SetTile(pos, newTile);
						exitPos = pos;
						exitSet = true;
					}
					else if (enemyCount<enemyAmount
					         && playerDist > width/3 
					         && playerDist > height/3
					         && !neighbors.Find(x => 
						         x.cellType is CellTypes.EnemySpawn))
					{
						var newTile = Instantiate(spawnCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						targetMap.SetTile(pos, newTile);
						enemyCount++;
						_enemySpawnPositions.Add(pos);
					}
					else if (itemCount<itemAmount
					         && !neighbors.Find(x => 
						         x.cellType is CellTypes.ItemSpawn))
					{
						var newTile = Instantiate(itemCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						targetMap.SetTile(pos, newTile);
						itemCount++;
						_itemSpawnPositions.Add(pos);
					}
					else if (!keySet
					         && playerDist > width/2
					         && exitDist > width/4
					         && !neighbors.Find(x => 
						         x.cellType is CellTypes.ItemSpawn))
					{
						var newTile = Instantiate(keyCell);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						targetMap.SetTile(pos, newTile);
						keySet = true;
					}
				}
			}
		}

		public Vector3Int GetPlayerSpawnPosition()
		{
			return _playerSpawnPosition;
		}

		public List<Vector3Int> GetEnemySpawnPositions()
		{
			return _enemySpawnPositions;
		}

		public List<Vector3Int> GetItemSpawnPositions()
		{
			return _itemSpawnPositions;
		}

		public List<CellData> GetCellsInRadius(Vector3Int pos, int radius)
		{
			var cellList = new List<CellData>();
			for (var i = pos.x - radius; i <= pos.x + radius; i++)
			{
				for (var j = pos.y - radius; j <= pos.y + radius; j++)
				{
					if (i >= 0 && i < width && j >= 0 && j < height)
					{
						if (i != pos.x || j != pos.y)
						{
							cellList.Add(_mapBuffer[i, j]);
						}
					}
				}
			}
			return cellList;
		}
		public int GetCellDistance(Vector3Int pos1, Vector3Int pos2)
		{
			var x = Mathf.Abs(pos1.x - pos2.x);
			var y = Mathf.Abs(pos1.y - pos2.y);
			return x + y;
		}

		private int GetNeighborWalls(int x, int y)
		{
			var wallCount = 0;
			for (var i = x - 1; i <= x + 1; i++)
			{
				for (var j = y - 1; j <= y + 1; j++)
				{
					if (i >= 0 && i < width && j >= 0 && j < height)
					{
						if (i != x || j != y)
						{
							wallCount += (_map[i, j].cellType == CellTypes.Wall)?1:0;
						} 
					}
					else
					{
						wallCount++;
					}
				}
			}
			return wallCount;
		}

		private void CreateMapBorders()
		{
			CellData[,] borderedMap = new CellData[width + 2, height + 2];

			for (int x = 0; x < borderedMap.GetLength(0); x++)
			{
				for (int y = 0; y < borderedMap.GetLength(1); y++)
				{
					if (x > 0 && x < width && y > 0 && y < height)
					{
						borderedMap[x, y] = _map[x, y];
					}
					else
					{
						var newTile = Instantiate(wallCell);
						//newTile.gridPosition = new Vector3Int(x, y, 0);
						targetMap.SetTile(new Vector3Int(x, y, 0), newTile);
						borderedMap[x, y] = newTile;
					}
				}
			}
			_map = borderedMap;
		}
		private void DrawBaseTilemap()
		{
			targetMap.size = new Vector3Int(width, height, 1);
			targetMap.origin = new Vector3Int(0, 0, 0);
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					//Turn into prefab-based map and set sprites/colors at runtime
					if (_map[i, j].cellType == CellTypes.Wall)
					{
						targetMap.SetTile(_map[i,j].gridPosition, wallCell);
						_map[i, j] = targetMap.GetTile(_map[i, j].gridPosition) as CellData;
					}
					else  if(_map[i,j].cellType == CellTypes.Ground)
					{
						targetMap.SetTile(_map[i,j].gridPosition, groundCell);
						_map[i, j] = targetMap.GetTile(_map[i, j].gridPosition) as CellData;
					}
					else if(_map[i,j].cellType == CellTypes.PlayerSpawn)
					{
						targetMap.SetTile(_map[i,j].gridPosition, playerSpawnCell);
						_map[i, j] = targetMap.GetTile(_map[i, j].gridPosition) as CellData;
					}
					else if(_map[i,j].cellType == CellTypes.EnemySpawn)
					{
						targetMap.SetTile(_map[i,j].gridPosition, spawnCell);
						_map[i, j] = targetMap.GetTile(_map[i, j].gridPosition) as CellData;
					}
					else if(_map[i,j].cellType == CellTypes.Exit)
					{
						targetMap.SetTile(_map[i,j].gridPosition, exitCell);
						_map[i, j] = targetMap.GetTile(_map[i, j].gridPosition) as CellData;
					}
				}
			}
		}

		void ProcessMap() {
			List<List<Vector3Int>> wallRegions = GetRegions(CellTypes.Wall);
			int wallThresholdSize = 50;

			foreach (List<Vector3Int> wallRegion in wallRegions) {
				if (wallRegion.Count < wallThresholdSize) {
					foreach (Vector3Int tile in wallRegion) {
						var newTile = Instantiate(groundCell);
						_map[tile.x, tile.y] = newTile;
						//newTile.gridPosition = tile;
						targetMap.SetTile(tile, newTile);
					}
				}
			}

			List<List<Vector3Int>> roomRegions = GetRegions (0);
			int roomThresholdSize = 50;
			List<GridRoom> survivingRooms = new List<GridRoom> ();
		
			foreach (List<Vector3Int> roomRegion in roomRegions) {
				if (roomRegion.Count < roomThresholdSize) {
					foreach (Vector3Int tile in roomRegion) {
						var newTile = Instantiate(wallCell);
						_map[tile.x, tile.y] = newTile;
						//newTile.gridPosition = tile;
						targetMap.SetTile(tile, newTile);
					}
				}
				else {
					survivingRooms.Add(new GridRoom(roomRegion, _map));
				}
			}
			survivingRooms.Sort ();
			survivingRooms [0].isMainRoom = true;
			survivingRooms [0].isAccessibleFromMainRoom = true;

			ConnectClosestRooms (survivingRooms);
		}

		void ConnectClosestRooms(List<GridRoom> allRooms, bool forceAccessibilityFromMainRoom = false) {

			List<GridRoom> roomListA = new List<GridRoom> ();
			List<GridRoom> roomListB = new List<GridRoom> ();

			if (forceAccessibilityFromMainRoom) {
				foreach (GridRoom room in allRooms) {
					if (room.isAccessibleFromMainRoom) {
						roomListB.Add (room);
					} else {
						roomListA.Add (room);
					}
				}
			} else {
				roomListA = allRooms;
				roomListB = allRooms;
			}

			int bestDistance = 0;
			Vector3Int bestTileA = new Vector3Int();
			Vector3Int bestTileB = new Vector3Int();
			GridRoom bestRoomA = new GridRoom();
			GridRoom bestRoomB = new GridRoom();
			bool possibleConnectionFound = false;

			foreach (GridRoom roomA in roomListA) {
				if (!forceAccessibilityFromMainRoom) {
					possibleConnectionFound = false;
					if (roomA.connectedRooms.Count > 0) {
						continue;
					}
				}

				foreach (GridRoom roomB in roomListB) {
					if (roomA == roomB || roomA.IsConnected(roomB)) {
						continue;
					}
				
					for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++) {
						for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++) {
							Vector3Int tileA = roomA.edgeTiles[tileIndexA];
							Vector3Int tileB = roomB.edgeTiles[tileIndexB];
							int distanceBetweenRooms = (int)(Mathf.Pow (tileA.x-tileB.x,2) 
							                                 + Mathf.Pow (tileA.y-tileB.y,2));

							if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
								bestDistance = distanceBetweenRooms;
								possibleConnectionFound = true;
								bestTileA = tileA;
								bestTileB = tileB;
								bestRoomA = roomA;
								bestRoomB = roomB;
							}
						}
					}
				}
				if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
					CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
				}
			}

			if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
				ConnectClosestRooms(allRooms, true);
			}

			if (!forceAccessibilityFromMainRoom) {
				ConnectClosestRooms(allRooms, true);
			}
		}
		
		void CreatePassage(GridRoom roomA, GridRoom roomB, Vector3Int tileA, Vector3Int tileB) {
			GridRoom.ConnectRooms (roomA, roomB);
			List<Vector3Int> line = GridUtils.GetLineOfSight(tileA, tileB);
			foreach (Vector3Int c in line) {
				SurroundCell(c,circleRadius, groundCell);
			}
		}

		void SurroundCell(Vector3Int pos, int radius, CellData cell) {
			for (int x = -radius; x <= radius; x++) {
				for (int y = -radius; y <= radius; y++) {
					if (x*x + y*y <= radius*radius) {
						int drawX = pos.x + x;
						int drawY = pos.y + y;
						if (IsInMapRange(drawX, drawY))
						{
							var newTile = Instantiate(cell);
							_map[drawX, drawY] = newTile;
							//newTile.gridPosition = new Vector3Int(drawX, drawY, 0);
							targetMap.SetTile(new Vector3Int(drawX, drawY, 0), newTile);
						}
					}
				}
			}
		}

		private List<List<Vector3Int>> GetRegions(CellTypes cellTypes) 
		{
			List<List<Vector3Int>> regions = new List<List<Vector3Int>> ();
			int[,] mapFlags = new int[width,height];
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					if (mapFlags[x,y] == 0 && _map[x,y].cellType == cellTypes) 
					{
						List<Vector3Int> newRegion = GetRegionTiles(x,y);
						regions.Add(newRegion);

						foreach (Vector3Int tile in newRegion) 
						{
							mapFlags[tile.x, tile.y] = 1;
						}
					}
				}
			}
			return regions;
		}

		private List<Vector3Int> GetRegionTiles(int startX, int startY)
		{
			List<Vector3Int> tiles = new List<Vector3Int> ();
			int[,] mapFlags = new int[width,height];
			CellTypes cellTypes = _map[startX, startY].cellType;

			Queue<Vector3Int> queue = new Queue<Vector3Int> ();
			queue.Enqueue (new Vector3Int (startX, startY));
			mapFlags[startX, startY] = 1;

			while (queue.Count > 0) 
			{
				Vector3Int tile = queue.Dequeue();
				tiles.Add(tile);

				for (int x = tile.x - 1; x <= tile.x + 1; x++) 
				{
					for (int y = tile.y - 1; y <= tile.y + 1; y++) 
					{
						if (IsInMapRange(x,y) && (y == tile.y || x == tile.x)) 
						{
							if (mapFlags[x,y] == 0 && _map[x,y].cellType == cellTypes) 
							{
								mapFlags[x,y] = 1;
								queue.Enqueue(new Vector3Int(x,y));
							}
						}
					}
				}
			}
			return tiles;
		}

		private bool IsInMapRange(int x, int y) 
		{
			return x >= 0 && x < width && y >= 0 && y < height;
		}

		[Button]
		public void ClearMap()
		{
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			
			highlightTilemap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
		}

		public void GetCell(Vector3Int cell)
		{
			
		}

		public void SwapCell(Vector3Int cell)
		{
			
		}
	}
}