using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall.Documents {

    public static class StaticEmailDictionary {
        public static Dictionary<string, EmailItem> EmailItems { get; private set; } = new Dictionary<string, EmailItem>();
        public static int i;

        public static void SetDictionary(Dictionary<string, EmailItem> dict) {
            EmailItems = dict;
        }

        public static void Add(string key, EmailItem value) {
            EmailItems.Add(key, value);
        }

        public static void Remove(string key) {
            EmailItems.Remove(key);
        }

        public static bool ContainsKey(string key) {
            if (EmailItems.ContainsKey(key)) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
