using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;
using UnityEngine.EventSystems;

namespace Paywall {

    /// <summary>
    /// Button select behavior for depot buttons
    /// </summary>
    public class DepotButtonSelect : ButtonSelectController {
        /// The item type corresponding to this button
        [field: Tooltip("The item type corresponding to this button")]
        [field: SerializeField] public DepotItemTypes DepotItemType { get; protected set; }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            SupplyDepotMenuManager.Instance.SetDescription(DepotItemType);
        }
    }
}
