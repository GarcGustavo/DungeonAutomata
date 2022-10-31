using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(GridController2D))]
	public class PlayerUnit : MonoBehaviour, IUnit, ICombatUnit
	{
		[SerializeField] private PlayerData playerData;
		private InventoryManager _inventory;
		private GameManager _manager;
		private EventManager _eventManager;
		private MapManager _mapManager;

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

		[SerializeField] private bool zAxisUp;

		private void Awake()
		{
			_controller = GetComponent<GridController2D>();
			var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			spriteRenderer.sprite = playerData.sprite;
			_manager = GameManager.Instance;
			_eventManager = EventManager.Instance;
			_inventory = InventoryManager.Instance;
			_eventManager.OnPlayerDamaged += Damage;
			_eventManager.OnPlayerAction += UseEnergy;
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
			
			_controller.InitializeGrid();
		}

		public void RefillEnergy()
		{
			SetMoveState(true);
			CurrentEnergy = MaxEnergy;
		}

		public void UseEnergy()
		{
			CurrentEnergy--;
			if (CurrentEnergy <= 0)
			{
				_eventManager.InvokePlayerTurnEnd();
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

		public void SetMoveState(bool is_moving)
		{
			_controller.CanMove = is_moving;
		}

		public void Move(Vector3Int position)
		{
			_controller.MoveUnit(position);
		}

		//Not efficient, rework later
		public void Attack(IUnit source, IUnit target)
		{
			if (this == source)
			{
				target.Damage(1);
			}
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

		public List<CellData> GetNeighbors()
		{
			return _mapManager.GetCellsInRadius(CurrentTile, 1);
		}

		public void GetPlayerInput()
		{
			if (Input.GetKey(KeyCode.W) && !Input.GetKeyUp(KeyCode.W))
			{
				var direction = zAxisUp ? Vector3Int.forward : Vector3Int.up;
				Move(CurrentTile + direction);
			}
			else if (Input.GetKey(KeyCode.S) && !Input.GetKeyUp(KeyCode.S))
			{
				var direction = zAxisUp ? Vector3Int.back : Vector3Int.down;
				Move(CurrentTile + direction);
			}
			else if (Input.GetKey(KeyCode.A) && !Input.GetKeyUp(KeyCode.A))
			{
				Move(CurrentTile + Vector3Int.left);
			}
			else if (Input.GetKey(KeyCode.D) && !Input.GetKeyUp(KeyCode.D))
			{
				Move(CurrentTile + Vector3Int.right);
			}
			else if (Input.GetKeyDown(KeyCode.Tab))
			{
				_eventManager.InvokeMenu();
			}
			else if (Input.GetKey(KeyCode.Space) && !Input.GetKeyUp(KeyCode.Space))
			{
				//Essentially a fast forward button
				_eventManager.InvokePlayerAction();
			}
		}
	}
}