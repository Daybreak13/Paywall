using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Module that increases reload speed
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedReloaderModule", menuName = "Paywall/Modules/SpeedReloaderModule")]
    public class SpeedReloaderModule : ScriptableModule
    {
        /// Reduction in reload time (increase rate)
        [field: Tooltip("Reduction in reload time (increase rate)")]
        [field: SerializeField] public float ReloadTimeReduction { get; protected set; }
        /// Reload delay reduction (time before reload starts)
        [field: Tooltip("Reload delay reduction (time before reload starts)")]
        [field: SerializeField] public float ReloadDelayReduction { get; protected set; }
    }
}
