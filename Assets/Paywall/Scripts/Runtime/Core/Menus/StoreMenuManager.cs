using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace Paywall {

    /// <summary>
    /// Manages the store menu
    /// The list of the store's upgrades must be set via inspector button
    /// OnClick and OnSelect events of the buttons are caught by this manager
    /// </summary>
    public class StoreMenuManager : MonoBehaviour, MMEventListener<PaywallUpgradeEvent>, MMEventListener<PWUIEvent>, MMEventListener<PWInputEvent> {

        /// The money counter
        [field: Tooltip("The money counter")]
        [field: SerializeField] public TextMeshProUGUI MoneyCounter { get; protected set; }
        // The upgrade's title
        [field: Tooltip("The upgrade's title")]
        [field: SerializeField] public TextMeshProUGUI Title { get; protected set; }
        /// The upgrade's description
        [field: Tooltip("The upgrade's description")]
        [field: SerializeField] public TextMeshProUGUI Description { get; protected set; }
        // If true, show the upgrade's description on select
        [field: Tooltip("If true, show the upgrade's description on select")]
        [field: SerializeField] public bool ShowOnSelect { get; protected set; }

        [field: Header("Buttons")]

        /// The buy upgrade button
        [field: Tooltip("The buy upgrade button")]
        [field: SerializeField] public GameObject BuyButton { get; protected set; }
        /// The cancel buy button
        [field: Tooltip("The cancel buy button")]
        [field: SerializeField] public GameObject CancelButton { get; protected set; }
        /// The cancel and buy buttons' container
        [field: Tooltip("The cancel and buy buttons' container")]
        [field: SerializeField] public GameObject BuyButtonsContainer { get; protected set; }

        [field: Header("Canvas Groups")]

        /// The canvas group containing the upgrade buttons
        [field: Tooltip("The canvas group containing the upgrade buttons")]
        [field: SerializeField] public CanvasGroup UpgradeButtonsCanvasGroup { get; protected set; }
        /// The top bar canvas group
        [field: Tooltip("The top bar canvas group")]
        [field: SerializeField] public CanvasGroup TopBarCanvasGroup { get; protected set; }

        [field:Header("Error Message")]

        /// The error message container (if there is insufficient funds to purchase upgrade)
        [field: Tooltip("The error message container (if there is insufficient funds to purchase upgrade)")]
        [field: SerializeField] public GameObject ErrorMessage { get; protected set; }
        [field: SerializeField] public float ErrorMessageDuration { get; protected set; } = 2f;

        [field:Header("Lists")]

        /// List of store menus
        [field: Tooltip("List of store menus")]
        [field: SerializeField] public List<GameObject> Menus { get; private set; }
        /// Dictionary of possible upgrades. Needs to be set via the Inspector.
        [field: Tooltip("Dictionary of possible upgrades. Needs to be set via the Inspector.")]
        [field: SerializeField] public List<UpgradeButton> UpgradeButtons { get; private set; } = new();

        protected Dictionary<string, UpgradeButton> _upgradeButtons = new();

        protected int _credits;
        protected int _index = 0;
        protected UpgradeButton _currentButton;
        protected bool _buyButtonsActive;
        protected bool _awake;
        protected Coroutine _errorCoroutine;

        protected bool UsingBuyButtons {
            get {
                return ((BuyButton != null) && (CancelButton != null));
            }
        }

        protected virtual void Awake() {
            PopulateDictionary();
            ToggleBuyButtons(false);
            _awake = true;
        }

        protected virtual void Start() {
            if (PaywallProgressManager.HasInstance) {
                _credits = PaywallProgressManager.Instance.Credits;
                UpdateUpgradeButtons();
            }

            if (ErrorMessage != null) {
                ErrorMessage.SetActive(false);
            }
        }

        /// <summary>
        /// On start, update the store's upgrade buttons with their current unlock status based on the progress manager's save file
        /// </summary>
        protected virtual void UpdateUpgradeButtons() {
            foreach (KeyValuePair<string, Upgrade> entry in PaywallProgressManager.Instance.Upgrades) {
                if (_upgradeButtons.TryGetValue(entry.Key, out UpgradeButton button)) {
                    if (entry.Value.Unlocked) {
                        button.SetAsUnlocked();
                    }
                }
            }
        }

        /// <summary>
        /// Update credit display
        /// </summary>
        protected virtual void Update() {
            if (PaywallProgressManager.HasInstance && (MoneyCounter != null)) {
                MoneyCounter.text = PaywallProgressManager.Instance.Credits.ToString();
            }            
        }

        /// <summary>
        /// Populates upgrade button dictionary using the UpgradeButtons serialized list
        /// </summary>
        protected virtual void PopulateDictionary() {
            foreach (UpgradeButton button in UpgradeButtons) {
                _upgradeButtons.Add(button.Upgrade.UpgradeID, button);
            }
        }

        /// <summary>
        /// Triggers error message coroutine
        /// </summary>
        public virtual void TriggerErrorMessage() {
            if (ErrorMessage != null) {
                if (_errorCoroutine != null) {
                    StopCoroutine(_errorCoroutine);
                }
                _errorCoroutine = StartCoroutine(ErrorMessageFader());
            }
        }
        
        /// <summary>
        /// Fades error message
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ErrorMessageFader() {
            ErrorMessage.SetActive(true);
            yield return new WaitForSeconds(ErrorMessageDuration);
            ErrorMessage.SetActive(false);
            _errorCoroutine = null;
        }

        /// <summary>
        /// Loads upgrades from save data to the StoreMenuManager and updates the display accordingly
        /// Called by PaywallProgressManager OnSceneLoaded
        /// </summary>
        /// <param name="upgrades"></param>
        public virtual void LoadUpgrades(Dictionary<string, Upgrade> upgrades) {
            if (upgrades == null) {
                return;
            }
            foreach (KeyValuePair<string, Upgrade> entry in upgrades) {
                if (_upgradeButtons.ContainsKey(entry.Value.UpgradeID)) {
                    _upgradeButtons[entry.Value.UpgradeID].SetButtonStatus(entry.Value.Unlocked);
                }
            }
        }

        #region UI

        /// <summary>
        /// Opens the menu at the specified index
        /// </summary>
        /// <param name="idx"></param>
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
            MMGameEvent.Trigger("MenuChange");
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

        /// <summary>
        /// Updates the details display with the info from the given upgrade
        /// </summary>
        public virtual void UpdateDetailsDisplay() {
            ScriptableUpgrade upgrade = _currentButton.Upgrade;
            Title.text = upgrade.UpgradeName;
            Description.text = upgrade.UpgradeDescription;
        }

        /// <summary>
        /// Sets the active state of BuyButton and CancelButton
        /// </summary>
        /// <param name="active"></param>
        public virtual void ToggleBuyButtons(bool active) {
            if (UsingBuyButtons) {
                BuyButtonsContainer.SetActive(active);
                BuyButton.SetActive(active);
                CancelButton.SetActive(active);
                _buyButtonsActive = active;

                UpgradeButtonsCanvasGroup.interactable = !active;
                TopBarCanvasGroup.interactable = !active;
                if (active) {
                    EventSystem.current.SetSelectedGameObject(BuyButton);
                } 
                else if (_awake) {
                    EventSystem.current.SetSelectedGameObject(_currentButton.gameObject);
                }
            }
        }

        /// <summary>
        /// Called by BuyButton's OnClick event
        /// Tries to unlock the currently selected upgrade
        /// </summary>
        public virtual void ClickBuyButton() {
            _currentButton.TryUnlockUpgrade();
        }

        /// <summary>
        /// Cancel the buy display for currently selected upgrade
        /// </summary>
        public virtual void ClickCancelButton() {
            ToggleBuyButtons(false);
        }

        #endregion

        /// <summary>
        /// Catch upgrade events
        /// Set the button to unlocked, or throw error message
        /// </summary>
        /// <param name="upgradeEvent"></param>
        public virtual void OnMMEvent(PaywallUpgradeEvent upgradeEvent) {
            if (upgradeEvent.UpgradeMethod == UpgradeMethods.Unlock) {
                //upgradeEvent.ButtonComponent.UnlockUpgrade();     // Handled by PaywallProgressManager
            }
            if (upgradeEvent.UpgradeMethod == UpgradeMethods.Error) {
                TriggerErrorMessage();
            }
        }

        /// <summary>
        /// Catches button click and select events
        /// </summary>
        /// <param name="uIEvent"></param>
        public virtual void OnMMEvent(PWUIEvent uIEvent) {
            if (ShowOnSelect) {
                if (uIEvent.EventType == UIEventTypes.Select) {
                    if (uIEvent.Obj.TryGetComponent(out _currentButton)) {
                        UpdateDetailsDisplay();
                    }
                }
            }
            // If an UpgradeButton is clicked, either update the details display or try to unlock the upgrade
            if (uIEvent.EventType == UIEventTypes.Click) {
                if (UsingBuyButtons) {
                    _currentButton = uIEvent.Obj.GetComponent<UpgradeButton>();
                    if (!ShowOnSelect) {
                        UpdateDetailsDisplay();
                    }
                    if (_currentButton.Unlocked) {
                        ToggleBuyButtons(false);
                    } else {
                        ToggleBuyButtons(true);
                        UpgradeButtonsCanvasGroup.interactable = false;
                        TopBarCanvasGroup.interactable = false;
                        EventSystem.current.SetSelectedGameObject(BuyButton);
                    }
                } else {
                    uIEvent.Obj.GetComponent<UpgradeButton>().TryUnlockUpgrade();
                }
            }
        }

        #region Editor

        /// <summary>
        /// Used by editor
        /// </summary>
        /// <param name="upgrades"></param>
        public virtual void SetUpgradesEditor(List<UpgradeButton> upgradeButtons) {
            UpgradeButtons = upgradeButtons;
        }

        #endregion

        /// <summary>
        /// Set the EventSystem selected gameobject
        /// </summary>
        /// <param name="inputEvent"></param>
        public virtual void OnMMEvent(PWInputEvent inputEvent) {
            if (inputEvent.EventType == PWInputEventTypes.Back) {
                if (UsingBuyButtons && _buyButtonsActive) {
                    ToggleBuyButtons(false);
                }
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<PaywallUpgradeEvent>();
            this.MMEventStartListening<PWUIEvent>();
            this.MMEventStartListening<PWInputEvent>();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<PaywallUpgradeEvent>();
            this.MMEventStopListening<PWUIEvent>();
            this.MMEventStopListening<PWInputEvent>();
        }

    }

}
