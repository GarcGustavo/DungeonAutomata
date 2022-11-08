using System;
using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class TileMapGenerator : MonoBehaviour
	{
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
		[SerializeField] private CellData waterCell;
		private Vector3Int _playerSpawnPosition;
		private List<Vector3Int> _enemySpawnPositions;
		private List<Vector3Int> _itemSpawnPositions;
		
		[SerializeField] private int roomAmount = 10;

		//Cellular automata maps
		private CellData[,] _map;
		private CellData[,] _mapBuffer;
		private List<List<Vector3Int>> _rooms;

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
			//Modify to avoid backtracking by adding cyclic paths
			ProcessMap();
			GenerateRooms(roomAmount);
			var foundRooms = FindRooms();
			var currentRoom = _rooms[0];
			//foreach (var room in _rooms)
			foreach (var room in _rooms)
			{
				if(currentRoom != room)
					ConnectRooms(currentRoom, room);
				currentRoom = room;
			}
			DrawTilemap(_map);
		}

		private void CreateLake(List<Vector3Int> region)
		{
			region.ForEach(pos => _map[pos.x, pos.y] = Instantiate(waterCell));
		}

		[Button]
		public void ClearMap()
		{
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			
			highlightTilemap.ClearAllTiles();
			highlightTilemap.CompressBounds();
			highlightTilemap.RefreshAllTiles();
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

		public List<List<Vector3Int>> GetRooms()
		{
			return _rooms;
		}
		
		#endregion

		#region AUTOMATAGEN

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

		private void GenerateRooms(int amount)
		{
			_rooms = new List<List<Vector3Int>>();
			var random = new Random(seed);
			var roomList = new List<int[,]>();
			//Generate an initial batch of rooms, might expand later for determining biomes
			for (int i = 0; i < amount; i++)
			{
				var room = CreateRoom(random.Next(3, width/6), random.Next(3, height/6));
				roomList.Add(room);
			}
			foreach (var room in roomList)
			{
				//TODO: finish BSP implementation
				//var splitMap = SplitMap(_map, roomList);
				PlaceRoom(room, _map);
			}
		}

		private void PlaceRoom(int[,] room, CellData[,] map)
		{
			Random random = new Random(seed);
			var roomFits = false;
			var placementAttempts = 0;
			var roomWidth = room.GetLength(0);
			var roomHeight = room.GetLength(1);
			while (!roomFits)
			{
				if (placementAttempts > 100)
					break;
				var roomOrigin = new Vector3Int(
					random.Next(roomWidth + 1, map.GetLength(0) - roomWidth - 1),
					random.Next(roomHeight + 1, map.GetLength(1) - roomHeight - 1));

				var roomCells = GridUtils.GetCellsInShape(roomOrigin, room, map);

				foreach (var cell in roomCells)
				{
					roomFits = (cell.cellType == CellTypes.Wall);
					if (!roomFits)
						break;
				}

				if (roomFits)
				{
					placementAttempts = 0;
					MergeRoomToMap(roomOrigin, room);
				}

				placementAttempts++;
				random = new Random(seed + placementAttempts);
			}
		}

		private void ConnectRooms(List<Vector3Int> startRoom, List<Vector3Int> endRoom)
		{
			var start = GridUtils.GetRandomPosition(startRoom);
			var end = GridUtils.GetRandomPosition(endRoom);
			var path = GridUtils.GetLine(start, end);
			foreach (var cell in path) {
				_map[cell.x, cell.y] = Instantiate(groundCell);
				SurroundCell(cell, 1, groundCell);
			}
		}

		private List<List<Vector3Int>> FindRooms()
		{
			var rooms = new List<List<Vector3Int>> ();
			int[,] mapFlags = new int[width,height];
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					if (mapFlags[x,y] == 0 && _map[x,y].cellType == CellTypes.Wall) 
					{
						List<Vector3Int> newRegion = GetRegionTiles(x,y);
						rooms.Add(newRegion);

						foreach (Vector3Int tile in newRegion) 
						{
							mapFlags[tile.x, tile.y] = 1;
						}
					}
				}
			}
			return rooms;
		}

		//Binary space partitioning
		private CellData[,] SplitMap(CellData[,] map, List<int[,]> roomList)
		{
			var mapWidth = map.GetLength(0);
			var mapHeight = map.GetLength(1);
			var newMap = new CellData[mapWidth, mapHeight];
			var left = new CellData[mapWidth, mapHeight];
			var right = new CellData[mapWidth, mapHeight];
			var direction = new Random(seed).Next(0, 2);
			var splitPoint = new Random(seed).Next(1, direction == 0 ? mapWidth - 1 : mapHeight - 1);
			var timesToSplit = new Random(seed).Next(1, 3);
			
			//Split map in half
			for (int i = 0; i < timesToSplit; i++)
			{
				if (direction == 0)
				{
					for (int x = 0; x < mapWidth; x++)
					{
						for (int y = 0; y < mapHeight; y++)
						{
							if (x < splitPoint)
							{
								newMap[x, y] = map[x, y];
								left[x, y] = map[x, y];
							}
							else
							{
								newMap[x, y] = Instantiate(wallCell);
							}
						}
					}
				}
				else
				{
					for (int x = 0; x < mapWidth; x++)
					{
						for (int y = 0; y < mapHeight; y++)
						{
							if (y < splitPoint)
							{
								newMap[x, y] = map[x, y];
								right[x, y] = map[x, y];
							}
							else
							{
								newMap[x, y] = Instantiate(wallCell);
							}
						}
					}
				}
				direction = new Random(seed).Next(0, 2);
				splitPoint = new Random(seed).Next(1, direction == 0 ? mapWidth - 1 : mapHeight - 1);
			}
			return newMap;
		}
		
		private int[,] CreateRoom(int roomWidth, int roomHeight)
		{
			var room = new int[roomWidth, roomHeight];
			for (int x = 0; x < roomWidth; x++)
			{
				for (int y = 0; y < roomHeight; y++)
				{
					room[x, y] = 1;
				}
			}
			return room;
		}
		
		private void MergeRoomToMap(Vector3Int origin, int[,] room)
		{
			var roomPositions = GridUtils.GetPositionsInShape(origin, room);
			_rooms.Add(roomPositions);
			foreach (var cell in roomPositions)
			{
				_map[cell.x, cell.y] = Instantiate(groundCell);
			}
		}

		private void ProcessMap() {
			List<List<Vector3Int>> wallRegions = GetRegions(CellTypes.Wall);
			int wallThresholdSize = 50;

			foreach (List<Vector3Int> wallRegion in wallRegions) {
				if (wallRegion.Count < wallThresholdSize) 
				{
					foreach (Vector3Int tile in wallRegion) 
					{
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
						_map[tile.x, tile.y] = newTile;
						CreateLake(roomRegion);
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
			List<Vector3Int> line = GridUtils.GetLine(tileA, tileB);
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

			/*
			foreach (var room in _rooms)
			{
				foreach (var cell in room)
				{
					targetMap.SetColor(cell, Color.red);
				}
			}
			*/
		}

		#endregion
		
	}
}