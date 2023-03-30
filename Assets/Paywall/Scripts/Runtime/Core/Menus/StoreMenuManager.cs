using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.Tools;

namespace Paywall {

    /// <summary>
    /// Manages the store menu
    /// </summary>
    public class StoreMenuManager : MMSingleton<StoreMenuManager> {

        /// The money counter
        [Tooltip("The money counter")]
        [field: SerializeField] public TextMeshProUGUI MoneyCounter { get; protected set; }

        [field:Header("Error Message")]

        /// The error message container (if there is insufficient funds to purchase upgrade)
        [Tooltip("The error message container (if there is insufficient funds to purchase upgrade)")]
        [field: SerializeField] public GameObject ErrorMessage { get; protected set; }
        [field: SerializeField] public float ErrorMessageDuration { get; protected set; } = 2f;

        [field:Header("Lists")]

        /// List of store menus
        [Tooltip("List of store menus")]
        [field:SerializeField] public List<GameObject> Menus { get; private set; }
        /// List of possible upgrades. Needs to be set via the Inspector.
        [Tooltip("List of possible upgrades. Needs to be set via the Inspector.")]
        [field: SerializeField] public List<ScriptableUpgrade> Upgrades { get; private set; }

        protected Dictionary<string, ScriptableUpgrade> UpgradesDict;

        protected int _credits;
        protected int _index = 0;

        protected virtual void Start() {
            if (PaywallProgressManager.HasInstance) {
                _credits = PaywallProgressManager.Instance.Credits;
            }
            if (ErrorMessage != null) {
                ErrorMessage.SetActive(false);
            }
        }

        protected virtual void Update() {
            if (PaywallProgressManager.HasInstance && (MoneyCounter != null)) {
                MoneyCounter.text = PaywallProgressManager.Instance.Credits.ToString();
            }            
        }

        public virtual void TriggerErrorMessage() {
            if (ErrorMessage != null) {
                StartCoroutine(ErrorMessageFader());
            }
        }

        protected virtual IEnumerator ErrorMessageFader() {
            ErrorMessage.SetActive(true);
            yield return new WaitForSeconds(ErrorMessageDuration);
            ErrorMessage.SetActive(false);
        }

        /// <summary>
        /// Used by editor
        /// </summary>
        /// <param name="upgrades"></param>
        public virtual void SetUpgrades(List<ScriptableUpgrade> upgrades) {
            Upgrades = upgrades;
        }

        protected virtual void OpenMenu(int idx) {
            int i = 0;
            foreach (GameObject menu in Menus) {
                if (i == idx) {
                    menu.SetActive(true);
                } else {
                    menu.SetActive(false);
                }
                i++;
            }
        }

        /// <summary>
        /// Opens the previous menu in the list
        /// </summary>
        public virtual void PreviousMenu() {
            if (Menus != null && Menus.Count > 1) {
                if (--_index < 0) {
                    _index = Menus.Count - 1;
                }
                OpenMenu(_index);
            }
        }

        /// <summary>
        /// Opens the next menu in the list
        /// </summary>
        public virtual void NextMenu() {
            if (Menus != null && Menus.Count > 1) {
                if (++_index >= Menus.Count) {
                    _index = 0;
                }
                OpenMenu(_index);
            }
        }

    }

}
