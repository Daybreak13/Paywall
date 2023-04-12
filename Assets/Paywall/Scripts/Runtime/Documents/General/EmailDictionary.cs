using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall.Documents {

    /// <summary>
    /// Scriptable object for exchanging the email dictionary between different components
    /// </summary>
    [CreateAssetMenu(fileName = "EmailDictionary", menuName = "Paywall/Emails/Root/EmailDictionary", order = 1)]
    public class EmailDictionary : ScriptableObject {
        /// The email dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public Dictionary<string, EmailItem> EmailItems { get; protected set; } = new Dictionary<string, EmailItem>();

        public virtual void SetDictionary(Dictionary<string, EmailItem> dict) {
            EmailItems = dict;
        }

        public virtual void Add(string key, EmailItem value) {
            EmailItems.Add(key, value);
        }

        public virtual void Remove(string key) {
            EmailItems.Remove(key);
        }

        public virtual bool ContainsKey(string key) {
            if (EmailItems.ContainsKey(key)) {
                return true;
            } else {
                return false;
            }
        }
    }
}
