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
		/// <summary>
		///     Cellular automata generation
		/// </summary>
		
		//TODO: Minimize serialized fields
		[SerializeField] private Tilemap targetMap;
		[SerializeField] private Tilemap highlightTilemap;
		[SerializeField] private float randomFillPercent = 50f;
		[SerializeField] private int width;
		[SerializeField] private int height;
		[SerializeField] private int seed;
		[SerializeField] private int circleRadius = 5;
		[SerializeField] private int smoothingIterations = 5;
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

		//Cellular automata maps
		private CellData[,] _map;
		private CellData[,] _mapBuffer;
		private List<int[,]> _rooms;

		#region API
		
		[Button]
		public void GenerateMap()
		{
			if (randomSeed)
			{
				seed = (int)DateTime.Now.Ticks;
			}
			_map = new CellData[width,height];
			RandomFillMap();
			SmoothMap(smoothingIterations);
			GenerateRooms(5);
			//ProcessMap();
			//CreateMapBorders();
			//SetSpawns();
			DrawTilemap(_map);
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
		
		//Getters to be accessed through manager singleton in order to decouple map generator from other systems
		public Tilemap GetTileMap()
		{
			return targetMap;
		}
		
		public CellData[,] GetCellMap()
		{
			return _map;
		}
		
		public Tilemap GetHighlightMap()
		{
			return highlightTilemap;
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
		
		#endregion

		#region AUTOMATAGEN

		//Cellular Automata ruleset
		private void RandomFillMap()
		{
			var prng = new Random(seed);
			
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					if (x == 0 || x == width-1 || y == 0 || y == height-1) 
					{
						//Create borders
						var newTile = Instantiate(wallCell);
						_map[x,y] = newTile;
					}
					else
					{
						var newTile = Instantiate(prng.Next(0, 100) < randomFillPercent ? wallCell : groundCell);
						_map[x, y] = newTile;
					}
				}
			}
		}
		
		private void SmoothMap(int iterations)
		{
			for (int i = 0; i < iterations; i++)
			{
				_mapBuffer = _map;
				for (var x = 1; x < width-1; x++)
				{
					for (var y = 1; y < height-1; y++)
					{
						if (_mapBuffer[x, y].cellType == CellTypes.Ground)
						{
							var neighborGround = GridUtils.GetNeighborCells(
								new Vector3Int(x, y),
								_mapBuffer,
								CellTypes.Ground).Count;
							if (neighborGround < 4)
							{
								var newTile = Instantiate(wallCell);
								_map[x, y] = newTile;
							}
						}
						else
						{
							var neighborGround = GridUtils.GetNeighborCells(
								new Vector3Int(x, y),
								_mapBuffer,
								CellTypes.Ground).Count;
							if (neighborGround > 6)
							{
								var newTile = Instantiate(groundCell);
								_map[x, y] = newTile;
							}
						}
					}
				}
			}
		}

		private void GenerateRooms(int roomAmount)
		{
			_rooms = new List<int[,]>();
			for (int i = 0; i < roomAmount; i++)
			{
				_mapBuffer = _map;
				var isValidOrigin = false;
				var placementAttempts = 0;
				var random = new Random(seed + i);
				var roomWidth = random.Next(5, 25);
				var roomHeight = random.Next(5, 25);
				var room = new int[roomWidth, roomHeight];
				for (int x = 0; x < roomWidth; x++)
				{
					for (int y = 0; y < roomHeight; y++)
					{
						room[x, y] = 1;
					}
				}
				var roomOrigin = new Vector3Int(
					random.Next(roomWidth + 1, width - roomWidth - 1), 
					random.Next(roomHeight + 1, height - roomHeight - 1));
				
				var roomCells = GridUtils.GetCellsInShape(roomOrigin, room, _mapBuffer);

				while (!isValidOrigin)
				{
					if (placementAttempts > 100)
					{
						placementAttempts = 0;
						break;
					}
					placementAttempts++;
					isValidOrigin = true;
					
					foreach (var cell in roomCells)
					{
						if (cell.cellType != CellTypes.Wall)
						{
							roomOrigin.x = random.Next(roomWidth + 1, width - roomWidth - 1);
							roomOrigin.y = random.Next(roomWidth + 1, height - roomHeight - 1);
							roomCells = GridUtils.GetCellsInShape(roomOrigin, room, _mapBuffer);
							isValidOrigin = false;
							break;
						}
					}

					if (isValidOrigin)
					{
						placementAttempts = 0;
						_rooms.Add(room);
						foreach (var cell in GridUtils.GetPositionsInShape(roomOrigin, room))
						{
							_map[cell.x, cell.y] = Instantiate(groundCell);
						}
					}
				}
			}
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
			var prng = new Random(seed);
			
			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					if (playerSet)
					    //&& exitSet 
					    //&& keySet 
					    //&& enemyCount >= enemyAmount 
					    //&& itemCount >= itemAmount)
					{
						break;
					}

					if (_mapBuffer[i, j].cellType != CellTypes.Ground) continue;
					var pos = new Vector3Int(i, j, 0);
					//var neighbors = GridUtils.GetCellsInRadius(pos, spawnRadius, _mapBuffer);
					var neighbors = GridUtils.GetCellsInRadius(pos, spawnRadius, _map);
					var playerDist = playerSet?GridUtils.GetCellDistance(_playerSpawnPosition, pos):0;
					var exitDist = exitSet?GridUtils.GetCellDistance(exitPos, pos):0;
					
					if (!playerSet)
					{
						var newTile = Instantiate(playerSpawnCell);
						targetMap.SetTile(pos, newTile);
						_map[i, j] = newTile;
						//newTile.gridPosition = pos;
						_playerSpawnPosition = pos;
						playerSet = true;
					}
					/*
					else if (!exitSet 
					         && playerDist > width/2 
					         && playerDist > height/2)
					{
						var newTile = Instantiate(exitCell);
						targetMap.SetTile(pos, newTile);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
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
						targetMap.SetTile(pos, newTile);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						enemyCount++;
						_enemySpawnPositions.Add(pos);
					}
					else if (itemCount<itemAmount
					         && !neighbors.Find(x => 
						         x.cellType is CellTypes.ItemSpawn))
					{
						var newTile = Instantiate(itemCell);
						targetMap.SetTile(pos, newTile);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
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
						targetMap.SetTile(pos, newTile);
						_mapBuffer[i, j] = newTile;
						//newTile.gridPosition = pos;
						keySet = true;
					}
					*/
				}
			}
		}

		private void ProcessMap() {
			List<List<Vector3Int>> wallRegions = GetRegions(CellTypes.Wall);
			int wallThresholdSize = 50;

			foreach (List<Vector3Int> wallRegion in wallRegions) {
				if (wallRegion.Count < wallThresholdSize) {
					foreach (Vector3Int tile in wallRegion) {
						var newTile = Instantiate(groundCell);
						//targetMap.SetTile(tile, newTile);
						_map[tile.x, tile.y] = newTile;
						//newTile.gridPosition = tile;
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
						//targetMap.SetTile(tile, newTile);
						_map[tile.x, tile.y] = newTile;
						//newTile.gridPosition = tile;
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

		private void ConnectClosestRooms(List<GridRoom> allRooms, bool forceAccessibilityFromMainRoom = false) {

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
		
		private void CreatePassage(GridRoom roomA, GridRoom roomB, Vector3Int tileA, Vector3Int tileB) {
			GridRoom.ConnectRooms (roomA, roomB);
			List<Vector3Int> line = GridUtils.GetLineOfSight(tileA, tileB);
			foreach (Vector3Int c in line) {
				SurroundCell(c,circleRadius, groundCell);
			}
		}

		private void SurroundCell(Vector3Int pos, int radius, CellData cell) {
			for (int x = -radius; x <= radius; x++) {
				for (int y = -radius; y <= radius; y++) {
					if (x*x + y*y <= radius*radius) {
						int drawX = pos.x + x;
						int drawY = pos.y + y;
						if (GridUtils.IsInMapRange(drawX, drawY, _map))
						{
							var newTile = Instantiate(cell);
							//targetMap.SetTile(new Vector3Int(drawX, drawY, 0), newTile);
							_map[drawX, drawY] = newTile;
							//newTile.gridPosition = new Vector3Int(drawX, drawY, 0);
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
						if (GridUtils.IsInMapRange(x, y, _map) && (y == tile.y || x == tile.x)) 
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
		private void DrawTilemap(CellData[,] cellMap)
		{
			targetMap.size = new Vector3Int(cellMap.GetLength(0), cellMap.GetLength(1), 1);
			targetMap.origin = new Vector3Int(0, 0, 0);
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			for (var i = 0; i < cellMap.GetLength(0); i++)
			{
				for (var j = 0; j < cellMap.GetLength(1); j++)
				{
					targetMap.SetTile(new Vector3Int(i, j), _map[i,j]);
				}
			}
		}

		#endregion
		
	}
}