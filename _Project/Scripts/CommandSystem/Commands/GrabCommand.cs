using System.Collections;
using System.Numerics;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.CommandSystem.Commands
{
	public class GrabCommand : ICommand
	{
		private readonly ICombatUnit _combatUnit;
		private readonly Vector3Int _target;

		public GrabCommand(ICombatUnit combatUnit, Vector3Int target)
		{
			_combatUnit = combatUnit;
			_target = target;
		}

		public void Execute()
		{
			_combatUnit.Grab(_target);
		}
	}
}