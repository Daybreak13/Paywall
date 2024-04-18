using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Scriptable object replacing enum for spawn types
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnType", menuName = "Paywall/Procedural/SpawnType")]
    public class ScriptableSpawnType : ScriptableObject {
        /// ID of this spawnable type
        [Tooltip("ID of this spawnable type")]
        [field: SerializeField] public string ID { get; protected set; }
    }
}
