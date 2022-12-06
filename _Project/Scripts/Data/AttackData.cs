using MoreMountains.Feedbacks;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Data
{
	[CreateAssetMenu(fileName = "AttackData", menuName = "AttackData", order = 0)]
	public class AttackData : ScriptableObject
	{
		public Sprite sprite;
		public MMFeedback attackFeedback;
		public float damage;
		public float attackSpeed;
		public float attackRange;
		public float attackAngle;
		public float attackDuration;
		public float attackCooldown;
		public float attackDelay;
		public float attackKnockback;
		public float attackKnockbackDuration;
	}
}