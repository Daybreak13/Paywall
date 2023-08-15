using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public enum EventFlagNames {
        // Chapter 1
        EnterDepotFirstTime
    }

    [Serializable]
    public class EventFlag {
        public EventFlagNames EventFlagName;
        public bool Completed;
    }

    /// <summary>
    /// Manages event flags.
    /// </summary>
    [Serializable]
    public class EventFlagManager {
        /// Has the depot been entered at least once?
        [field: Tooltip("Has the depot been entered at least once?")]
        [field: SerializeField] public bool EnterDepotFirstTime { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        [field: SerializeField] public List<EventFlag> EventFlagsList { get; protected set; }
        public Dictionary<EventFlagNames, bool> EventFlagsDict { get; protected set; }

        public virtual void PopulateDictionary() {

        }

        public virtual void SetDictionary() {

        }
    }
}
