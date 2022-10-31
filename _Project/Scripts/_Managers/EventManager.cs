using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts._Interfaces;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public class EventManager : MonoBehaviour
	{
		public static EventManager Instance { get; private set; }

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject);
		}

		// State Events
		public event Action OnStartGame;
		public event Action OnPlayerSpawn;
		public event Action OnMenu;
		public event Action OnPlayerTurnStart;
		public event Action OnEnemyTurnStart;
		public event Action OnPlayerTurnEnd;
		public event Action OnEnemyTurnEnd;
		
		// Player Events
		public event Action<int> OnPlayerDamaged;
		public event Action<int> OnPlayerHealed;
		public event Action OnPlayerDeath;
		public event Action OnPlayerGoalReached;
		public event Action OnPlayerAction;
		public event Action<IItem> OnItemSelect; 

		public event Action<Vector3Int> OnPlayerMove;

		// General Unit Events, possibly need to add unique id to each unit
		public event Action<IUnit, List<Vector3Int>> OnAttack;
		public event Action<IItem> OnPickup;
		public event Action OnUpdateInventory;
		public event Action<Vector3Int> OnUnitMove;
		public event Action<IUnit> OnUnitAction;

		#region Invokes

		// State Events
		public void InvokeStartGame()
		{
			OnStartGame?.Invoke();
		}

		public void InvokePlayerSpawn()
		{
			OnPlayerSpawn?.Invoke();
		}

		public void InvokeMenu()
		{
			OnMenu?.Invoke();
		}

		public void InvokePlayerTurnStart()
		{
			OnPlayerTurnStart?.Invoke();
		}

		public void InvokePlayerTurnEnd()
		{
			OnPlayerTurnEnd?.Invoke();
		}

		public void InvokeEnemyTurnStart()
		{
			OnEnemyTurnStart?.Invoke();
		}

		public void InvokeEnemyTurnEnd()
		{
			OnEnemyTurnEnd?.Invoke();
		}

		// Unit Events
		public void InvokePlayerMove(Vector3Int position)
		{
			OnPlayerMove?.Invoke(position);
		}

		public void InvokeUnitMove(Vector3Int position)
		{
			OnUnitMove?.Invoke(position);
		}

		public void InvokeUnitAction(IUnit unit)
		{
			OnUnitAction?.Invoke(unit);
		}

		public void InvokeAttack(IUnit source, List<Vector3Int> targetCells)
		{
			OnAttack?.Invoke(source, targetCells);
		}

		public void InvokePickup(IItem item)
		{
			Debug.Log("Pickup Event");
			OnPickup?.Invoke(item);
		}

		public void InvokeUpdateInventory(IItem item)
		{
			Debug.Log("Inventory Updated");
			OnUpdateInventory?.Invoke();
		}
		// Player Events
		public void InvokePlayerAction()
		{
			OnPlayerAction?.Invoke();
		}

		public void InvokePlayerDamaged(int damage)
		{
			OnPlayerDamaged?.Invoke(damage);
		}

		public void InvokePlayerHealed(int hp)
		{
			OnPlayerHealed?.Invoke(hp);
		}

		public void InvokePlayerGoalReached()
		{
			OnPlayerGoalReached?.Invoke();
		}

		public void InvokePlayerDeath()
		{
			OnPlayerDeath?.Invoke();
		}

		#endregion

		public void InvokeItemSelect(IItem item)
		{
			OnItemSelect?.Invoke(item);
		}
	}
}