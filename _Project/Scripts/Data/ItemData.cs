using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Data
{
	[ManageableData]
	[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "ItemData", order = 0)]
	public class ItemData : SerializedScriptableObject
	{
		[PreviewField(75, ObjectFieldAlignment.Left)]
		public Sprite sprite;
		public string itemName;
		public ItemType itemType;
		public Dictionary<StatModifier, float> StatModifiers;
		public int itemValue;
		public int itemAmount;
		public string description;
	}

	public enum ItemType
	{
		Consumable,
		Throwable,
		Equipment,
		Weapon,
		Gold,
		Key
	}

	public enum StatModifier
	{
		Health,
		Mana,
		Energy,
		Hunger,
		Thirst,
		Level,
		Exp,
		Strength,
		Dexterity,
		Intelligence,
		Luck
	}
}