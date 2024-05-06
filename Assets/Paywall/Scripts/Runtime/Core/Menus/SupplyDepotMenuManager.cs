using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;
using Paywall.Tools;
using System.Linq;
using static UnityEditor.Progress;
using System;

namespace Paywall {

    public enum DepotItemTypes { None, EX, Health, Ammo, OverclockHealth, Module }

    /// <summary>
    /// Handles UI button presses, input, while in the supply depot menu
    /// </summary>
    public class SupplyDepotMenuManager : Singleton_PW<SupplyDepotMenuManager>, MMEventListener<MMGameEvent>, MMEventListener<PaywallDialogueEvent> {
        [field: Header("Price Multiplier")]

        /// How much to increase the price with each subsequent level
        [field: Tooltip("How much to increase the price with each subsequent level")]
        [field: SerializeField] public float PriceMultiplier { get; protected set; } = 1.1f;

        [field: Header("Buttons")]

        /// The gain EX button
        [field: Tooltip("The gain EX button")]
        [field: SerializeField] public Button EXButton { get; protected set; }
        /// The gain health capsule button
        [field: Tooltip("The gain health capsule button")]
        [field: SerializeField] public Button HealthButton { get; protected set; }
        /// The gain ammo capsule button
        [field: Tooltip("The gain ammo capsule button")]
        [field: SerializeField] public Button AmmoButton { get; protected set; }
        /// The no to overclock button
        [field: Tooltip("The no to overclock button")]
        [field: SerializeField] public Button NoButton { get; protected set; }
        /// The leave depot button
        [field: Tooltip("The leave depot button")]
        [field: SerializeField] public Button LeaveButton { get; protected set; }
        /// The leave depot button
        [field: Tooltip("The leave depot button")]
        [field: SerializeField] public Button TalkButton { get; protected set; }

        [field: Header("Canvas Groups")]

        /// How much to increase level speed when overclocking
        [field: Tooltip("How much to increase level speed when overclocking")]
        [field: SerializeField] public GameObject SupplyDepotMenuCanvas { get; protected set; }
        /// The container for all item buttons
        [field: Tooltip("The container for all item buttons")]
        [field: SerializeField] public GameObject ItemButtonContainer { get; protected set; }
        /// The container for all item buttons
        [field: Tooltip("The container for all item buttons")]
        [field: SerializeField] public GameObject StandardSelectionContainer { get; protected set; }
        /// The container for module buttons
        [field: Tooltip("The container for module buttons")]
        [field: SerializeField] public GameObject ModulesContainer { get; protected set; }
        /// The container for shop options
        [field: Tooltip("The container for shop options")]
        [field: SerializeField] public GameObject ShopContainer { get; protected set; }
        /// The container for ritual options
        [field: Tooltip("The container for ritual options")]
        [field: SerializeField] public GameObject RitualContainer { get; protected set; }
        /// The container for selling module buttons
        [field: Tooltip("The container for selling module buttons")]
        [field: SerializeField] public GameObject SellModulesContainer { get; protected set; }

        [field: Header("Descriptions")]

        /// The selected item's name
        [field: Tooltip("The selected item's name")]
        [field: SerializeField] public TextMeshProUGUI ItemNameText { get; protected set; }
        /// The selected item's description
        [field: Tooltip("The selected item's description")]
        [field: SerializeField] public TextMeshProUGUI ItemDescriptionText { get; protected set; }

        [field: Header("Dialogue Controls")]

        /// The dialogue box container
        [field: Tooltip("The dialogue box container")]
        [field: SerializeField] public GameObject DialogueContainer { get; protected set; }
        /// The dialogue text
        [field: Tooltip("The dialogue text")]
        [field: SerializeField] public TextMeshProUGUI DialogueText { get; protected set; }
        /// If true, the dialogue controls have priority and supply depot buttons are not interactable
        [field: Tooltip("If true, the dialogue controls have priority and supply depot buttons are not interactable")]
        [field: SerializeField] public bool LockDialogueControl { get; protected set; }
        /// The masking canvas group for the dialogue override
        [field: Tooltip("The masking canvas group for the dialogue override")]
        [field: SerializeField] public GameObject DialogueMask { get; protected set; }

        [field: Header("Dialogue Lines")]

        /// The dialogue text when depot is entered
        [field: Tooltip("The dialogue text when depot is entered")]
        [field: TextArea]
        [field: SerializeField] public string EnterDepotDialogue { get; protected set; }
        /// The dialogue text when depot is entered for the first time
        [field: Tooltip("The dialogue text when depot is entered for the first time")]
        [field: SerializeField] public List<DialogueLine> EnterDepotDialogueLines { get; protected set; }

        [field: Header("Modules")]

        /// How many modules are provided as options
        [field: Tooltip("How many modules are provided as options")]
        [field: SerializeField] public int NumberOfModulesDisplayed { get; protected set; } = 3;

        [field: Header("Button Lists")]

        /// List of module buttons
        [field: Tooltip("List of module buttons")]
        [field: SerializeField] public List<DepotButtonDataReference> ModuleButtonList { get; protected set; }
        /// List of shop buttons
        [field: Tooltip("List of shop buttons")]
        [field: SerializeField] public List<DepotButtonDataReference> ShopButtonList { get; protected set; }
        /// List of ritual buttons
        [field: Tooltip("List of ritual buttons")]
        [field: SerializeField] public List<DepotButtonDataReference> RitualButtonList { get; protected set; }

        /// List of inactive valid modules (ones that are in the shop pool)
        protected List<ScriptableModule> _inactiveModuleList = new();
        protected List<DepotItemList> _validDepotSets = new();
        protected List<ScriptableRitualItem> _validRitualItems = new();

        protected BaseScriptableDepotItem _currentItem;
        protected DepotButtonDataReference _currentButton;
        protected bool _firstEntered;
        protected int _firstEmptyShopButton;
        protected System.Random _moduleRandomizer;
        protected System.Random _shopRandomizer;
        protected System.Random _ritualRandomizer;

        /// <summary>
        /// Initialize module dict
        /// </summary>
        protected override void Awake() {
            base.Awake();
            // Initialize the inactive module list, only adding valid modules
            foreach (ModuleData module in PaywallProgressManager.Instance.ModulesDict.Values) {
                if (!module.IsActive && module.IsValid) {
                    _inactiveModuleList.Add(module.Module);
                }
            }

            // Set the default item buttons, since these will never change
            foreach (BaseScriptableDepotItem item in PaywallProgressManager.Instance.DefaultShopItemsList.Items) {
                if (!PaywallProgressManager.Instance.DefaultShopItemsDict[item.Name].IsValid) 
                    return;
                DepotButtonDataReference button = ShopButtonList[_firstEmptyShopButton++];
                button.SetupButton(item, false);
            }

            // First ritual is health, which always appears, don't add that to the randomizer
            foreach (DepotItemData data in PaywallProgressManager.Instance.RitualItemsDict.Values) {
                if (data.IsValid && !data.Name.Equals(PaywallProgressManager.Instance.RitualItemsList.Items[0].Name)) {
                    _validRitualItems.Add((ScriptableRitualItem)data.DepotItem);
                }
            }
            RitualButtonList[0].SetupButton(PaywallProgressManager.Instance.RitualItemsList.Items[0], false);

            _moduleRandomizer = RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
            _shopRandomizer = RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
            _ritualRandomizer = RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
        }

        /// <summary>
        /// What to do when the supply depot is entered
        /// </summary>
        protected virtual void EnterSupplyDepot() {
            DialogueMask.SetActiveIfNotNull(false);

            // First time enter event, play dialogue first
            if (!PaywallProgressManager.Instance.EventFlags.EnterDepotFirstTime) {
                EventSystem.current.SetSelectedGameObject(null);
                SupplyDepotMenuCanvas.GetComponent<CanvasGroup>().interactable = false;
                PaywallDialogueEvent.Trigger(DialogueEventTypes.Open, EnterDepotDialogueLines);
                PaywallProgressManager.Instance.EventFlags.EnterDepotFirstTime = true;
                DialogueMask.SetActiveIfNotNull(true);
                _firstEntered = true;
            }
            // Otherwise set default dialogue
            else {
                SetDialogueText(EnterDepotDialogue);
                _currentItem = ModuleButtonList[0].DepotItem;
                _currentButton = ModuleButtonList[0];
            }

            // Generate module options and fill out the buttons accordingly
            GenerateModuleSelection();
            GenerateShopSelection();
            GenerateRitualSelection();

            EventSystem.current.sendNavigationEvents = true;

            ItemNameText.text = string.Empty;
            ItemDescriptionText.text = string.Empty;

            // Set the proper active state of all objects
            LeaveButton.gameObject.SetActive(false);
            ItemButtonContainer.SetActive(true);
            StandardSelectionContainer.SetActive(true);
            if (_inactiveModuleList.Count < NumberOfModulesDisplayed) {
                ModulesContainer.SetActive(false);
                ShopContainer.SetActive(true);
            }
            else {
                ModulesContainer.SetActive(true);
                ShopContainer.SetActive(false);
            }
            RitualContainer.SetActive(false);
            SellModulesContainer.SetActive(false);
        }

        #region On Click/Select
        
        /// <summary>
        /// Switches shop display between regular shop and rituals
        /// </summary>
        public virtual void SwitchTab() {
            ShopContainer.SetActive(!ShopContainer.activeSelf);
            RitualContainer.SetActive(!RitualContainer.activeSelf);
            _currentButton = null;
            _currentItem = null;
        }

        /// <summary>
        /// Leave depot immediately. Use for debugging
        /// </summary>
        public virtual void ForceLeave() {
            LeaveDepot();
        }

        /// <summary>
        /// Set currently selected item and button
        /// </summary>
        /// <param name="item"></param>
        public virtual void SetBuyOption(BaseScriptableDepotItem item, DepotButtonDataReference button) {
            ItemNameText.text = item.Name.ToUpper();
            // If the selected item is a module, the description is different if it's enhanced
            if (item is ScriptableModule module) {
                if (module.IsEnhanced) {
                    ItemDescriptionText.text = module.EnhancedDescription;
                }
                else {
                    ItemDescriptionText.text = item.Description;
                }
            }
            else {
                ItemDescriptionText.text = item.Description;
            }
            _currentItem = item;
            if (_currentButton == null) {
                _currentButton = button;
            }
            else {
                _currentButton.SetOutline(false);
                _currentButton = button;
            }
            _currentButton.SetOutline(true);
        }

        /// <summary>
        /// Buy the currently selected item
        /// OnClick event of the Confirm button
        /// </summary>
        public virtual void BuyCurrentOption() {
            if (_currentItem == null) return;

            // Buy module
            if (_currentItem is ScriptableModule module) {
                BuyModule(module);
                ModulesContainer.SetActive(false);
                ShopContainer.SetActive(true);
                _currentItem = null;
                ActivateLeave();        // Once a module is chosen, the player has the option to leave or buy from shop
            }
            // Buy shop option (usually requires trinkets)
            else {
                // If we are selecting a ritual
                if (RitualContainer.activeSelf) {

                }

                if (PaywallProgressManager.Instance.Trinkets < _currentItem.Cost) {
                    return;
                }
                PaywallCreditsEvent.Trigger(MoneyTypes.Trinket, MoneyMethods.Add, -_currentItem.Cost);
                _currentItem.BuyAction();

                // Grey out a shop button if it costs more than what we can afford
                foreach(DepotButtonDataReference button in ShopButtonList) {
                    if (button.gameObject.activeSelf) {
                        if (button.DepotItem.Cost > PaywallProgressManager.Instance.Trinkets) {
                            SetButtonState(button, false);
                        }
                        else {
                            SetButtonState(button, true);
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Buy a module from the shop
        /// </summary>
        /// <param name="module"></param>
        protected virtual void BuyModule(ScriptableModule module) {
            PaywallProgressManager.Instance.ModulesDict[module.Name].IsActive = true;
            _inactiveModuleList.Remove(module);
            PaywallModuleEvent.Trigger(module);
        }

        /// <summary>
        /// Sell a module from inventory
        /// </summary>
        /// <param name="module"></param>
        protected virtual void SellModule(ScriptableModule module) {
            PaywallProgressManager.Instance.ModulesDict[module.Name].IsActive = false;
            _inactiveModuleList.Add(module);
        }

        /// <summary>
        /// Generates the module name/image/descriptions for the modules that are to be displayed in this depot
        /// Called every time a depot is entered and the player has module space left
        /// </summary>
        protected virtual void GenerateModuleSelection() {
            if (_inactiveModuleList.Count < NumberOfModulesDisplayed) {
                return;
            }
            int idx;
            List<ScriptableModule> modules = _inactiveModuleList.ToList();
            // Randomly pull modules from the list, with removal
            for (int i = 0; i < ModuleButtonList.Count; i++) {
                if (i < NumberOfModulesDisplayed) {
                    idx = _moduleRandomizer.Next(0, modules.Count);
                    if (modules[idx].UISprite != null) {
                        ModuleButtonList[i].SetImage(modules[idx].UISprite);
                        ModuleButtonList[i].image.sprite = modules[idx].UISprite;
                    }
                    ModuleButtonList[i].SetupButton(modules[idx], false);
                    ModuleButtonList[i].gameObject.SetActive(true);
                    modules.RemoveAt(idx);
                }
                else {
                    ModuleButtonList[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Generates shop options (buyable ones, not the modules) to be displayed
        /// Displayed after selecting a module
        /// </summary>
        protected virtual void GenerateShopSelection() {
            if (_validDepotSets.Count == 0) {
                foreach (DepotItemListData data in PaywallProgressManager.Instance.ShopItemSetsDict.Values) {
                    if (data.Active) {
                        _validDepotSets.Add(data.ItemList);
                    }
                }
            }
            DepotItemList list = _validDepotSets[_shopRandomizer.Next(0, _validDepotSets.Count)];
            int idx = 0;

            for (int i = 0; i < ShopButtonList.Count; i++) {
                ShopButtonList[i].SetOutline(false);
                if (i < _firstEmptyShopButton) {
                    ShopButtonList[i].gameObject.SetActive(true);
                    ShopButtonList[i].SetOutline(false);
                }
                else {
                    if (idx < list.Items.Count) {
                        ShopButtonList[i].SetupButton(list.Items[idx], false);
                        ShopButtonList[i].gameObject.SetActive(true);
                        idx++;
                    }
                    else {
                        ShopButtonList[i].gameObject.SetActive(false);
                    }
                }
            }

            // Grey out a shop button if it costs more than what we can afford
            foreach (DepotButtonDataReference button in ShopButtonList) {
                if (button.gameObject.activeSelf) {
                    if (button.DepotItem.Cost > PaywallProgressManager.Instance.Trinkets) {
                        SetButtonState(button, false);
                    }
                    else {
                        SetButtonState(button, true);
                    }
                }
            }
        }

        /// <summary>
        /// Generates ritual selection
        /// </summary>
        protected virtual void GenerateRitualSelection() {
            int i = 0;
            ScriptableRitualItem item = _validRitualItems[_ritualRandomizer.Next(0, _validRitualItems.Count)];
            
            foreach (DepotButtonDataReference button in RitualButtonList) {
                if (i == 0) { i++; continue; }
                if (i > 1) { i++; button.gameObject.SetActive(false); continue; }
                button.SetupButton(item, false);
            }

        }

        /// <summary>
        /// Activates the leave button and allows for the player to leave the depot
        /// </summary>
        protected virtual void ActivateLeave() {
            LeaveButton.gameObject.SetActive(true);
        }

        protected virtual IEnumerator WaitToSelect(GameObject selected) {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(selected);
        }

        /// <summary>
        /// Called by Leave Button OnClick event. Trigger leave depot.
        /// </summary>
        public virtual void LeaveDepotButtonPressed() {
            LeaveDepot();
        }

        /// <summary>
        /// Grey a button out (if the item can't be afforded, for example)
        /// Doesn't set the interactable to false so that the player can examine the item
        /// Bool presents option to reset the color of the button instead
        /// </summary>
        /// <param name="button"></param>
        protected virtual void SetButtonState(ButtonDataReference button, bool active) {
            if (active) {
                button.ResetColor();
            }
            else {
                button.SetColor(button.colors.disabledColor);
            }
        }

        #region Item Effects

        /// <summary>
        /// Add health fragment(s) to runner inventory
        /// </summary>
        /// <param name="amount"></param>
        protected virtual void PickHealth(int amount) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.Health, amount);
        }

        /// <summary>
        /// Add ammo fragment(s) to runner inventory
        /// </summary>
        /// <param name="amount"></param>
        protected virtual void PickAmmo(int amount) {
            RunnerItemPickEvent.Trigger(PowerUpTypes.Ammo, amount);
        }

        /// <summary>
        /// Add EX meter to player character
        /// </summary>
        /// <param name="amount"></param>
        protected virtual void PickEX(float amount) {
            PaywallEXChargeEvent.Trigger(amount, ChangeAmountMethods.Add);
        }

        #endregion

        protected virtual void ChangeToShopDisplay() {
            ModulesContainer.SetActive(false);
            ShopContainer.SetActive(true);
        }

        /// <summary>
        /// Sets the dialogue text
        /// </summary>
        /// <param name="dialogue"></param>
        protected virtual void SetDialogueText(string dialogue) {
            DialogueText.text = dialogue;
        }

        /// <summary>
        /// Leave the depot and resume the game
        /// Unpause, set game status
        /// </summary>
        protected virtual void LeaveDepot() {
            MMGameEvent.Trigger("LeaveDepot");
            //ProceduralLevelGenerator.Instance.LeaveShop();    // Use game event listener in ProceduralLevelGenerator instead
            GameManagerIRE_PW.Instance.Pause(PauseScreenMethods.SupplyDepotScreen);
        }

        /// <summary>
        /// Wait for end of frame to close dialogue, otherwise hitting the submit button to close the dialogue also hits whatever button is currently selected
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator CloseDialogueCo() {
            yield return new WaitForEndOfFrame();
            DialogueMask.SetActiveIfNotNull(false);
            if (_firstEntered) {
                _firstEntered = false;
                SupplyDepotMenuCanvas.GetComponent<CanvasGroup>().interactable = true;
                SetDialogueText(EnterDepotDialogue);
            }
            //_currentItem = ModuleButtonList[0].DepotItem;
            //_currentButton = ModuleButtonList[0];
            //EventSystem.current.SetSelectedGameObject(ModuleButtonList[0].gameObject);
            //ItemNameText.text = item.Name;
            //ItemDescriptionText .text = item.Description;
        }

        public void OnMMEvent(MMGameEvent gameEvent) {
            
        }

        /// <summary>
        /// Catch dialogue events, set UI when dialogue closes
        /// </summary>
        /// <param name="dialogueEvent"></param>
        public void OnMMEvent(PaywallDialogueEvent dialogueEvent) {
            if (dialogueEvent.DialogueEventType == DialogueEventTypes.Close) {
                StartCoroutine(CloseDialogueCo());
            }
        }

        /// <summary>
        /// Component is enabled by GUIManager when supply depot is entered
        /// </summary>
        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<PaywallDialogueEvent>();
            EnterSupplyDepot();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<PaywallDialogueEvent>();
        }
    }
}
