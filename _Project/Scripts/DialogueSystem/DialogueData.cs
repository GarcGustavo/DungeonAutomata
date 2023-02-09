using UnityEngine;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
	[CreateAssetMenu(fileName = "Dialogue System", menuName = "Dialogue Data", order = 0)]
	public class DialogueData : ScriptableObject
	{
		public string[] dialogue;
		public Sprite[] sprites;
	}
}