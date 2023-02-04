using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonAutomata._Project.Scripts._Managers
{
	[CreateAssetMenu(fileName = "Dialogue System", menuName = "Dialogue Data", order = 0)]
	public class DialogueData : ScriptableObject
	{
		public string[] dialogue;
		public Sprite[] sprites;
	}
}