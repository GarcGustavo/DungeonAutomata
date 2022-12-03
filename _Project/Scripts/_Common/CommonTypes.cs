namespace DungeonAutomata._Project.Scripts._Common
{
	public static class CommonTypes
	{
		public enum Stat
		{
			Health,
			Mana,
			Hunger,
			Thirst,
			Stamina,
			Strength, 
			Dexterity, 
			Intelligence, 
			Wisdom, 
			Charisma
		}

		public enum GameState
		{
			PlayerTurn,
			EnemyTurn,
			Lose,
			Win,
			Menu
		}
	}
}