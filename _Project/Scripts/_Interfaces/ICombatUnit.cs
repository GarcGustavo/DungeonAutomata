using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Interfaces
{
	public interface ICombatUnit
	{
		public int MaxHP { get; set; }
		public int CurrentHP { get; set; }
		public void Move(Vector3Int position);
		public void Attack(IUnit source, IUnit target);
		public void Attack(Vector3Int target, AttackData data);
	}
}