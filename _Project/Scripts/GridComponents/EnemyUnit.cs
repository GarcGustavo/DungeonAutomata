using System;
using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(GridController2D))]
	public class EnemyUnit : MonoBehaviour, IUnit, ICombatUnit
	{
		private MapManager _mapManager;
		private EventManager _eventManager;
		public string UnitName { get; set; }
		public int MaxHP { get; set; }
		public int CurrentHP { get; set; }
		public int Hunger { get; set; }
		public int Thirst { get; set; }
		public int AggroDistance { get; set; }
		
		public Vector3Int CurrentTile { get; set; }
		private List<CellData> _visibleCells;
		public Vector3Int CurrentTarget { get; set; }
		private Vector3Int playerPos;
		private CellTypes targetType;
		private Tilemap _tilemap;
		public bool CanMove { get; set; }
		private GridController2D _controller;
		[SerializeField] private int _viewDistance = 5;
		[SerializeField] private EnemyData _enemyData;

		private void Awake()
		{
			_mapManager = MapManager.Instance;
			_eventManager = EventManager.Instance;
			_controller = GetComponent<GridController2D>();
			_eventManager.OnEnemyTurnStart += UpdateState;
			var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			spriteRenderer.sprite = _enemyData.sprite;
			UnitName = _enemyData.enemyName;
			AggroDistance = _enemyData.aggroDistance;
		}

		public void InitializeUnit()
		{
			_tilemap = _mapManager.tileMapGenerator.GetTileMap();
			MaxHP = _enemyData.Health;
			CurrentHP = MaxHP;
			CanMove = true;
			//_controller.InitializeGrid();
		}

		private void UpdateState()
		{
			_controller.InitializeGrid();
			if (Hunger >= 100 || Thirst >= 100 || CurrentHP <= 0)
			{
				//Die();
			}
			Hunger++;
			Thirst++;
			playerPos = _mapManager.GetPlayerPosition();
			
			//Debugging enemy vision, not efficient at all and should be using highlight map later
			if (_visibleCells?.Count != 0)
			{
				StartCoroutine(PaintVisibleTiles(Color.white));
			}
			_visibleCells = _mapManager.GetCellsInRadius(CurrentTile, _viewDistance);
			
			//Check for player
			if (_mapManager.GetCellDistance(playerPos, CurrentTile) <= AggroDistance)
			{
				Debug.Log("Player spotted at: " + playerPos);
				CurrentTarget = LookForPlayer();
			}
			//Check for food and water
			else if (Hunger>50)
			{
				targetType = CellTypes.ItemSpawn;
			}
			else if (Thirst>50)
			{
				targetType = CellTypes.Water;
			}
			targetType = CellTypes.ItemSpawn;

			Debug.Log("Moving enemy unit: " + UnitName);
			CurrentTarget = LookForCellType(targetType);
			_controller.MoveUnit(CurrentTarget);
			_eventManager.InvokeUnitAction(this);
		}

		private IEnumerator PaintVisibleTiles(Color color)
		{
			if (_visibleCells != null)
				foreach (var cell in _visibleCells)
				{
					_tilemap.SetColor(cell.gridPosition, color);
				}
			yield break;
		}

		private Vector3Int LookForCellType(CellTypes cellType)
		{
			var cellTarget = _visibleCells.Find(x => x.cellType == cellType);
			StartCoroutine( PaintVisibleTiles(Color.red));
			if (cellTarget != null)
			{
				var los = GetUnitSight(CurrentTile, cellTarget.gridPosition);
				if (los.Contains(cellTarget.gridPosition))
				{
					return los[0];
				}
			}
			return Wander();
		}

		private Vector3Int LookForPlayer()
		{
			var cellTarget = _mapManager.GetPlayer();
			var dist = _mapManager.GetCellDistance(CurrentTile, playerPos);
			if (cellTarget != null)
			{
				var los = GetUnitSight(CurrentTile, cellTarget.CurrentTile);
				if (los.Contains(cellTarget.CurrentTile) && dist <= AggroDistance)
				{
					return los[0];
				}
			}
			return Wander();
		}

		public void Move(Vector3Int position)
		{
			_controller.MoveUnit(position);
		}

		public void Damage(int dmg)
		{
			Debug.Log("Enemy took " + dmg + " damage!");
			CurrentHP -= dmg;
		}

		public void Die()
		{
			gameObject.SetActive(false);
		}

		public void Consume(ItemUnit itemUnit)
		{
			throw new NotImplementedException();
		}

		public void Equip(ItemUnit itemUnit)
		{
			throw new NotImplementedException();
		}

		// Behaviors/Actions
		
		private Vector3Int Wander()
		{
			var randomDirection = new Random().Next(0, 4);
			var direction = Vector3Int.zero;
			switch (randomDirection)
			{
				case 0:
					direction = Vector3Int.up;
					break;
				case 1:
					direction = Vector3Int.down;
					break;
				case 2:
					direction = Vector3Int.left;
					break;
				case 3:
					direction = Vector3Int.right;
					break;
			}
			return CurrentTile + direction;
		}
		public void Chase(IUnit target)
		{
			var targetPos = target.CurrentTile;
			_controller.MoveTowards(targetPos);
		}
		
		public void Flee(IUnit target)
		{
			_controller.MoveAwayFrom(target.CurrentTile);
		}

		public void Rest()
		{
			Move(Vector3Int.zero);
		}
		
		public void Eat()
		{
			Hunger = 0;
		}
		
		public void Drink()
		{
			Thirst = 0;
		}
		//Not efficient, rework later
		public void Attack(IUnit source, IUnit target)
		{
			if (this == source)
			{
				target.Damage(1);
			}
		}
		
		//Testing Bresenhams Line Algorithm
		private List<Vector3Int> GetUnitSight(Vector3Int source, Vector3Int target)
		{
			var gridMap = _mapManager.GetGridMap();
			//Debug.DrawLine( map.CellToWorld(source) , map.CellToWorld(target), Color.red, 5f);
			var losCells = GridUtils.GetLineOfSight(source, target);
			var visibleLos= new List<Vector3Int>();
			foreach (var losCell in losCells)
			{
				if (gridMap[losCell.x, losCell.y].cellType == CellTypes.Wall)
				{
					break;
				}
				visibleLos.Add(losCell);
			}
			return visibleLos;
		}

	}
}