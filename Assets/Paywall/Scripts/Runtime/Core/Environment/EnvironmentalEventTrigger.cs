using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class EnvironmentalEventTrigger : MonoBehaviour {
        /// The name of the event to trigger
        [field: Tooltip("The name of the event to trigger")]
        [field: SerializeField] public string EventName { get; protected set; }

        protected virtual void OnTriggerEnter2D(Collider2D collider2D) {
            if (collider2D.gameObject.CompareTag("Player")) {
                MMGameEvent.Trigger(EventName);
            }
        }
    }
}
