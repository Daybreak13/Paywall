using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Scriptable object replacing enum for spawn types
    /// Represents a weighted pooler that holds a specific set of spawnables
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnType", menuName = "Paywall/Procedural/SpawnType")]
    public class ScriptableSpawnType : ScriptableObject
    {
        /// ID of this spawnable type
        [Tooltip("ID of this spawnable type")]
        [field: SerializeField] public string ID { get; protected set; }
    }
}
