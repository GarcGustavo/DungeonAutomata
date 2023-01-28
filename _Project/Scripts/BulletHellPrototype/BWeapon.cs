using System.Collections;
using System.Collections.Generic;
using DungeonAutomata._Project.Scripts.BulletHellPrototype.Interfaces;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.BulletHellPrototype
{
	public class BWeapon : MonoBehaviour
	{
		private BulletPool<Bullet> _bulletPool;
		[SerializeField] private BulletPatternData bData;
		
		private void Awake()
		{
			_bulletPool = BulletPool<Bullet>.Instance;
		}
		
		public IEnumerator FireWeapon()
		{
			//TODO: Attack cd
			//while(canSpawn)
			//{
			//	Spawn();
			//	yield return null;
			//}
			yield return null;
		}

		public void Spawn()
		{
			//TODO: Rework example implementation
			//int random = Random.Range(0, 3);
			//Vector3 position = Random.insideUnitSphere * range + transform.position;
			//GameObject prefab;
			//prefab = cubePool.PullGameObject(position, Random.rotation);
			//prefab = spherePool.PullGameObject(position, Random.rotation);
			//prefab = capsulePool.PullGameObject(position, Random.rotation);
			//prefab = cubePool.PullGameObject(position, Random.rotation);
			//prefab.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		
		
	}
}
