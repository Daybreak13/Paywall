using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Paywall {

    /// <summary>
    /// Base class for depot item SOs
    /// Includes modules, power-up options
    /// </summary>
    [CreateAssetMenu(fileName = "BaseDepotItem", menuName = "Paywall/Depot/BaseDepotItem")]
    public class BaseScriptableDepotItem : ScriptableObject {
        /// Name to display on the button for this option
        [field: Tooltip("Name to display on the button for this option")]
        [field: SerializeField] public string Name { get; protected set; }
        /// Description to display when this option is selected/highlighted
        [field: Tooltip("Description to display when this option is selected/highlighted")]
        [field: TextArea]
        [field: SerializeField] public string Description { get; protected set; }
        /// Image representing this item
        [field: Tooltip("Image representing this item")]
        [field: SerializeField] public Sprite UISprite { get; protected set; }
        /// The shop cost of this item
        [field: Tooltip("The shop cost of this item")]
        [field: SerializeField] public int Cost { get; protected set; }
        /// Can this item show up in the store?
        [field: Tooltip("Can this item show up in the store?")]
        [field: SerializeField] public bool IsValid { get; protected set; } = true;
        /// The type of depot item this is
        [field: Tooltip("The type of depot item this is")]
        [field: SerializeField] public DepotItemTypes DepotItemType { get; protected set; }
    }
}
