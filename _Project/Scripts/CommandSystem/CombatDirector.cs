using System;
using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts.CommandSystem
{
	[CreateAssetMenu(fileName = "Data", menuName = "Managers/CombatDirector", order = 0)]
	public class CombatDirector : ScriptableObject
	{
		private List<ICommand> _actionList;
		private bool _isRunning;

		// private List<GameObject> _playerParty;
		// private List<GameObject> _enemyParty;

		//Invoker Function
		public IEnumerator ExecuteCommands()
		{
			foreach (var command in _actionList)
			{
				command.Execute();
				yield return GetWaitForSeconds(command.duration);
			}
		}

		public void RegisterCommand(ICommand command)
		{
			// Play around with order execution via unit stats later
			_actionList.Add(command);
		}

		private void GetPlayerParty()
		{
		}

		private void GetEnemyParty()
		{
		}

		//battle logic
		private void BattleLogic()
		{
			//get player and enemy party

			//for each unit in player party request action
			//for each unit in enemy party request action
			//execute actions
		}

		private void NextTurn()
		{
			throw new NotImplementedException();
		}

		public void StartCombat()
		{
			_isRunning = true;
			throw new NotImplementedException();
		}

		public void EndCombat()
		{
			_isRunning = false;
			throw new NotImplementedException();
		}

		public bool IsRunning()
		{
			return _isRunning;
		}

		public void KillUnit(CombatUnit combat_unit)
		{
			combat_unit.gameObject.SetActive(false);
		}
	}
}