using DungeonAutomata._Project.Scripts.Utilities.ObjectPooling;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.BulletHellPrototype.Interfaces
{
	public interface IBullet : IPoolObject<IBullet>
	{
		public void InitializeBullet(Vector3 direction, float speed, float damage);
		public void DamageTarget(IBUnit unit, float damage);
		public void DisableBullet();
	}
}
