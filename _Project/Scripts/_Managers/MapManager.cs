using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public enum MapType
	{
		TopDown,
		FirstPerson
	}
	[RequireComponent(typeof(MapGenerator))]
	public class MapManager : MonoBehaviour
	{
		public static MapManager Instance { get; private set; }
		[SerializeField] private MapType mapType;
		[SerializeField] private Grid grid;
		[SerializeField] private Tilemap highLightMap;
		[SerializeField] private CellData highLightTile;
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject enemyPrefab;
		[SerializeField] private GameObject itemPrefab;
		//Move this logic to a separate class that handles setting SO data
		[SerializeField] private EnemyData[] enemyData;
		[SerializeField] private ItemData itemData;
		[SerializeField] private ItemData foodData;
		[SerializeField] private ItemData healthData;
	
		[FormerlySerializedAs("tileMapGenerator")] public MapGenerator mapGenerator;
		private TopDownManager _turnManager;
		private EventManager _eventManager;
		private UIManager _uiManager;
		private PlayerUnit _player;
		private Vector3Int _playerSpawnPoint;
		private List<Vector3Int> _startingRoom;
		private List<EnemyUnit> _enemies;
		//private List<IUnit> _unitsToMove;
		private int _unitsToMove = 0;
		private List<ItemUnit> _items;
		private List<Vector3Int> _enemySpawnPoints;
		private List<Vector3Int> _itemSpawnPoints;
		private List<Vector3Int> _visibleCells;
		private List<Vector3Int> _exitRoom;
	
		//Main reference point to get occupant info from
		private CellData[,] _gridMap;
		private Tilemap _tileMap;

		//Dijkstra map objectives
		private int[,] _playerMap;
		private int[,] _enemyMap;
		private int[,] _itemMap;
		private int[,] _foodMap;
		private int[,] _waterMap;
		private int[,] _preyMap;
		private int[,] _predatorMap;

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject);
		}

		private void Start()
		{
			_eventManager = EventManager.Instance;
			_turnManager = TopDownManager.Instance;
			_uiManager = UIManager.Instance;
			mapGenerator = GetComponent<MapGenerator>();
			_items = new List<ItemUnit>();
			_enemies = new List<EnemyUnit>();
			//_unitsToMove = new List<IUnit>();
			_visibleCells = new List<Vector3Int>();
			/*
			var cellBounds = _tileMap.cellBounds;
			_gridMap = new CellData[cellBounds.size.x, cellBounds.size.y];
			_playerMap = new int[cellBounds.size.x, cellBounds.size.y];
			_enemyMap = new int[cellBounds.size.x, cellBounds.size.y];
			_itemMap = new int[cellBounds.size.x, cellBounds.size.y];
			_foodMap = new int[cellBounds.size.x, cellBounds.size.y];
			_waterMap = new int[cellBounds.size.x, cellBounds.size.y];
			_preyMap = new int[cellBounds.size.x, cellBounds.size.y];
			_predatorMap = new int[cellBounds.size.x, cellBounds.size.y];
			*/
			_eventManager.OnUnitAction += CheckUnitsToMove;
			_eventManager.OnAttack += DamageCells;
			_eventManager.OnUnitDeath += RemoveUnitToMove;
			_eventManager.OnPlayerAction += UpdatePlayerMap;
			_eventManager.OnMapUpdate += UpdateCellMap;
			_eventManager.OnCellUpdate += UpdateCell;
			//_eventManager.OnTurnEnd += UpdatePlayerMap;
		}

		#region MAPFUNCTIONS

		public void InitializeMap()
		{
			mapGenerator.GenerateSpriteMap();
			_tileMap = mapGenerator.GetTileMap();
			_gridMap = mapGenerator.GetCellMap();
			//highLightMap.ClearAllTiles();
			//highLightMap.RefreshAllTiles();
			var cellBounds = _tileMap.cellBounds;
			var x = cellBounds.size.x;
			var y = cellBounds.size.y;
			
			if (mapType == MapType.FirstPerson)
			{
				y = cellBounds.size.z;
			}
			
			_playerMap = new int[x, y];
			_enemyMap = new int[x, y];
			_itemMap = new int[x, y];
			_foodMap = new int[x, y];
			_waterMap = new int[x, y];
			_preyMap = new int[x, y];
			_predatorMap = new int[x, y];
		}

		[Button]
		public void PopulateGridMap()
		{
			SetExit();
			if(_turnManager.GetPlayer() == null)
				SpawnPlayer();
			SpawnEnemies();
			SpawnItems();
		}

		public void ResetMap()
		{
			_tileMap.ClearAllTiles();
			highLightMap.ClearAllTiles();
			_enemies.Clear();
			_items.Clear();
			if (_player) Destroy(_player.gameObject);
		}

		//For debugging purposes
		[Button]
		private void ClearDijkstraMap()
		{
			foreach (var cell in _gridMap)
			{
				_tileMap.SetColor(cell.gridPosition, Color.white);
			}
		}
		[Button]
		private void VisualizePlayerDijsktraMap()
		{
			ClearDijkstraMap();
			var dijkstraMap = GridUtils.GetDijkstraMap(_player.CurrentPos, _gridMap);
			for (int i = 0; i < dijkstraMap.GetLength(0); i++)
			{
				for (int j = 0; j < dijkstraMap.GetLength(1); j++)
				{
					if (_gridMap[i, j] != null)
					{
						var cell = _gridMap[i, j];
						if (dijkstraMap[i,j] == -1)
							_tileMap.SetColor(cell.gridPosition, Color.white);
						else
							_tileMap.SetColor(cell.gridPosition, 
								Color.Lerp(Color.blue, Color.red, 25f/dijkstraMap[i, j]));
					}
				}
			}
		}
		[Button]
		private void VisualizeEnemyDijsktraMap()
		{
			ClearDijkstraMap();
			var enemyPositions = new List<Vector3Int>();
			foreach (var enemy in _enemies)
			{
				enemyPositions.Add(enemy.CurrentPos);
			}
			var dijkstraMap = GridUtils.GetDijkstraMap(enemyPositions, _gridMap);
			for (int i = 0; i < dijkstraMap.GetLength(0); i++)
			{
				for (int j = 0; j < dijkstraMap.GetLength(1); j++)
				{
					if (_gridMap[i, j] != null)
					{
						var cell = _gridMap[i, j];
						
						if (dijkstraMap[i,j] == -1)
							_tileMap.SetColor(cell.gridPosition, Color.white);
						else
							_tileMap.SetColor(cell.gridPosition, 
								Color.Lerp(Color.blue, Color.red, 5f/dijkstraMap[i, j]));
					}
				}
			}
		}

		//TODO: Overload for cell lists/shapes
		public void HighLightCell(Vector3Int cell)
		{
			if (_tileMap == null) return;
			var cellPos = grid.WorldToCell(cell);
			//var cellCartPos = IsIsometric() ? GridUtils.GetCartesianPos(cellPos) : cellPos;
			if (_tileMap.HasTile(cellPos) && _gridMap[cellPos.x, cellPos.y])
			{
				var cellData = _gridMap[cellPos.x, cellPos.y];
				if (cellData == null) return;
				highLightMap.ClearAllTiles();
				highLightMap.SetTile(cellPos, highLightTile);
				//highLightMap.SetColor(cellPos, Color.green);
				//Debug.Log("Highlighting cart cell: " + cellCartPos + ", iso: " + cellPos);

				_visibleCells = GridUtils.GetLine(_player.CurrentPos, cellPos);
				if (cellData.Occupant != null)
				{
					_uiManager.SetUnitInfo(cellData.Occupant);
					_uiManager.SetHoverText("Cell["+ cellData.gridPosition +"]");
				}
				else
				{
					_uiManager.SetUnitInfo(null);
					_uiManager.SetHoverText("Cell["+ cellData.gridPosition +"]");
				}
			}
			//foreach (var visibleCell in _visibleCells)
			//{
			//	highLightMap.SetTile(visibleCell, highLightTile);
			//}
		}
		public void HighLightCell(List<Vector3Int> cells)
		{
			foreach (var cell in cells)
			{
				HighLightCell(cell);
			}
		}

		//Move to combat manager later
		private void DamageCells(IUnit sourceUnit, List<Vector3Int> cells)
		{
			foreach (var cell in cells)
			{
				var unit = _gridMap[cell.x, cell.y].Occupant;
				if (unit != null && unit != sourceUnit)
				{
					unit.Damage(1);
				}
			}
		}

		private void RemoveUnitToMove(IUnit unit)
		{
			//_gridMap[unit.CurrentTile.x, unit.CurrentTile.y].Occupant = null;
			//_gridMap[unit.CurrentTile.x, unit.CurrentTile.y].isWalkable = true;
			Debug.Log("Removing unit");
			Debug.Log("Units to move: " + _unitsToMove);
			_unitsToMove--;
			if (_unitsToMove <= 0)
			{
				foreach (var enemy in _enemies)
				{
					if (enemy == null)
						continue;
					if (!enemy.isActiveAndEnabled)
						continue;
					if (enemy.CanMove)
						_unitsToMove++;
				}
				_eventManager.InvokeTurnEnd();
			}
			//if(_unitsToMove.Contains(unit))
			//	_unitsToMove.Remove(unit);
		}

		private void CheckUnitsToMove(IUnit unit)
		{
			//if (_unitsToMove.Count <= 0)
			Debug.Log("Checking units to move");
			Debug.Log("Units to move: " + _unitsToMove);
			//else if (_unitsToMove.Contains(unit))
			if (_unitsToMove > 0)
			{
				_unitsToMove--;
			}
			if (_unitsToMove <= 0)
			{
				foreach (var enemy in _enemies)
				{
					if (enemy == null)
						continue;
					if (!enemy.isActiveAndEnabled)
						continue;
					if (enemy.CanMove)
						_unitsToMove++;
				}
				_eventManager.InvokeTurnEnd();
			}
		}

		#endregion

		#region SPAWNERS

		private void SetExit()
		{
			var rooms = mapGenerator.GetRooms();
			_exitRoom = rooms[Random.Range(0, rooms.Count)];
			var exitCell = _exitRoom[Random.Range(0, _exitRoom.Count)];
			//var cellPos = IsIsometric() ? GridUtils.GetIsometricPos(exitCell) : exitCell;
			while (_gridMap[exitCell.x, exitCell.y].Occupant != null && !_gridMap[exitCell.x, exitCell.y].isWalkable)
			{ 
				exitCell = _exitRoom[Random.Range(0, _exitRoom.Count)];
			}
			_gridMap[exitCell.x, exitCell.y].cellType = CellTypes.Exit;
			_tileMap.SetColor(exitCell, Color.magenta);
			_tileMap.SetTile(exitCell, _gridMap[exitCell.x, exitCell.y]);
		}
		
		private void SpawnPlayer()
		{
			var rooms = mapGenerator.GetRooms();
			//_playerSpawnPoint = tileMapGenerator.GetPlayerSpawnPosition();
			
			//TODO: Add value map logic to spawn player in the best position
			foreach (var room in rooms)
			{
				if (_playerSpawnPoint == Vector3Int.zero)
				{
					_playerSpawnPoint = GridUtils.GetRandomPosition(room);
					_startingRoom = room;
				}
				else
					break;
			}

			var prefab = Instantiate(playerPrefab, _tileMap.CellToWorld(_playerSpawnPoint), Quaternion.identity);
			_player = prefab.GetComponent<PlayerUnit>();
			_player.InitializeUnit();
			_player.CurrentPos = _playerSpawnPoint;
			_turnManager.SetPlayer(_player);
			_turnManager.ResetCamera(_player.transform);
			_gridMap[_playerSpawnPoint.x, _playerSpawnPoint.y].Occupant = _player;
			_gridMap[_playerSpawnPoint.x, _playerSpawnPoint.y].isWalkable = false;
			//var tile = tileMap.GetTile(_player.CurrentTile) as CellData;
			//tile.Occupant = _player;
			UpdatePlayerMap();
		}

		private void SpawnEnemies()
		{
			_enemySpawnPoints = new List<Vector3Int>();
			var rooms = mapGenerator.GetRooms();
			foreach (var room in rooms)
			{
				if (room != _startingRoom)
				{
					//for( int i = 0; i < 3; i++)
					//{
					//	var pos = GridUtils.GetRandomPosition(room);
					//	_enemySpawnPoints.Add(pos);
					//}
					var pos = GridUtils.GetRandomPosition(room);
					_enemySpawnPoints.Add(pos);
				}
			}
			//Spawn enemies at available tiles
			foreach (var spawnPoint in _enemySpawnPoints)
			{
				var data = enemyData[Random.Range(0, enemyData.Length)];
				//var tilePos = IsIsometric() ? GridUtils.GetIsometricPos(gridPos) : gridPos;
				if (_gridMap[spawnPoint.x, spawnPoint.y].Occupant == null)
				{
					var prefab = Instantiate(enemyPrefab, _tileMap.CellToWorld(spawnPoint), Quaternion.identity);
					var enemy = prefab.GetComponent<EnemyUnit>();
					enemy.InitializeUnit(data);
					enemy.CurrentPos = spawnPoint;
					_enemies.Add(enemy);
					//_unitsToMove.Add(enemy);
					_unitsToMove++;
					_gridMap[spawnPoint.x, spawnPoint.y].Occupant = enemy;
					_gridMap[spawnPoint.x, spawnPoint.y].isWalkable = false;
				}
			}
			UpdateEnemyMap(_enemySpawnPoints);
		}

		private void SpawnItems()
		{
			//Spawn items at available tiles
			_itemSpawnPoints = new List<Vector3Int>();
			var rooms = mapGenerator.GetRooms();
			foreach (var room in rooms)
			{
				if (room != _startingRoom)
				{
					//TODO: Add value map logic to spawn items in the best position
				}
				//for (int i = 0; i < 3; i++)
				//{
				//	var spawn = GridUtils.GetRandomPosition(room);
				//	if (_gridMap[spawn.x, spawn.y].Occupant == null)
				//	{
				//		_itemSpawnPoints.Add(spawn);
				//	}
				//}
				var spawn = GridUtils.GetRandomPosition(room);
				if (_gridMap[spawn.x, spawn.y].Occupant == null)
				{
					_itemSpawnPoints.Add(spawn);
				}
			}
			foreach (var spawnPoint in _itemSpawnPoints)
			{
				//Debug.Log("Spawning item at: " + spawnPoint);
				var prefab = Instantiate(itemPrefab, _tileMap.CellToWorld(spawnPoint), Quaternion.identity);
				var item = prefab.GetComponent<ItemUnit>();
				
				//Eventually move logic to a level director or something
				item.InitializeItem(Random.Range(0, 100) < 50 ? foodData : healthData, item);

				item.CurrentPos = spawnPoint;
				_gridMap[spawnPoint.x, spawnPoint.y].Occupant = item;
				_gridMap[spawnPoint.x, spawnPoint.y].isWalkable = false;
				//var tile = tileMap.GetTile(gridPos) as CellData;
				//tile.Occupant = item;
				_items.Add(item);
			}
		}
		
		#endregion

		#region GETS

		public bool IsIsometric()
		{
			return grid.cellLayout == GridLayout.CellLayout.Isometric;
		}
		
		public CellData[,] GetCellMap()
		{
			return _gridMap;
		}

		public Tilemap GetTileMap()
		{
			return _tileMap;
		}

		public PlayerUnit GetPlayer()
		{
			return _player;
		}

		public Vector3Int GetPlayerPosition()
		{
			return _player.CurrentPos;
		}
		
		public void UpdateCellMap(CellData[,] map)
		{
			_gridMap = map;
			//mapGenerator.UpdateCellMap(_gridMap);
		}
		
		public void UpdateCell(CellData cell)
		{
			var gridPos = cell.gridPosition;
			_gridMap[gridPos.x, gridPos.y] = cell;
			//mapGenerator.UpdateCell(cell);
		}

		#endregion

		#region VALUEMAPS
		//TODO: Tie map updates to appropriate events
		private void UpdatePlayerMap()
		{
			Debug.Log("Updating player map");
			_playerMap = GridUtils.GetDijkstraMap(_player.CurrentPos, _gridMap);
		}
		
		private void UpdateEnemyMap(List<Vector3Int> enemyPositions)
		{
			_enemyMap = GridUtils.GetDijkstraMap(enemyPositions, _gridMap);
		}
		
		private void UpdateItemMap(List<Vector3Int> itemPositions)
		{
			_itemMap = GridUtils.GetDijkstraMap(itemPositions, _gridMap);
		}
		
		private void UpdatePredatorMap(List<Vector3Int> predatorPositions)
		{
			_predatorMap = GridUtils.GetDijkstraMap(predatorPositions, _gridMap);
		}
		
		private void UpdatePreyMap(List<Vector3Int> preyPositions)
		{
			_preyMap = GridUtils.GetDijkstraMap(preyPositions, _gridMap);
		}
		
		private void UpdateFoodMap(List<Vector3Int> foodPositions)
		{
			_foodMap = GridUtils.GetDijkstraMap(foodPositions, _gridMap);
		}
		
		private void UpdateWaterMap(List<Vector3Int> waterPositions)
		{
			_waterMap = GridUtils.GetDijkstraMap(waterPositions, _gridMap);
		}

		public int[,] GetPlayerMap()
		{
			return _playerMap;
		}
		
		public int[,] GetEnemyMap()
		{
			return _enemyMap;
		}
		
		public int[,] GetItemMap()
		{
			return _itemMap;
		}
		
		public int[,] GetPredatorMap()
		{
			return _predatorMap;
		}
		
		public int[,] GetPreyMap()
		{
			return _preyMap;
		}
		
		public int[,] GetFoodMap()
		{
			return _foodMap;
		}
		
		public int[,] GetWaterMap()
		{
			return _waterMap;
		}

		#endregion
		
		
	}
}