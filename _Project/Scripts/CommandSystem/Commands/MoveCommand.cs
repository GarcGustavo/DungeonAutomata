using System.Collections;
using System.Numerics;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;
using static DungeonAutomata._Project.Scripts.Utilities.Utils;

namespace DungeonAutomata._Project.Scripts.CommandSystem.Commands
{
	public class MoveCommand : ICommand
	{
		private readonly ICombatUnit _combatUnit;
		private readonly Vector3Int _target;

		public MoveCommand(ICombatUnit combatUnit, Vector3Int target)
		{
			_combatUnit = combatUnit;
			_target = target;
		}

		public void Execute()
		{
			_combatUnit.MoveTurnBased(_target);
		}
	}
}