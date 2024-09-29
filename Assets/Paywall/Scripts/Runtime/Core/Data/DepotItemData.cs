using System;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Serializable class to hold mutable depot item data during runtime
    /// Used by progress manager
    /// </summary>
    [Serializable]
    public class DepotItemData
    {
        /// Name to display on the button for this option
        [field: Tooltip("Name to display on the button for this option")]
        [field: SerializeField] public string Name { get; set; }
        /// Can this item show up in the store?
        [field: Tooltip("Can this item show up in the store?")]
        [field: SerializeField] public bool IsValid { get; set; } = true;

        [field: NonSerialized]
        public BaseScriptableDepotItem DepotItem { get; set; }

        public DepotItemData(string name, bool isValid, BaseScriptableDepotItem depotItem)
        {
            Name = name; IsValid = isValid;
            DepotItem = depotItem;
        }
    }
}
