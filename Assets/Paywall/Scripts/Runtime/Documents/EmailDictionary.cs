using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall.Documents {

    public class EmailDictionary : ScriptableObject {
        /// The email dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public Dictionary<string, EmailItem> EmailDict = new Dictionary<string, EmailItem>();

        public virtual void SetDictionary(Dictionary<string, EmailItem> dict) {
            EmailDict = dict;
        }

        public virtual void Add(string key, EmailItem value) {
            EmailDict.Add(key, value);
        }

        public virtual void Remove(string key) {
            EmailDict.Remove(key);
        }
    }
}
