using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.CommandSystem;
using DungeonAutomata._Project.Scripts.CommandSystem.Commands;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.Utilities;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(GridController2D))]
	[RequireComponent(typeof(DraggableController))]
	public class PlayerUnit : MonoBehaviour, IUnit, ICombatUnit
	{
		[SerializeField] private PlayerData playerData;
		private InventoryManager _inventory;
		private GameManager _manager;
		private EventManager _eventManager;
		private MapManager _mapManager;
		private CellData[,] _cellMap;

		public string Description { get; set; }
		public Vector3Int CurrentTile { get; set; }
		private GridController2D _controller;
		public string UnitName { get; set; }
		public int MaxHP { get; set; }
		public int CurrentHP { get; set; }
		public int MaxMP { get; set; }
		public int CurrentMP { get; set; }
		public int MaxEnergy { get; set; }
		public int CurrentEnergy { get; set; }
		public int Hunger { get; set; }
		public int Thirst { get; set; }
		private int _level;
		private int _exp;
		private int _strength;
		private int _dexterity;
		private int _intelligence;
		private int _luck;

		private bool _inputEnabled = true;
		[SerializeField] private bool zAxisUp;

		private void Awake()
		{
			_controller = GetComponent<GridController2D>();
			var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			spriteRenderer.sprite = playerData.sprite;
			_manager = GameManager.Instance;
			_eventManager = EventManager.Instance;
			_inventory = InventoryManager.Instance;
			_mapManager = MapManager.Instance;
			_eventManager.OnPlayerDamaged += Damage;
			//_eventManager.OnPlayerAction += UseEnergy;
			_eventManager.OnPlayerTurnStart += RefillEnergy;
			CurrentTile = new Vector3Int();
			UnitName = playerData.name;
		}

		public void InitializeUnit()
		{
			MaxHP = playerData.Health;
			CurrentHP = MaxHP;
			MaxMP = playerData.Mp;
			CurrentMP = playerData.Mp;
			MaxEnergy = playerData.Energy;
			CurrentEnergy = MaxEnergy;
			
			_strength = playerData.Strength;
			_dexterity = playerData.Dexterity;
			_intelligence = playerData.Intelligence;
			_luck = playerData.Luck;
			_level = playerData.Level;
			_exp = playerData.Exp;
			
			Hunger = 0;
			Thirst = 0;
			_cellMap = _mapManager.GetCellMap();
			
			_controller.InitializeGrid();
			UpdateDescription();
			SetMoveState(true);
		}
		
		public void UpdateDescription()
		{
			Description = $"Name: {playerData.name}\n" +
			              $"Level: {_level}\n" +
			              $"Exp: {_exp}\n" +
			              $"Health: {CurrentHP}/" + $"{MaxHP}\n" +
			              $"Mp: {CurrentMP}/" + $"{MaxMP}\n" +
			              $"Energy: {CurrentEnergy}/" + $"{MaxEnergy}\n" +
			              $"Strength: {_strength}\n" +
			              $"Dexterity: {_dexterity}\n" +
			              $"Intelligence: {_intelligence}\n" +
			              $"Luck: {_luck}\n" +
			              $"Hunger: {Hunger}\n" +
			              $"Thirst: {Thirst}\n";
		}

		public void RefillEnergy()
		{
			SetMoveState(true);
			CurrentEnergy = MaxEnergy;
		}

		public void UseEnergy()
		{
			if (CurrentEnergy <= 0)
			{
				SetMoveState(false);
				_eventManager.InvokeTurnEnd();
			}
			else
			{
				CurrentEnergy--;
				SetMoveState(true);
				_eventManager.InvokePlayerAction();
			}
		}

		public void Damage(int dmg)
		{
			Debug.Log("Player took " + dmg + " damage!");
			CurrentHP -= dmg;
			_eventManager.OnPlayerDamaged -= Damage;
			if (CurrentHP <= 0)
			{
				Die();
			}
		}

		public void SetMoveState(bool isMoving)
		{
			_inputEnabled = isMoving;
			//_controller.CanMove = isMoving;
		}

		public void Move(Vector3Int position)
		{
			Debug.Log("Player moving to: " + position + " from: " + CurrentTile);
			_controller.MoveUnit(position);
		}
		
		public void SetPosition(Vector3Int position)
		{
			_cellMap = _mapManager.GetCellMap();
			if (_cellMap != null
			    && _cellMap[position.x, position.y] != null
			    && _cellMap[position.x, position.y].Occupant == null
			    && _cellMap[position.x, position.y].isWalkable)
			{
				_controller.SetPosition(position);
				CurrentTile = position;
			}
			else
			{
				transform.position = CurrentTile;
			}
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
			throw new System.NotImplementedException();
			UpdateDescription();
		}

		public void Die()
		{
			_eventManager.InvokePlayerDeath();
			gameObject.SetActive(false);
		}

		public void Consume(ItemUnit itemUnit)
		{
			throw new System.NotImplementedException();
		}

		public void Equip(ItemUnit itemUnit)
		{
			throw new System.NotImplementedException();
		}

		public void GetPlayerInput()
		{
			if (_inputEnabled)
			{
				if (Input.GetKey(KeyCode.W) && !Input.GetKeyUp(KeyCode.W))
				{
					var direction = zAxisUp ? Vector3Int.forward : Vector3Int.up;
					_manager.RegisterCommand(new MoveCommand(this, CurrentTile + direction));
					UseEnergy();
					//Move(CurrentTile + direction);
				}
				else if (Input.GetKey(KeyCode.S) && !Input.GetKeyUp(KeyCode.S))
				{
					var direction = zAxisUp ? Vector3Int.back : Vector3Int.down;
					_manager.RegisterCommand(new MoveCommand(this, CurrentTile + direction));
					UseEnergy();
					//Move(CurrentTile + direction);
				}
				else if (Input.GetKey(KeyCode.A) && !Input.GetKeyUp(KeyCode.A))
				{
					_manager.RegisterCommand(new MoveCommand(this, CurrentTile + Vector3Int.left));
					UseEnergy();
					//Move(CurrentTile + Vector3Int.left);
				}
				else if (Input.GetKey(KeyCode.D) && !Input.GetKeyUp(KeyCode.D))
				{
					_manager.RegisterCommand(new MoveCommand(this, CurrentTile + Vector3Int.right));
					UseEnergy();
					//Move(CurrentTile + Vector3Int.right);
				}
				else if (Input.GetKeyDown(KeyCode.Tab))
				{
					_eventManager.InvokeMenu();
				}
				else if (Input.GetKey(KeyCode.Space) && !Input.GetKeyUp(KeyCode.Space))
				{
					//Essentially a fast forward button
					_eventManager.InvokePlayerAction();
					UseEnergy();
				}
			}
		}
		
	}
}