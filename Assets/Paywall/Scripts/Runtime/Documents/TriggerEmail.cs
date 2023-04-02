using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall.Documents {

    public class TriggerEmail : MonoBehaviour {
        /// The email to add
        [Tooltip("The email to add")]
        [field: SerializeField] public EmailItem emailItem { get; protected set; }

        protected const string _playerID = "Player1";

        /// <summary>
        /// Triggers EmailEvent.Pick
        /// </summary>
        public virtual void PickEmail() {
            EmailEvent.Trigger(EmailEventType.Pick, emailItem, "");
        }

    }
}
