using System.Collections;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.Data;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.CommandSystem.Commands
{
	public class AttackCommand : ICommand
	{
		private readonly ICombatUnit _combatUnit;
		private readonly Vector3Int _target;
		private readonly AttackData _data;

		public AttackCommand(ICombatUnit combatUnit, Vector3Int target, AttackData data)
		{
			_combatUnit = combatUnit;
			_target = target;
			_data = data;
		}

		public void Execute()
		{
			//possibly replace with generic logic and simply pass character/attack data into each concrete command
			_combatUnit.Attack(_target, _data);
		}
	}
}