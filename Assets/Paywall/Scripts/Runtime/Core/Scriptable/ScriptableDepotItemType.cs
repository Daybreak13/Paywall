using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "DepotItem", menuName = "Paywall/Depot/DepotItem", order = 1)]
    [Serializable]
    public class ScriptableDepotItemType : ScriptableObject {
        /// The type of this item
        [field: Tooltip("The type of this item")]
        [field: SerializeField] public DepotItemTypes DepotItemType { get; protected set; }
    }

}