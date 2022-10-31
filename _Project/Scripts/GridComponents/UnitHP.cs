using System;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class UnitHP : MonoBehaviour
	{
		private ICombatUnit _unit;
		private float _maxHP;
		private float _currentHP;
		[SerializeField] private SpriteRenderer spriteRenderer;
		
		private void Awake()
		{
			_unit = GetComponent<ICombatUnit>();
			_maxHP = _unit.MaxHP;
			_currentHP = _unit.CurrentHP;
		}

		private void Update()
		{
			UpdateSprite();
		}

		private void UpdateSprite()
		{
			_currentHP = _unit.CurrentHP;
			//spriteRenderer.color = Color.Lerp(Color.red, Color.white, _currentHP / _maxHP);
		}

	}
}