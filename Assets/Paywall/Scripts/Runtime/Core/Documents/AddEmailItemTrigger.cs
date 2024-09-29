using Paywall.Documents;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Triggers email item pick on start
    /// </summary>
    public class AddEmailItemTrigger : MonoBehaviour
    {
        /// Email item to pick
        [field: Tooltip("Email item to pick")]
        [field: SerializeField] public EmailItemScriptable Email { get; protected set; }

        protected virtual void Start()
        {
            EmailEvent.Trigger(EmailEventType.Pick, null, string.Empty, null, Email);
        }
    }
}
