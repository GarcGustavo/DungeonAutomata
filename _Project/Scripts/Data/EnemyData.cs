using DungeonAutomata._Project.Scripts._Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Data
{
	[ManageableData]
	[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "EnemyData", order = 0)]
	public class EnemyData : ScriptableObject
	{
		[PreviewField(75, ObjectFieldAlignment.Left)]
		public Sprite sprite;
		public string enemyName;
		public EnemyType type;
		public string description;

		public int Level;
		public int Exp;
		public int Health;
		public int Mp;
		public int Strength;
		public int Dexterity;
		public int Intelligence;
		public int Luck;
		public int aggroDistance;
	}

	public enum EnemyType
	{
		Brute,
		Sniper,
		Mage,
		Boomer,
		Trap
	}
}