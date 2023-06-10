using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Paywall {

    /// <summary>
    /// Stores specific upgrade info on the upgrade's button gameobject
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UpgradeButton : MonoBehaviour, ISelectHandler {

        /// The scriptable upgrade object associated with this button
        [field: Tooltip("The scriptable upgrade object associated with this button")]
        [field:SerializeField] public ScriptableUpgrade Upgrade { get; protected set; }
        /// The scriptable upgrade object associated with this button
        [field: Tooltip("The scriptable upgrade object associated with this button")]

        public bool Unlocked { get; protected set; }

        protected bool _visible;

        protected virtual void Awake() {
            if (GetComponent<Button>().onClick.GetPersistentEventCount() == 0) {
                GetComponent<Button>().onClick.AddListener(OnClick);
            }
        }

        protected virtual void OnClick() {
            PWUIEvent.Trigger(this.gameObject, UIEventTypes.Click);
            //TryUnlockUpgrade();
        }

        public virtual void OnSelect(BaseEventData eventData) {
            PWUIEvent.Trigger(this.gameObject, UIEventTypes.Select);
        }

        /// <summary>
        /// Set the interactable bool of the button, as well as Unlocked state
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetButtonStatus(bool active) {
            Unlocked = active;
            GetComponent<Button>().interactable = active;
        }

        /// <summary>
        /// Sets the button's unlock status to true and updates the button display
        /// </summary>
        public virtual void SetAsUnlocked() {
            Unlocked = true;
            SetButtonStatus(false);
        }

        /// <summary>
        /// Tries to unlock the button's associated upgrade and triggers an event
        /// Called by the OnClick event
        /// </summary>
        public virtual void TryUnlockUpgrade() {
            if (Unlocked) {
                return;
            }
            PaywallUpgradeEvent.Trigger(UpgradeMethods.TryUnlock, Upgrade, this);
        }

        protected virtual void OnDestroy() {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

}
