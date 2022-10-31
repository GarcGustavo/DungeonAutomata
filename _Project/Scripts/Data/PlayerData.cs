using DungeonAutomata._Project.Scripts._Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Data
{
	[ManageableData]
	[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "PlayerData", order = 0)]
	public class PlayerData : ScriptableObject
	{
		[PreviewField(75, ObjectFieldAlignment.Left)]
		public Sprite sprite;
		public string playerName;
		public int Health;
		public int Mp;
		public int Energy;
		public int Level;
		public int Exp;
		public int Strength;
		public int Dexterity;
		public int Intelligence;
		public int Luck;
	}
}