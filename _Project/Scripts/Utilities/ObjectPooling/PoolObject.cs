using System;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities.ObjectPooling
{
	public class PoolObject : MonoBehaviour, IPoolObject<PoolObject>
	{
		private Action<PoolObject> returnToPool;

		private void OnDisable()
		{
			ReturnToPool();
		}

		public void Initialize(Action<PoolObject> returnAction)
		{
			//cache reference to return action
			returnToPool = returnAction;
		}

		public void ReturnToPool()
		{
			//invoke and return this object to pool
			returnToPool?.Invoke(this); 
		}
	}
}