using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using Paywall.Documents;

namespace Paywall {

    /// <summary>
    /// Triggers email item pick on start
    /// </summary>
    public class AddEmailItemTrigger : MonoBehaviour {
        /// Email item to pick
        [field: Tooltip("Email item to pick")]
        [field: SerializeField] public EmailItemScriptable Email { get; protected set; }

        protected virtual void Start() {
            EmailEvent.Trigger(EmailEventType.Pick, null, string.Empty, null, Email);
        }
    }
}
