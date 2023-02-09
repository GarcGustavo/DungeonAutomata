namespace DungeonAutomata._Project.Scripts.DialogueSystem.Interfaces
{
	public interface IPanel
	{
		public void OpenPanel(string dialogueString);
		public void OpenPanel(DialogueData dialogueData);
	}
}