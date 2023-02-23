using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities.Algorithms;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class MapGenerator : MonoBehaviour
	{
		private enum GenerationType { Automata, BSP }
		[SerializeField] private Tilemap targetMap;
		[SerializeField] private int width;
		[SerializeField] private int height;
		[SerializeField] private int seed;
		[SerializeField] private bool randomSeed = true;
		//Move this to a scriptable object and initialize 
		[SerializeField] private CellData wallCell;
		[SerializeField] private CellData groundCell;
		[SerializeField] private CellData waterCell;
		[SerializeField] private GameObject cube;
		[SerializeField] private bool isometric;
		[SerializeField] private GenerationType generationType = GenerationType.Automata;
		
		private Grid targetGrid;
		private int _circleRadius = 1;
		private int _smoothingIterations = 2;
		private int _roomAmount = 10;
		private float _randomFillPercent = 50f;
		private CellData[,] _map;
		private CellData[,] _mapBuffer;
		private BoundsInt _mapBounds;
		private List<List<Vector3Int>> _bspSections;
		private List<List<Vector3Int>> _rooms;
		private List<List<Vector3Int>> _regions;
		private List<GameObject> _cubeMap;

		#region API
		
		[Button]
		public void GenerateSpriteMap()
		{
			targetGrid = targetMap.GetComponentInParent<Grid>();
			targetGrid.cellLayout = isometric? GridLayout.CellLayout.Isometric:GridLayout.CellLayout.Rectangle;
			ClearMap();
			GenerateCellMap();
			DrawTilemap(_map);
		}
		
		public void GenerateCubeMap()
		{
			ClearMap();
			GenerateCellMap();
			DrawCubemap(_map);
		}

		private void CreateLake(List<Vector3Int> region)
		{
			region.ForEach(pos =>
			{
				_map[pos.x, pos.y] = Instantiate(waterCell);
				//_map[pos.x, pos.y].isometric = isometric;
			});
		}

		[Button]
		public void ClearMap()
		{
			targetMap.ClearAllTiles();
			targetMap.CompressBounds();
			targetMap.RefreshAllTiles();
			if(_cubeMap != null)
				_cubeMap.ForEach(DestroyImmediate);
		}

		public CellData[,] GenerateCellMap()
		{
			_map = new CellData[width,height];
			_rooms = new List<List<Vector3Int>>();
			
			if (randomSeed)
			{
				seed = (int)DateTime.Now.Ticks;
			}
			//Move into generate method and find closest room method
			//Modify to avoid backtracking by adding cyclic paths
			
			//Foundrooms is temporary test to detect regions/biomes
			var foundRooms = new List<List<Vector3Int>>();
			if (generationType == GenerationType.Automata)
			{
				RandomFillMap(_map);
				SmoothMap(_map, _smoothingIterations);
				ProcessMap(_map);
				_rooms = GenerateRooms(_roomAmount);
				foundRooms = FindRooms();
			}
			else if (generationType == GenerationType.BSP)
			{
				FillMap();
				var bsp = SplitMap(_map);
				foundRooms.Add(bsp);
				_rooms = foundRooms;
			}
			
			var connectedRooms = new List<List<Vector3Int>>();
			foreach (var room in foundRooms)
			{
				var currentRoom = room;
				var random = new Random();
				var idx = random.Next(foundRooms.Count);
				var nextRoom = foundRooms[idx];
				var nextClosestRoom = foundRooms[idx];
				var roomsConnected = false;

				//FindClosestRoom(currentRoom[0]);
				var distance = GridUtils.GetCellDistance(currentRoom[0], foundRooms[0][0]);
				
				for (int i = 0; i < foundRooms.Count; i++ )
				{
					if (currentRoom != foundRooms[i] 
					    && GridUtils.GetCellDistance(currentRoom[0], foundRooms[i][0]) < distance)
					{
						nextClosestRoom = nextRoom;
						nextRoom = foundRooms[i];
						distance = GridUtils.GetCellDistance(currentRoom[0], foundRooms[i][0]);
					}
				}
				
				ConnectRooms(currentRoom, nextRoom);
				ConnectRooms(currentRoom, nextClosestRoom);
				connectedRooms.Add(currentRoom);
			}
			return _map;
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
		
		public void UpdateCellMap(CellData[,] map)
		{
			_map = map;
		}

		public List<List<Vector3Int>> GetRooms()
		{
			return _rooms;
		}
		
		#endregion

		#region MAPGEN

		private void FillMap()
		{
			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					if (x == 0 || x == width-1 || y == 0 || y == height-1) 
					{
						//Create borders
						var newTile = Instantiate(wallCell);
						//newTile.isometric = isometric;
						_map[x,y] = newTile;
					}
					else
					{
						var newTile = Instantiate(groundCell);
						//newTile.isometric = isometric;
						_map[x, y] = newTile;
					}
				}
			}
		}

		private void RandomFillMap(CellData[,] map)
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
						//newTile.isometric = isometric;
						_map[x,y] = newTile;
					}
					else
					{
						var newTile = Instantiate(prng.Next(0, 100) < _randomFillPercent ? wallCell : groundCell);
						//newTile.isometric = isometric;
						_map[x, y] = newTile;
					}
				}
			}
		}
		
		private void SmoothMap(CellData[,] map, int iterations)
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
								//newTile.isometric = isometric;
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
								//newTile.isometric = isometric;
								_map[x, y] = newTile;
							}
						}
					}
				}
			}
		}

		private List<List<Vector3Int>> GenerateRooms(int amount)
		{
			var rooms = new List<List<Vector3Int>>();
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
				rooms.Add(PlaceRoom(room));
			}
			return rooms;
		}

		private List<Vector3Int> PlaceRoom(int[,] room)
		{
			Random random = new Random(seed);
			var roomFits = false;
			var placementAttempts = 0;
			var roomWidth = room.GetLength(0);
			var roomHeight = room.GetLength(1);
			var roomPositions = new List<Vector3Int>();
			while (!roomFits)
			{
				if (placementAttempts > 100)
					break;
				
				var roomOrigin = new Vector3Int(
					random.Next(roomWidth + 1, _map.GetLength(0) - roomWidth - 1),
					random.Next(roomHeight + 1, _map.GetLength(1) - roomHeight - 1));

				//If room is too close to other rooms, try again
				if (_rooms.Count > 0)
				{
					var closestRoom = FindClosestRoom(roomOrigin);
					if (GridUtils.GetCellDistance(roomOrigin, closestRoom[0]) < 5)
					{
						placementAttempts++;
						continue;
					}
				}
				
				var roomCells = GridUtils.GetCellsInShape(roomOrigin, room, _map);
				foreach (var cell in roomCells)
				{
					roomFits = (cell.cellType == CellTypes.Wall);
					if (!roomFits)
						break;
				}

				if (roomFits)
				{
					placementAttempts = 0;
					roomPositions = MergeRoomToMap(roomOrigin, room);
				}

				placementAttempts++;
				random = new Random(seed + placementAttempts);
			}

			return roomPositions;
		}
		
		private List<Vector3Int> MergeRoomToMap(Vector3Int origin, int[,] room)
		{
			var roomPositions = GridUtils.GetPositionsInShape(origin, room);
			foreach (var cell in roomPositions)
			{
				_map[cell.x, cell.y] = Instantiate(groundCell);
				//_map[cell.x, cell.y].isometric = isometric;
			}
			return roomPositions;
		}

		private void ConnectRooms(List<Vector3Int> startRoom, List<Vector3Int> endRoom)
		{
			var start = GridUtils.GetRandomPosition(startRoom);
			var end = GridUtils.GetRandomPosition(endRoom);
			var path = GridUtils.GetLine(start, end);
			foreach (var cell in path) {
				_map[cell.x, cell.y] = Instantiate(groundCell);
				//_map[cell.x, cell.y].isometric = isometric;
				SurroundCell(cell, 1, groundCell);
			}
		}

		private List<List<Vector3Int>> FindRooms()
		{
			var rooms = new List<List<Vector3Int>> ();
			int[,] mapFlags = new int[width,height];
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					if (mapFlags[x,y] == 0 && _map[x,y].cellType == CellTypes.Ground) 
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

		private List<Vector3Int> FindClosestRoom(Vector3Int roomOrigin)
		{
			var closestRoom = new List<Vector3Int>();
			var distance = 1000f;
			foreach (var room in _rooms)
			{
				if (GridUtils.GetCellDistance(roomOrigin, room[0]) < distance)
				{
					closestRoom = room;
					distance = GridUtils.GetCellDistance(roomOrigin, room[0]);
				}
			}
			return closestRoom;
		}

		//Binary space partitioning, returns a list of rooms (which in turn are lists of cell locations)
		private List<Vector3Int> SplitMap(CellData[,] map)
		{
			//_bspSections = new List<List<Vector3Int>>();
			var bspRooms = new List<Vector3Int>();
			var mapWidth = map.GetLength(0);
			var mapHeight = map.GetLength(1);
			
			var direction = new Random(seed).Next(0, 1);
			var splitPoint = new Random(seed).Next(1, direction == 0 ? mapWidth - 1 : mapHeight - 1);
			
			//TODO: Figure out heuristic for determining boundInt start position
			_mapBounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(width, height, 1));
			
			//TODO: Heuristic for minimum room size and pre-built room
			var boundRooms = ProcGenUtils.BinarySpacePartitioning(_mapBounds, 5, 5);
			bspRooms = CreateRoomsFromBounds(boundRooms);
			
			return bspRooms;
		}

		private List<Vector3Int> CreateRoomsFromBounds(List<BoundsInt> roomsList)
		{
			var offset = 1;
			var floor = new List<Vector3Int>();
			foreach (var room in roomsList)
			{
				for (int col = offset; col < room.size.x - offset; col++)
				{
					for (int row = offset; row < room.size.y - offset; row++)
					{
						var position = room.min + new Vector3Int(col, row);
						floor.Add(position);
					}
				}
			}
			return floor;
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

		private void ProcessMap(CellData[,] map) {
			List<List<Vector3Int>> wallRegions = GetRegions(CellTypes.Wall);
			int wallThresholdSize = 50;

			foreach (List<Vector3Int> wallRegion in wallRegions) {
				if (wallRegion.Count < wallThresholdSize) 
				{
					foreach (Vector3Int tile in wallRegion) 
					{
						var newTile = Instantiate(groundCell);
						//newTile.isometric = isometric;
						//targetMap.SetTile(tile, newTile);
						_map[tile.x, tile.y] = newTile;
						//newTile.gridPosition = tile;
					}
				}
			}

			List<List<Vector3Int>> roomRegions = GetRegions(CellTypes.Ground);
			int roomThresholdSize = 50;
			List<GridRoom> survivingRooms = new List<GridRoom> ();
		
			foreach (List<Vector3Int> roomRegion in roomRegions) {
				if (roomRegion.Count < roomThresholdSize) {
					foreach (Vector3Int tile in roomRegion) {
						var newTile = Instantiate(wallCell);
						//newTile.isometric = isometric;
						_map[tile.x, tile.y] = newTile;
						CreateLake(roomRegion);
					}
				}
				else {
					survivingRooms.Add(new GridRoom(roomRegion, _map));
				}
			}
			if (survivingRooms.Count > 0)
			{
				survivingRooms.Sort();
				survivingRooms[0].isMainRoom = true;
				survivingRooms[0].isAccessibleFromMainRoom = true;
				ConnectClosestRooms(survivingRooms);
			}
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
				SurroundCell(c,_circleRadius, groundCell);
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
							//newTile.isometric = isometric;
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
					//_map[i,j].gridPosition = new Vector3Int(i, j);
					//_map[i, j].isometric = isometric;
					targetMap.SetTile(new Vector3Int(i, j), _map[i,j]);
				}
			}
		}

		//For testing 3D isometric mapgen
		private void DrawCubemap(CellData[,] map)
		{
			_cubeMap = new List<GameObject>();
			for (var i = 0; i < map.GetLength(0); i++)
			{
				for (var j = 0; j < map.GetLength(1); j++)
				{
					var newCube = Instantiate(cube);
					newCube.transform.position = new Vector3(i, 0, j);
					_cubeMap.Add(newCube);
				}
			}
		}

		#endregion
		
	}
}