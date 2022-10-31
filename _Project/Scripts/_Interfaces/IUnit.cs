using DungeonAutomata._Project.Scripts.GridComponents;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Interfaces
{
	public interface IUnit
	{
		string UnitName { get; set; }
		public Vector3Int CurrentTile { get; set; }
		public void InitializeUnit();
		public void Move(Vector3Int position);
		public void Attack(IUnit source, IUnit target);
		public void Damage(int dmg);
		public void Die();
		public void Consume(ItemUnit itemUnit);
		public void Equip(ItemUnit itemUnit);
	}
}