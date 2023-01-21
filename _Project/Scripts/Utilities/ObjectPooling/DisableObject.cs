using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities.ObjectPooling
{
	public class DisableObject : MonoBehaviour
	{
		private void OnTriggerEnter(Collider collider)
		{
			collider.gameObject.SetActive(false);
		}
	}
}