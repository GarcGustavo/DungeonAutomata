namespace DungeonAutomata._Project.Scripts.Utilities.ObjectPooling
{
	public interface IPool<T>
	{
		T Pull();
		void Push(T t);
	}
}