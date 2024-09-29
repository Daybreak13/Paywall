using System.Collections.Generic;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// List SO that stores immutable list to be referenced
    /// </summary>
    public abstract class ScriptableList<T> : ScriptableObject
    {
        /// List of items belonging to this list
        [field: Tooltip("List of items belonging to this list")]
        [field: SerializeField] public List<T> Items { get; protected set; } = new();

        public void Add(T item)
        {
            if (!Items.Contains(item)) Items.Add(item);
        }
        public void Remove(T item)
        {
            if (Items.Contains(item)) Items.Remove(item);
        }
    }
}
