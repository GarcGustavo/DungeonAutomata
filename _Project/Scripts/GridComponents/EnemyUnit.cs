using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.CommandSystem.Commands;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;
using Random = System.Random;
using static DungeonAutomata._Project.Scripts._Common.GridUtils;
using Utils = DungeonAutomata._Project.Scripts.Utilities.Utils;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(GridController2D))]
	[RequireComponent(typeof(DraggableController))]
	public class EnemyUnit : MonoBehaviour, IUnit, ICombatUnit
	{
		[SerializeField] private EnemyData _enemyData;
		
		private TopDownManager _turnManager;
		private MapManager _mapManager;
		private EventManager _eventManager;
		public string UnitName { get; set; }
		public StatsData UnitStats { get; set; }
		public string Description { get; set; }
		public int MaxHP { get; set; }
		public int CurrentHP { get; set; }
		public int Hunger { get; set; }
		public int Thirst { get; set; }
		public int AggroDistance { get; set; }
		
		public Vector3Int CurrentPos { get; set; }
		private List<CellData> _visibleCells;
		public Vector3Int CurrentTarget { get; set; }
		private Vector3Int playerPos;
		private CellTypes targetType;
		private Tilemap _tilemap;
		private CellData[,] _cellMap;
		public bool CanMove { get; set; }
		private GridController2D _controller;
		private SpriteRenderer _spriteRenderer;

		private void Awake()
		{
			_turnManager = TopDownManager.Instance;
			_mapManager = MapManager.Instance;
			_eventManager = EventManager.Instance;
			_controller = GetComponent<GridController2D>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			//_eventManager.OnEnemyTurnStart += UpdateState;
		}

		private void Start()
		{
			_eventManager.OnEnemyTurnStart += UpdateState;
		}

		//TODO: remove redundant overload from interface
		public void InitializeUnit()
		{
			_tilemap = _mapManager.GetTileMap();
			_spriteRenderer.sprite = _enemyData.sprite;
			AggroDistance = _enemyData.aggroDistance;
			UnitName = _enemyData.enemyName;
			MaxHP = _enemyData.Health;
			CurrentHP = MaxHP;
			CanMove = true;
			SetDescription();
			//_controller.InitializeGrid();
		}

		public void InitializeUnit(EnemyData unitData)
		{
			_enemyData = unitData;
			_tilemap = _mapManager.GetTileMap();
			_spriteRenderer.sprite = _enemyData.sprite;
			AggroDistance = _enemyData.aggroDistance;
			UnitName = _enemyData.enemyName;
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

		public void UpdateState()
		{
			Debug.Log("Enemy initializing controller grid");
			_cellMap = _mapManager.GetCellMap();
			_controller.InitializeGrid();
			if (CanMove)
			{
				CurrentTarget = LookForPlayer();
				if (CurrentTarget != null)
				{
					//_turnManager.RegisterCommand(new MoveCommand(this, CurrentTarget));
					//Move(CurrentTarget);
				}
				if (CurrentHP <= 0)
				{
					Die();
					//_eventManager.InvokeUnitAction(this);
					return;
				}
				Debug.Log("Enemy registering command");
				_turnManager.RegisterCommand(new MoveCommand(this, CurrentTarget));
				_eventManager.InvokeUnitAction(this);
				Hunger++;
				Thirst++;
			}
			else
			{
				_eventManager.InvokeUnitAction(this);
			}
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
		
		public void SetPosition(Vector3Int position)
		{
			if (_cellMap != null
			    && _cellMap[position.x, position.y] != null
			    && _cellMap[position.x, position.y].Occupant == null
			    && _cellMap[position.x, position.y].isWalkable)
			{
				_controller.SetPosition(position);
				CurrentPos = position;
			}
			else
			{
				_controller.SetPosition(CurrentPos);
			}
		}
		
		public void Grab(Vector3Int target)
		{
			var cell = _mapManager.GetCellMap()[target.x, target.y];
			if (cell.Occupant != null)
			{
				var unit = cell.Occupant;
				if (unit != null)
				{
					cell.Occupant = null;
					unit.CurrentPos = new Vector3Int(-1, -1, -1);
					_heldUnit = unit;
					_bGrabbing = true;
					unit.SetGrabbedBy(transform);
				}
			}
		}

		private bool _bGrabbing = false;
		private IUnit _heldUnit;
		public void Throw(Vector3Int target)
		{
			Debug.Log("Throw at: " + target);
			var cell = _cellMap[target.x, target.y];
			if (_bGrabbing)
			{
				var unit = _heldUnit;
				unit?.ThrowToPosition(target);
				_bGrabbing = false;
				_heldUnit = null;
			}
		}

		public void ThrowToPosition(Vector3Int target)
		{
			Debug.Log("Throw at: " + target);
			//TODO: Add to utilities class and create coroutine to prevent movement while playing animation
			//transform.DOJump(target, 1f, 1, .5f);
			//transform.position = target;
			StartCoroutine(ThrowAnimation(target));
		}

		private IEnumerator ThrowAnimation(Vector3Int target)
		{
			transform.DOJump(target, 1f, 1, .5f);
			//transform.DOMove(target, .5f);
			yield return Utils.GetWaitForSeconds(.5f);
			transform.SetParent(null);
			CanMove = true;
			CurrentPos = target;
			SetPosition(target);
			_mapManager.UpdateCell(_cellMap[CurrentPos.x, CurrentPos.y]);
		}
		
		public void SetGrabbedBy(Transform grabber)
		{
			transform.position = grabber.position + Vector3.up/4 + Vector3.back/2;
			transform.SetParent(grabber);
			CanMove = false;
			_cellMap[CurrentPos.x, CurrentPos.y].Occupant = null;
			_cellMap[CurrentPos.x, CurrentPos.y].isWalkable = true;
			_mapManager.UpdateCell(_cellMap[CurrentPos.x, CurrentPos.y]);
			CurrentPos = new Vector3Int(-1, -1, -1);
		}

		private Vector3Int LookForPlayer()
		{
			var cellTarget = _mapManager.GetPlayer();
			var valueMap = _mapManager.GetPlayerMap();
			var nextCell = GetLowestCostAdjacentCell(CurrentPos, ref valueMap);
			if (cellTarget != null)
			{
				var losCells = GetLine(CurrentPos, cellTarget.CurrentPos);
				if(GetAdjacentCells(CurrentPos, _cellMap).Contains(_cellMap[playerPos.x, playerPos.y]))
					return playerPos;
				foreach (var cell in losCells)
				{
					if (_cellMap[cell.x, cell.y].cellType == CellTypes.Wall 
					    && GetCellDistance(playerPos, CurrentPos) >= AggroDistance)
					{
						return Wander();
					}
				}
			}
			playerPos = _mapManager.GetPlayerPosition();
			return nextCell;
		}

		public void MoveTurnBased(Vector3Int position)
		{
			_controller.MoveUnit(position);
		}

		public void Damage(int dmg)
		{
			CurrentHP -= dmg;
		}

		public void Die()
		{
			Debug.Log("Enemy died at: " + CurrentPos);
			var cell = _cellMap[CurrentPos.x, CurrentPos.y];
			cell.Occupant = null;
			cell.isWalkable = true;
			_eventManager.InvokeCellUpdate(cell);
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
			return CurrentPos + direction;
		}
		public void Chase(IUnit target)
		{
			var targetPos = target.CurrentPos;
			_controller.MoveTowards(targetPos);
		}
		
		public void Flee(IUnit target)
		{
			_controller.MoveAwayFrom(target.CurrentPos);
		}

		public void Rest()
		{
			_eventManager.InvokeUnitAction(this);
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