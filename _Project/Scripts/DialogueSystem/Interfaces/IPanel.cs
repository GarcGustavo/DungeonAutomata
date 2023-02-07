using DungeonAutomata._Project.Scripts._Managers;

namespace DungeonAutomata._Project.Scripts.DialogueSystem
{
	public interface IPanel
	{
		public void OpenPanel(string dialogueString);
		public void OpenPanel(DialogueData dialogueData);
	}
}