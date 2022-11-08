using System.Collections;

//using Cysharp.Threading.Tasks;

namespace DungeonAutomata._Project.Scripts._Interfaces
{
	public interface ICommand
	{
		float Duration { get; set; }
		IEnumerator Execute();
	}
}