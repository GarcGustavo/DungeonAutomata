using DungeonAutomata._Project.Scripts._Managers;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.GridComponents
{
	public class Goal : MonoBehaviour
	{
		private EventManager _eventManager;
		private GameManager _gameManager;
		private Vector3Int _gridPosition;
		public Vector3Int CurrentPosition { get; set; }

		private void Awake()
		{
			_gameManager = GameManager.Instance;
			_eventManager = EventManager.Instance;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.CompareTag("Player")) _eventManager.InvokePlayerExit();
		}
	}
}