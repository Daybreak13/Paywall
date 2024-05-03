using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [System.Serializable]
    public class EventFlag {
        [field: SerializeField] public string ID { get; protected set; }
        [field: SerializeField] public bool Triggered { get; protected set; }
        public EventFlag(string id, bool triggered) {
            ID = id;
            Triggered = triggered;
        }
        public void SetEventTriggered(bool triggered) {
            Triggered |= triggered;
        }
    }

    /// <summary>
    /// Scriptable event flag. Used to initialize list of EventFlag objects in PaywallProgressManager save
    /// </summary>
    [CreateAssetMenu(fileName = "EventFlag", menuName = "Paywall/Progress/EventFlag")]
    public class ScriptableEventFlag : ScriptableObject {
        /// Name of this event
        [field: Tooltip("Name of this event")]
        [field: SerializeField] public string EventID { get; protected set; }
        /// Has this event been triggered?
        [field: Tooltip("Has this event been triggered?")]
        [field: SerializeField] public bool Triggered { get; protected set; }
        /// Event description
        [field: Tooltip("Event description")]
        [field: TextArea]
        [field: SerializeField] public string Description { get; protected set; }
    }
}
