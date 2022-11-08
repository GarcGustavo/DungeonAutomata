using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class ItemUnit : MonoBehaviour, IUnit, IItem
	{
		private ItemData _itemData;
		private SpriteRenderer _spriteRenderer;
		public Vector3Int CurrentTile { get; set; }
		public ItemType ItemType { get; set; }
		public Sprite Icon { get; set; }
		public string UnitName { get; set; }
		public string Description { get; set; }
		

		private void Awake()
		{
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			//Icon = _itemData.sprite;
			//UnitName = _itemData.itemName;
			//Description = _itemData.description;
			//ItemType = _itemData.itemType;
			
		}

		public void InitializeItem(ItemData itemData)
		{
			Icon = itemData.sprite;
			_spriteRenderer.sprite = itemData.sprite;
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