using Unity.Netcode;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities
{
	public class NetworkButtons : MonoBehaviour
	{
		private void OnGUI() {
			GUILayout.BeginArea(new Rect(20, 20, 300, 600));
			if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
				if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
				if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
				if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
			}

			GUILayout.EndArea();
		}

		// private void Awake() {
		//     GetComponent<UnityTransport>().SetDebugSimulatorParameters(
		//         packetDelay: 120,
		//         packetJitter: 5,
		//         dropRate: 3);
		// }
	}
}