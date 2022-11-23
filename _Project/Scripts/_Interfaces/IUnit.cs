using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Interfaces
{
	public interface IUnit
	{
		public string UnitName { get; set; }
		public string Description { get; set; }
		public Vector3Int CurrentPos { get; set; }
		public void InitializeUnit();
		public void Move(Vector3Int position);
		public void SetPosition(Vector3Int position);
		public void Attack(IUnit source, IUnit target);
		public void Damage(int dmg);
		public void Die();
		public void Consume(ItemUnit itemUnit);
		public void Equip(ItemUnit itemUnit);
	}
}