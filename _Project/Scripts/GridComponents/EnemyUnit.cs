using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.CommandSystem.Commands;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;
using static DungeonAutomata._Project.Scripts.Utilities.GridUtils;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(GridController2D))]
	public class EnemyUnit : MonoBehaviour, IUnit, ICombatUnit
	{
		[SerializeField] private EnemyData _enemyData;
		[SerializeField] private int _viewDistance = 5;
		
		private GameManager _gameManager;
		private MapManager _mapManager;
		private EventManager _eventManager;
		public string UnitName { get; set; }
		public string Description { get; set; }
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
		private CellData[,] _cellMap;
		public bool CanMove { get; set; }
		private GridController2D _controller;

		private void Awake()
		{
			_gameManager = GameManager.Instance;
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
			_tilemap = _mapManager.GetTileMap();
			MaxHP = _enemyData.Health;
			CurrentHP = MaxHP;
			CanMove = true;
			SetDescription();
			//_controller.InitializeGrid();
		}
		
		private void SetDescription()
		{
			Description = $"Name: {_enemyData.name}\n" +
			               $"HP: {CurrentHP}\n" +
			               $"Hunger: {Hunger}\n" +
			               $"Thirst: {Thirst}\n" +
			               $"{_enemyData.description}\n";
		}

		private void UpdateState()
		{
			if (CurrentHP <= 0)
			{
				Die();
				return;
			}
			_controller.InitializeGrid();
			_cellMap = _mapManager.GetCellMap();
			Hunger++;
			Thirst++;
			CurrentTarget = LookForPlayer();
			if (CurrentTarget != null)
			{
				_gameManager.RegisterCommand(new MoveCommand(this, CurrentTarget));
				//Move(CurrentTarget);
			}
			_eventManager.InvokeUnitAction(this);
		}

		private void PaintCells(List<Vector3Int> cells, Color color)
		{
			if (cells != null)
			{
				foreach (var cell in cells)
				{
					_tilemap.SetColor(cell, color);
				}
			}
		}

		private Vector3Int LookForPlayer()
		{
			var cellTarget = _mapManager.GetPlayer();
			var valueMap = _mapManager.GetPlayerMap();
			var nextCell = GetLowestCostAdjacentCell(CurrentTile, ref valueMap);
			if (cellTarget != null)
			{
				var losCells = GetLine(CurrentTile, cellTarget.CurrentTile);
				if(GetAdjacentCells(CurrentTile, _cellMap).Contains(_cellMap[playerPos.x, playerPos.y]))
					return playerPos;
				foreach (var cell in losCells)
				{
					if (_cellMap[cell.x, cell.y].cellType == CellTypes.Wall 
					    && GetCellDistance(playerPos, CurrentTile) >= AggroDistance)
					{
						return Wander();
					}
				}
			}
			playerPos = _mapManager.GetPlayerPosition();
			return nextCell;
		}

		public void Move(Vector3Int position)
		{
			_controller.MoveUnit(position);
		}

		public void Damage(int dmg)
		{
			CurrentHP -= dmg;
		}

		public void Die()
		{
			_eventManager.InvokeUnitDeath(this);
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

		public void Attack(Vector3Int target, AttackData data)
		{
			Debug.Log("Attack method not implemented yet");
		}
	}
}