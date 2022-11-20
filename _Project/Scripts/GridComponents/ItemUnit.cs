using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.Controllers;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	[RequireComponent(typeof(DraggableController))]
	public class ItemUnit : MonoBehaviour, IUnit, IItem
	{
		private ItemData _itemData;
		private SpriteRenderer _spriteRenderer;
		private MapManager _mapManager;
		private EventManager _eventManager;
		public Vector3Int CurrentTile { get; set; }
		public ItemType ItemType { get; set; }
		public Sprite Icon { get; set; }
		public string UnitName { get; set; }
		public string Description { get; set; }

		public IUnit GridUnit { get; set; }
		

		private void Awake()
		{
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			_mapManager = MapManager.Instance;
			_eventManager = EventManager.Instance;
			//Icon = _itemData.sprite;
			//UnitName = _itemData.itemName;
			//Description = _itemData.description;
			//ItemType = _itemData.itemType;
			
		}

		public void InitializeItem(ItemData itemData, IUnit unit)
		{
			Icon = itemData.sprite;
			_spriteRenderer.sprite = itemData.sprite;
			GridUnit = unit;
			UnitName = itemData.itemName;
			Description = itemData.description;
			ItemType = itemData.itemType;
		}
		
		public void Use(Vector3Int target)
		{
			switch (ItemType)
			{
				//Use events to notify target unless consumable, in which case target is unit using item
				case ItemType.Consumable:
					// Use item on target
					break;
				case ItemType.Equipment:
					// Equip item on target
					break;
				case ItemType.Throwable:
					// Throw item at target
					break;
				case ItemType.Weapon:
					// Equip target with weapon
					break;
				case ItemType.Key:
					// Unlock target
					break;
				case ItemType.Gold:
					// Add gold to target
					break;
				default:
					break;
			}
		}

		public void InitializeUnit()
		{
			throw new System.NotImplementedException();
		}

		public void Move(Vector3Int position)
		{
			throw new System.NotImplementedException();
		}
		
		public void SetPosition(Vector3Int position)
		{
			var cellMap = _mapManager.GetCellMap();
			if (cellMap != null)
			{
				var currentCell = cellMap[position.x, position.y];
				if (currentCell != null
				    && cellMap[position.x, position.y].Occupant == null
				    && cellMap[position.x, position.y].isWalkable)
				{
					var previousCell = cellMap[CurrentTile.x, CurrentTile.y];
				
					previousCell.Occupant = null;
					previousCell.isWalkable = true;
					currentCell.Occupant = this;
					currentCell.isWalkable = false;
					//need to refactor, used to be isEmpty
					transform.position = position;
					CurrentTile = position;
				
					_eventManager.InvokeCellUpdate(previousCell);
					_eventManager.InvokeCellUpdate(currentCell);
					//_mapManager.UpdateCellMap(cellMap);
				}
				else
				{
					transform.position = CurrentTile;
				}
			}
		}

		public void Consume(ItemUnit itemUnit)
		{
			throw new System.NotImplementedException();
		}

		public void Equip(ItemUnit itemUnit)
		{
			throw new System.NotImplementedException();
		}

		public void Attack(IUnit source, IUnit target)
		{
			throw new System.NotImplementedException();
		}

		public void Damage(int dmg)
		{
			throw new System.NotImplementedException();
		}

		public void Die()
		{
			gameObject.SetActive(false);
		}
	}
}