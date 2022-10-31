using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Automata
{
    public class ParticleAgent : MonoBehaviour
    {
        Vector2Int Position { get; set; }
        Color Color { get; set; }
        bool IsAlive { get; set; }
        
        public void Initialize(Vector2Int position, Color color)
        {
            Position = position;
            Color = color;
            IsAlive = true;
        }
        
    }
}
