using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace DungeonAutomata._Project.Scripts._Managers
{
	[RequireComponent(typeof(TileMapGenerator))]
	public class MapManager : MonoBehaviour
	{
		[SerializeField] private Grid grid;
		[SerializeField] private Tilemap[] roomMaps;
		[SerializeField] private Tilemap tileMap;
		[SerializeField] private Tilemap highLightMap;
		[SerializeField] private CellData highLightTile;
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject[] enemyPrefabs;
		[SerializeField] private GameObject[] itemPrefabs;
		[SerializeField] private ItemData[] itemDataObjects;
	
		public TileMapGenerator tileMapGenerator;
		private GameManager _gameManager;
		private EventManager _eventManager;
		private UIManager _uiManager;
		private PlayerUnit _player;
		private Vector3Int _playerSpawnPoint;
		private List<EnemyUnit> _enemies;
		private List<IUnit> _unitsToMove;
		private List<ItemUnit> _items;
		private List<Vector3Int> _enemySpawnPoints;
		private List<Vector3Int> _itemSpawnPoints;
		private List<Vector3Int> _visibleCells = new List<Vector3Int>();
	
		//Main reference to get occupant info through
		private CellData[,] _gridMap;
		public static MapManager Instance { get; private set; }

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
			_gameManager = GameManager.Instance;
			_uiManager = UIManager.Instance;
			tileMapGenerator = GetComponent<TileMapGenerator>();
			_items = new List<ItemUnit>();
			_enemies = new List<EnemyUnit>();
			_unitsToMove = new List<IUnit>();
			//_eventManager.OnStartGame += SpawnPlayer;
			_gridMap = new CellData[tileMap.cellBounds.size.x, tileMap.cellBounds.size.y];
			_eventManager.OnUnitAction += CheckUnitsToMove;
			_eventManager.OnAttack += DamageCells;
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

		private void CheckUnitsToMove(IUnit unit)
		{
			if (_unitsToMove.Contains(unit))
			{
				_unitsToMove.Remove(unit);
				if (_unitsToMove.Count == 0)
				{
					_eventManager.InvokeEnemyTurnEnd();
					_unitsToMove.AddRange(_enemies);
				}
			}
		}

		//-------------------------------------------------------------------
		// Map functions
		//-------------------------------------------------------------------

		public void InitializeMap()
		{
			tileMapGenerator.GenerateMap();
			tileMap = tileMapGenerator.GetTileMap();
			_gridMap = tileMapGenerator.GetGridMap();
			highLightMap = tileMapGenerator.GetHighlightMap();
			highLightMap.ClearAllTiles();
			highLightMap.RefreshAllTiles();
			SpawnPlayer();
			SpawnEnemies();
			SpawnItems();
		}

		public void ResetMap()
		{
			tileMap.ClearAllTiles();
			highLightMap.ClearAllTiles();
			_enemies.Clear();
			_items.Clear();
			if (_player) Destroy(_player.gameObject);
		}

		public void HighLightCell(Vector3 cell)
		{
			if (tileMap.HasTile(grid.WorldToCell(cell)))
			{
				var cellData = _gridMap[grid.WorldToCell(cell).x, grid.WorldToCell(cell).y];
				if (cellData == null) return;
				highLightMap.ClearAllTiles();
				highLightMap.SetTile(grid.WorldToCell(cell), highLightTile);
				_visibleCells = GridUtils.GetLineOfSight(_player.CurrentTile, grid.WorldToCell(cell));
				if (cellData.Occupant != null)
				{
					_uiManager.SetHoverText("Cell["+ cellData.gridPosition +"] " + cellData.Occupant.UnitName);
				}
				else
				{
					_uiManager.SetHoverText("Cell["+ cellData.gridPosition +"]");
				}
			}
			foreach (var visibleCell in _visibleCells)
			{
				highLightMap.SetTile(visibleCell, highLightTile);
			}
		}

		public List<CellData> GetNeighborTiles(Vector3Int position)
		{
			var neighbors = new List<CellData>();
			var up = position + Vector3Int.up;
			var down = position + Vector3Int.down;
			var left = position + Vector3Int.left;
			var right = position + Vector3Int.right;
		
			if (tileMap.HasTile(up)) neighbors.Add(_gridMap[up.x, up.y]);
			if (tileMap.HasTile(down)) neighbors.Add(_gridMap[down.x, down.y]);
			if (tileMap.HasTile(left)) neighbors.Add(_gridMap[left.x, left.y]);
			if (tileMap.HasTile(right)) neighbors.Add(_gridMap[right.x, right.y]);
			return neighbors;
		}

		//Convert unity tilemap to 2d array for easier manipulation
		private int[,] ExtractTileMapData(Tilemap tileMap)
		{
			var roomTilemap = roomMaps[Random.Range(0, roomMaps.Length)];
			var roomData = new int[roomTilemap.size.x, roomTilemap.size.y];
			for (var i = 0; i < roomTilemap.size.x; i++)
			for (var j = 0; j < roomTilemap.size.y; j++)
				if (roomTilemap.GetTile(new Vector3Int(i, j, 0)) != null)
					//var tile = CreateInstance<Tile>();
					//tile.sprite = obstacleSprites[Random.Range(0, obstacleSprites.Length)];
					//tileMap.SetTile(new Vector3Int(i, j, 0), tile);
					roomData[i, j] = 1;
			return roomData;
		}

		//-------------------------------------------------------------------
		// Spawn functions
		//-------------------------------------------------------------------
		private void SpawnPlayer()
		{
			_playerSpawnPoint = tileMapGenerator.GetPlayerSpawnPosition();
			Debug.Log("Spawning player at: " + _playerSpawnPoint);
			var prefab = Instantiate(playerPrefab, _playerSpawnPoint, Quaternion.identity);
			_player = prefab.GetComponent<PlayerUnit>();
			_player.InitializeUnit();
			_player.CurrentTile = _gridMap[_playerSpawnPoint.x, _playerSpawnPoint.y].gridPosition;
			_gameManager.ResetCamera(_player.transform);
			var tile = tileMap.GetTile(_player.CurrentTile) as CellData;
			tile.Occupant = _player;
		}

		private void SpawnEnemies()
		{
			_enemySpawnPoints = tileMapGenerator.GetEnemySpawnPositions();
			//Spawn enemies at available tiles
			foreach (var spawnPoint in _enemySpawnPoints)
			{
				Debug.Log("Spawning enemy at: " + spawnPoint);
				var gridPos = spawnPoint;
				var prefab = Instantiate(enemyPrefabs[0], gridPos, Quaternion.identity);
				var enemy = prefab.GetComponent<EnemyUnit>();
				enemy.CurrentTile = gridPos;
				enemy.InitializeUnit();
				_enemies.Add(enemy);
				_unitsToMove.Add(enemy);
				var tile = tileMap.GetTile(gridPos) as CellData;
				tile.Occupant = enemy;
			}
		}

		private void SpawnItems()
		{
			//Spawn items at available tiles
			_itemSpawnPoints = tileMapGenerator.GetItemSpawnPositions();
			foreach (var spawnPoint in _itemSpawnPoints)
			{
				Debug.Log("Spawning item at: " + spawnPoint);
				var gridPos = spawnPoint;
				var prefab = Instantiate(itemPrefabs[0], gridPos, Quaternion.identity);
				var item = prefab.GetComponent<ItemUnit>();
				//item.InitializeItem(itemDataObjects[0]);
				var tile = tileMap.GetTile(gridPos) as CellData;
				tile.Occupant = item;
				item.CurrentTile = gridPos;
				_items.Add(item);
			}
		}

		//-------------------------------------------------------------------
		// Public getters
		//-------------------------------------------------------------------
		public CellData[,] GetGridMap()
		{
			return _gridMap;
		}

		public Tilemap GetTileMap()
		{
			return tileMap;
		}

		public PlayerUnit GetPlayer()
		{
			return _player;
		}

		public Vector3Int GetPlayerPosition()
		{
			return _player.CurrentTile;
		}

		public int GetCellDistance(Vector3Int src, Vector3Int target)
		{
			return tileMapGenerator.GetCellDistance(src, target);
		}

		public List<CellData> GetCellsInRadius(Vector3Int pos, int radius)
		{
			return tileMapGenerator.GetCellsInRadius(pos, radius);
		}
	}
}