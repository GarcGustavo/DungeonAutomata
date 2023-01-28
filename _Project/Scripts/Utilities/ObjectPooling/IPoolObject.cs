using System;

namespace DungeonAutomata._Project.Scripts.Utilities.ObjectPooling
{

	public interface IPoolObject<T>
	{
		void Initialize(Action<T> returnAction);
		void ReturnToPool();
	}
}