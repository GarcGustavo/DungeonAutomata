using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonTypes;

namespace DungeonAutomata._Project.Scripts._Common
{
	/// <summary>
	/// Create a new scriptable object to contain different stats for different units
	/// </summary>
	[CreateAssetMenu(menuName = "Stats Data")]
	public class StatsData : SerializedScriptableObject
	{
		public Dictionary<Stat, float> Stats = new Dictionary<Stat, float>();
		
		/// <summary>
		/// Used to handle requests for stats not in the dictionary
		/// </summary>
		/// <param name="statType"></param>
		/// <returns>value</returns>
		public float GetStat(Stat statType)
		{
			if(Stats.TryGetValue(statType, out float value))
			{
				return value;
			}
			else
			{
				Debug.LogError($"Stat {statType} not found in {name}");
				return 0;
			}
		}
	}
}