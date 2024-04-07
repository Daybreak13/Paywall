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
        /// The item SO corresponding to this button
        [field: Tooltip("The item SO corresponding to this button")]
        [field: SerializeField] public BaseScriptableDepotItem DepotItem { get; protected set; }

        public virtual void SetItem(BaseScriptableDepotItem item) {
            DepotItem = item;
        }

        /// <summary>
        /// What to do when this button is selected
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            //SupplyDepotMenuManager.Instance.SetBuyOption(DepotItem);
        }
    }
}
