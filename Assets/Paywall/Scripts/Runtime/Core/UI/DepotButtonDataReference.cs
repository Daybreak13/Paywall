using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paywall {

    public enum ButtonSelectType { Select, Click }

    /// <summary>
    /// Button data reference specifically for depot item buttons (modules and shop options)
    /// </summary>
    public class DepotButtonDataReference : ButtonDataReference {
        /// The item SO corresponding to this button
        [field: Tooltip("The item SO corresponding to this button")]
        [field: SerializeField] public BaseScriptableDepotItem DepotItem { get; protected set; }
        /// Show description and select button in SupplyDepotManager on select or on click event
        [field: Tooltip("Show description and select button in SupplyDepotManager on select or on click event")]
        [field: SerializeField] public ButtonSelectType SelectType { get; protected set; }

        /// <summary>
        /// Sets the item corresponding to this button
        /// </summary>
        /// <param name="item"></param>
        public virtual void SetItem(BaseScriptableDepotItem item) {
            DepotItem = item;
        }

        /// <summary>
        /// Sets the outline visibility
        /// </summary>
        /// <param name="on"></param>
        public virtual void SetOutline(bool on) {
            OuterImageComponent.enabled = on;
        }

        /// <summary>
        /// Assign to OnClick event
        /// </summary>
        public virtual void OnClick() {
            if (DepotItem != null && SelectType == ButtonSelectType.Click)
                SupplyDepotMenuManager.Instance.SetBuyOption(DepotItem, this);
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            if (DepotItem != null && SelectType == ButtonSelectType.Select)
                SupplyDepotMenuManager.Instance.SetBuyOption(DepotItem, this);
        }
    }
}
