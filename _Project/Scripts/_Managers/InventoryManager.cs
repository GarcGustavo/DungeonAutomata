using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public class InventoryManager : MonoBehaviour
	{
		public static InventoryManager Instance { get; private set; }
		private TopDownManager _turnManager;
		private EventManager _eventManager;
		public List<IItem> Items;
		public List<IItem> Equipment;
		public ItemUnit Weapon;
		
		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				Items = new List<IItem>();
				Equipment = new List<IItem>();
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			_turnManager = TopDownManager.Instance;
			_eventManager = EventManager.Instance;
			_eventManager.OnPickup += AddItem;
		}

		public void AddItem(IItem item)
		{
			Debug.Log("Item added");
			Items.Add(item);
			_eventManager.InvokeUpdateInventory(item);
		}
		
		public void RemoveItem(ItemUnit item)
		{
			Items.Remove(item);
		}
		
		public void EquipItem(ItemUnit item)
		{
			if (item.ItemType == ItemType.Weapon)
			{
				if (Weapon != null)
				{
					Items.Add(Weapon);
				}
				Weapon = item;
				Items.Remove(item);
			}
			else
			{
				Equipment.Add(item);
				Items.Remove(item);
			}
		}
		
		public void UnequipItem(ItemUnit item)
		{
			if (item.ItemType == ItemType.Weapon)
			{
				Items.Add(Weapon);
				Weapon = null;
			}
			else
			{
				Items.Add(item);
				Equipment.Remove(item);
			}
		}
	}
}