using System;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class UnitStatsUI : MonoBehaviour
	{
		private ICombatUnit _unit;
		private float _maxHP;
		private float _currentHP;
		[SerializeField] private SpriteRenderer spriteRenderer;
		
		//Eventually will manage unit hp bars/ui, need to communicate with combat and ui managers
		private void Awake()
		{
			_unit = GetComponent<ICombatUnit>();
			_maxHP = _unit.MaxHP;
			_currentHP = _unit.CurrentHP;
		}

		private void Update()
		{
			UpdateStats();
		}

		private void UpdateStats()
		{
			_currentHP = _unit.CurrentHP;
			//spriteRenderer.color = Color.Lerp(Color.red, Color.white, _currentHP / _maxHP);
		}

	}
}