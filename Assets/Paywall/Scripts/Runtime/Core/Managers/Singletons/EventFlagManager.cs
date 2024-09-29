using System;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Manages event flags. Serializable so it can be saved by PaywallProgressManager
    /// </summary>
    [Serializable]
    public class EventFlagManager
    {
        /// Has the depot been entered at least once?
        [field: Tooltip("Has the depot been entered at least once?")]
        [field: SerializeField] public bool EnterDepotFirstTime { get; set; }

    }
}
