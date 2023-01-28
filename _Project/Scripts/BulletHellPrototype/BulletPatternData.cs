using UnityEngine;

namespace DungeonAutomata._Project.Scripts.BulletHellPrototype
{
	[CreateAssetMenu(fileName = "BulletHell", menuName = "Bullet Pattern", order = 0)]
	public class BulletPatternData : ScriptableObject
	{
		public Bullet bulletPrefab;
		public int bulletCount;
		public float bSpeed;
		public float bDamage;
		public float bLifeTime;
		public float bSpawnDelay;
		public float bSpawnRadius;
		public float bSpawnAngle;
		public float bSpawnAngleOffset;
		public float bSpawnAngleIncrement;
	}
}