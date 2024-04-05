using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// List of objects kept at runtime
    /// Single scene data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RuntimeSet<T> : ScriptableObject {
        /// List of items belonging to this set
        [field: Tooltip("List of items belonging to this set")]
        [field: SerializeField] public List<T> Items { get; protected set; } = new();

        public void Add(T item) {
            if (!Items.Contains(item)) Items.Add(item);
        }
        public void Remove(T item) {
            if (Items.Contains(item)) Items.Remove(item);
        }
    }
}
