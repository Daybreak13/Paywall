using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paywall
{

    /// <summary>
    /// Stores specific upgrade info on the upgrade's button gameobject
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UpgradeButton : MonoBehaviour, ISelectHandler
    {

        /// The scriptable upgrade object associated with this button
        [field: Tooltip("The scriptable upgrade object associated with this button")]
        [field: SerializeField] public ScriptableUpgrade Upgrade { get; protected set; }

        /// <summary>
        /// Is the upgrade associated with this button unlocked
        /// </summary>
        public bool Unlocked { get; protected set; }

        protected bool _visible;
        protected TextMeshProUGUI _textMesh;

        /// <summary>
        /// If the OnClick event hasn't been assigned, assign it
        /// </summary>
        protected virtual void Awake()
        {
            if (GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
            {
                GetComponent<Button>().onClick.AddListener(OnClick);
            }
            _textMesh = GetComponentInChildren<TextMeshProUGUI>();
            _textMesh.text = Upgrade.UpgradeName;
        }

        /// <summary>
        /// OnClick event for upgrade buttons. Fires UI Click event, which is caught by the StoreMenuManager
        /// </summary>
        protected virtual void OnClick()
        {
            PWUIEvent.Trigger(this.gameObject, UIEventTypes.Click);
            //TryUnlockUpgrade();   // TryUnlockUpgrade() is instead called by StoreMenuManager
        }

        /// <summary>
        /// OnClick event for upgrade buttons. Fires UI Select event, which is caught by the StoreMenuManager
        /// </summary>
        public virtual void OnSelect(BaseEventData eventData)
        {
            PWUIEvent.Trigger(this.gameObject, UIEventTypes.Select);
        }

        /// <summary>
        /// Set the interactable bool of the button, as well as Unlocked state
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetButtonStatus(bool active)
        {
            Unlocked = active;
            GetComponent<Button>().interactable = active;
        }

        /// <summary>
        /// Sets the button's unlock status to true and updates the button display
        /// </summary>
        public virtual void SetAsUnlocked()
        {
            Unlocked = true;
            SetButtonStatus(false);
        }

        /// <summary>
        /// Tries to unlock the button's associated upgrade and triggers an event
        /// Called by the StoreMenuManager after catching the OnClick events fired by this button
        /// TryUnlock event is caught by PaywallProgressManager
        /// </summary>
        public virtual void TryUnlockUpgrade()
        {
            if (Unlocked)
            {
                return;
            }
            PaywallUpgradeEvent.Trigger(UpgradeMethods.TryUnlock, Upgrade, this);
        }

        /// <summary>
        /// Remove listeners on destroy
        /// </summary>
        protected virtual void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

}
