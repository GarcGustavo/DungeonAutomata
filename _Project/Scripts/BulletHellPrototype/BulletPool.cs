using System;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.Utilities.ObjectPooling;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.BulletHellPrototype
{
	public class BulletPool<T> : MonoBehaviour, IPool<T> where T : MonoBehaviour, IPoolObject<T>
	{
		public static BulletPool<T> Instance { get; private set; }

		private Action<T> _pullObject;
		private Action<T> _pushObject;
		private Stack<T> _pooledObjects = new Stack<T>();
		private GameObject _prefab;

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public T Pull()
		{
			T t;
			var pooledCount = _pooledObjects.Count;
			if (pooledCount > 0)
				t = _pooledObjects.Pop();
			else
				t = GameObject.Instantiate(_prefab).GetComponent<T>();

			t.gameObject.SetActive(true);
			t.Initialize(Push);

			//allow default behavior and turning object back on
			_pullObject?.Invoke(t);

			return t;
		}

		public void Push(T t)
		{
			throw new NotImplementedException();
		}

		public void CreateBullet(Vector3 direction, float speed, float damage)
		{
			throw new System.NotImplementedException();
		}
		
		public void PoolBullet()
		{
			throw new System.NotImplementedException();
		}

		public void DestroyBullet()
		{
			throw new System.NotImplementedException();
		}
	}
}