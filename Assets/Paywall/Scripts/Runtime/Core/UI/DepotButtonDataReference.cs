using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paywall {

    /// <summary>
    /// Button data reference specifically for depot item buttons (modules and shop options)
    /// </summary>
    public class DepotButtonDataReference : ButtonDataReference {
        /// The item SO corresponding to this button
        [field: Tooltip("The item SO corresponding to this button")]
        [field: SerializeField] public BaseScriptableDepotItem DepotItem { get; protected set; }

        /// <summary>
        /// Sets the item corresponding to this button
        /// </summary>
        /// <param name="item"></param>
        public virtual void SetItem(BaseScriptableDepotItem item) {
            DepotItem = item;
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            SupplyDepotMenuManager.Instance.SetBuyOption(DepotItem);
        }
    }
}
