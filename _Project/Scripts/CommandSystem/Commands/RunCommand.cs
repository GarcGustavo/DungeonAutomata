using System.Collections;
using DungeonAutomata._Project.Scripts._Interfaces;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.CommandSystem.Commands
{
	public class RunCommand : ICommand
	{
		private CombatUnit _combatUnit;

		public IEnumerator Execute()
		{
			//possibly replace with generic logic and simply pass character/attack data into each concrete command
			_combatUnit.ExecuteAttack();
			yield return GetWaitForSeconds(duration);
		}

		public float duration { get; set; }
	}
}