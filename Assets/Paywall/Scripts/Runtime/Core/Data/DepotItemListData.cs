using System;
using UnityEngine;

namespace Paywall
{

    [Serializable]
    public class DepotItemListData
    {
        /// Name to display on the button for this option
        [field: Tooltip("Name to display on the button for this option")]
        [field: SerializeField] public string ID { get; set; }
        /// Can this item show up in the store?
        [field: Tooltip("Can this item show up in the store?")]
        [field: SerializeField] public bool Active { get; set; } = true;

        [field: NonSerialized]
        public DepotItemList ItemList { get; set; }

        public DepotItemListData(string id, bool active, DepotItemList itemList)
        {
            ID = id;
            Active = active;
            ItemList = itemList;
        }
    }
}
