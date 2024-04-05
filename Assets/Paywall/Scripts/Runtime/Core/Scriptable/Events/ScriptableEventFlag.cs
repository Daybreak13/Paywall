using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

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

        /// <summary>
        /// Sets the event flag
        /// </summary>
        /// <param name="flag"></param>
        public virtual void SetFlag(bool flag) {
            Triggered = flag;
        }
    }
}
