using Paywall.Tools;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Add to object to allow it to oscillate up and down
    /// </summary>
    public class Oscillator : MonoBehaviour_PW {
        /// Auto calculate oscillation speed based on level speed
        [field: Tooltip("Auto calculate oscillation speed based on level speed")]
        [field: SerializeField] public bool UseLevelSpeed { get; protected set; } = true;
        /// Speed at which the object moves
        [field: Tooltip("Speed at which the object moves")]
        [field: FieldCondition("UseLevelSpeed", true, true)]
        [field: SerializeField] public float OscillationSpeed { get; protected set; }


    }
}
