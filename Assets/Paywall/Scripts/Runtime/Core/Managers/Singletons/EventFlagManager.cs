using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Manages event flags.
    /// </summary>
    [Serializable]
    public class EventFlagManager {
        /// Has the depot been entered at least once?
        [field: Tooltip("Has the depot been entered at least once?")]
        [field: SerializeField] public bool EnterDepotFirstTime { get; set; }

        public virtual void PopulateDictionary() {

        }

        public virtual void SetDictionary() {

        }
    }
}
