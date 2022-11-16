using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Interfaces
{
	public interface IItem
	{
		public Sprite Icon { get; set; }
		public string UnitName { get; set; }
		public string Description { get; set; }
		public void Use(Vector3Int target);
		public IUnit GridUnit{ get; set; }
		
		public void InitializeItem(ItemData itemData, IUnit unit);
	}
}