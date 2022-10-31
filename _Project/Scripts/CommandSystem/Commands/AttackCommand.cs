using System.Collections;
using DungeonAutomata._Project.Scripts._Interfaces;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.CommandSystem.Commands
{
	public class AttackCommand : ICommand
	{
		private readonly CombatUnit _combatUnit;

		public AttackCommand(CombatUnit combat_unit)
		{
			_combatUnit = combat_unit;
		}

		public IEnumerator Execute()
		{
			//possibly replace with generic logic and simply pass character/attack data into each concrete command
			_combatUnit.ExecuteAttack();
			yield return GetWaitForSeconds(duration);
		}

		public float duration { get; set; }
	}
}