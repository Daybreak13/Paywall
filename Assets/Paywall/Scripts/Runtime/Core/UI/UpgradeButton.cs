using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Paywall {

    [RequireComponent(typeof(Button))]
    public class UpgradeButton : MonoBehaviour {

        /// The scriptable upgrade object associated with this button
        [field: Tooltip("The scriptable upgrade object associated with this button")]
        [field:SerializeField] public ScriptableUpgrade Upgrade { get; protected set; }

        /// <summary>
        /// Unlocks the button's associated upgrade and triggers an event
        /// </summary>
        public virtual void UnlockUpgrade() {
            if (PaywallProgressManager.HasInstance) {
                if (PaywallProgressManager.Instance.Credits >= Upgrade.Cost) {
                    Upgrade.UnlockUpgrade();
                    PaywallUpgradeEvent.Trigger(UpgradeMethods.Unlock, Upgrade);
                } else {
                    if (StoreMenuManager.HasInstance) {
                        StoreMenuManager.Instance.TriggerErrorMessage();
                    }
                }
            }
            
        }

    }

}
