using UnityEngine;

//using Cysharp.Threading.Tasks;

namespace DungeonAutomata._Project.Scripts.CommandSystem
{
	[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CombatUnitData", order = 0)]
	public class CombatUnitData : ScriptableObject
	{
		public int hp = 100;
		public int mp = 100;
		private CombatUnitData _unit;
	}
}