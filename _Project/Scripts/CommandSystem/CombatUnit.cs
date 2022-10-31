using System;
using System.Collections;
using DungeonAutomata._Project.Scripts.CommandSystem.Commands;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.CommandSystem
{
	public class CombatUnit : MonoBehaviour
	{
		public int _hp = 100;
		public int _mp = 100;

		private CombatDirector _director;

		//Status Info
		private CombatUnitData _unit;

		public void Init(CombatDirector director, CombatUnitData unit)
		{
			_director = director;
			_unit = unit;
			_hp = unit.hp;
			_mp = unit.mp;
		}

		public void TakeDamage(int damage)
		{
			_hp -= damage;
			if (_hp <= 0)
			{
				_hp = 0;
				_director.KillUnit(this);
			}
		}

		//Command request API - Call from game loop to queue up actions
		public void RegisterAttack()
		{
			_director.RegisterCommand(new AttackCommand(this));
		}

		public void RegisterItem()
		{
			_director.RegisterCommand(new AttackCommand(this));
		}

		public void RegisterRun()
		{
			_director.RegisterCommand(new AttackCommand(this));
		}

		public void RegisterCry()
		{
			_director.RegisterCommand(new AttackCommand(this));
		}

		//Command implementations - Called by individual concrete command class
		public IEnumerator ExecuteAttack()
		{
			throw new NotImplementedException();
		}

		public IEnumerator ExecuteItem()
		{
			throw new NotImplementedException();
		}

		public IEnumerator ExecuteRun()
		{
			throw new NotImplementedException();
		}

		public IEnumerator ExecuteTalk()
		{
			throw new NotImplementedException();
		}
	}
}