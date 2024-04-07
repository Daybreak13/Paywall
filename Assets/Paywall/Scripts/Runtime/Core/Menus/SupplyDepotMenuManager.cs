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

namespace Paywall {

    public enum DepotItemTypes { None, EX, Health, Ammo, OverclockHealth, OverclockAmmo, Module }

    /// <summary>
    /// Handles UI button presses, input, while in the supply depot menu
    /// </summary>
    public class SupplyDepotMenuManager : Singleton_PW<SupplyDepotMenuManager>, MMEventListener<MMGameEvent>, MMEventListener<PaywallDialogueEvent> {
        /// How much to increase level speed when overclocking
        [field: Tooltip("How much to increase level speed when overclocking")]
        [field: SerializeField] public float OverclockSpeed { get; protected set; } = 5f;

        [field: Header("Item Amounts")]

        /// How much EX is given
        [field: Tooltip("How much EX is given")]
        [field: SerializeField] public float EXGain { get; protected set; } = 30f;
        /// How much health is given
        [field: Tooltip("How much health is given")]
        [field: SerializeField] public int HealthGain { get; protected set; } = 1;
        /// How much ammo is given
        [field: Tooltip("How much ammo is given")]
        [field: SerializeField] public int AmmoGain { get; protected set; } = 1;
        /// How much health/ammo is given when overclocking
        [field: Tooltip("How much health/ammo is given when overclocking")]
        [field: SerializeField] public int OverclockGain { get; protected set; } = 1;

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
        /// The leave depot button
        [field: Tooltip("The leave depot button")]
        [field: SerializeField] public GameObject OverclockContainer { get; protected set; }

        [field: Header("Descriptions")]

        /// The selected item's name
        [field: Tooltip("The selected item's name")]
        [field: SerializeField] public TextMeshProUGUI ItemNameText { get; protected set; }
        /// The selected item's description
        [field: Tooltip("The selected item's description")]
        [field: SerializeField] public TextMeshProUGUI ItemDescriptionText { get; protected set; }
        /// The generic overclock description
        [field: Tooltip("The generic overclock description")]
        [field: TextArea]
        [field: SerializeField] public string OverclockDescription { get; protected set; }

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
        /// The dialogue text when overclock is selected
        [field: Tooltip("The dialogue text when overclock is selected")]
        [field: TextArea]
        [field: SerializeField] public string OverclockDialogue { get; protected set; }

        [field: Header("Modules")]

        /// How many modules are provided as options
        [field: Tooltip("How many modules are provided as options")]
        [field: SerializeField] public int NumberOfModulesDisplayed { get; protected set; } = 3;

        [field: Header("Button Lists")]

        /// List of module buttons
        [field: Tooltip("List of module buttons")]
        [field: SerializeField] public List<DepotButtonDataReference> ModuleButtonList { get; protected set; }
        /// List of module buttons
        [field: Tooltip("List of module buttons")]
        [field: SerializeField] public List<DepotButtonDataReference> ShopButtonList { get; protected set; }

        /// List of inactive valid modules (ones that are in the shop pool)
        protected List<ScriptableModule> _inactiveModuleList = new();

        protected BaseScriptableDepotItem _currentItem;
        protected DepotButtonDataReference _currentButton;
        protected bool _firstEntered;
        protected int _firstEmptyShopButton;

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

            foreach (BaseScriptableDepotItem item in PaywallProgressManager.Instance.DefaultShopItemsList.Items) {
                if (!PaywallProgressManager.Instance.DefaultShopItemsDict[item.Name].IsValid) 
                    return;
                DepotButtonDataReference button = ShopButtonList[_firstEmptyShopButton++];
                //button.ImageComponent.sprite = item.UISprite;
                button.TextComponent.text = item.Name;
                button.SetItem(item);
            }
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

            EventSystem.current.sendNavigationEvents = true;
            //EventSystem.current.SetSelectedGameObject(ModuleButtonList[0].gameObject);

            ItemNameText.text = string.Empty;
            ItemDescriptionText.text = string.Empty;

            // Set the proper active state of all objects
            LeaveButton.gameObject.SetActive(false);
            OverclockContainer.SetActive(false);
            ItemButtonContainer.SetActive(true);
            StandardSelectionContainer.SetActive(true);
            ModulesContainer.SetActive(true);
            ShopContainer.SetActive(false);
        }

        #region On Click/Select

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
            _currentButton.SetOutline(false);
            _currentButton = button;
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
                if (PaywallProgressManager.Instance.Trinkets < _currentItem.Cost) {
                    return;
                }
                PaywallCreditsEvent.Trigger(MoneyTypes.Trinket, MoneyMethods.Add, -_currentItem.Cost);
                switch (_currentItem.DepotItemType) {
                    case DepotItemTypes.EX:
                        PickEX(EXGain);
                        break;
                    case DepotItemTypes.Health:
                        PickHealth(HealthGain);
                        break;
                    case DepotItemTypes.Ammo:
                        PickAmmo(AmmoGain);
                        break;
                }

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
            int idx;
            System.Random rand = new();
            List<ScriptableModule> modules = _inactiveModuleList.ToList();
            // Randomly pull modules from the list, with removal
            for (int i = 0; i < NumberOfModulesDisplayed; i++) {
                idx = rand.Next(0, modules.Count);
                if (modules[idx].UISprite != null) {
                    ModuleButtonList[i].SetImage(modules[idx].UISprite);
                    ModuleButtonList[i].image.sprite = modules[idx].UISprite;
                }
                ModuleButtonList[i].TextComponent.text = modules[idx].Name;
                ModuleButtonList[i].SetItem(modules[idx]);
                ModuleButtonList[i].SetOutline(false);
                modules.RemoveAt(idx);
            }
        }

        /// <summary>
        /// Generates shop options (buyable ones, not the modules) to be displayed
        /// Displayed after selecting a module
        /// </summary>
        protected virtual void GenerateShopSelection() {
            //int idx;
            //System.Random rand = new();
            for (int i = 0; i < ShopButtonList.Count; i++) {
                //ShopButtonList[i].TextComponent.text = 
                ShopButtonList[i].SetOutline(false);
                if (i < _firstEmptyShopButton) {
                    ShopButtonList[i].gameObject.SetActive(true);
                }
                else {
                    ShopButtonList[i].gameObject.SetActive(false);
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
        /// Activates the leave button and allows for the player to leave the depot
        /// </summary>
        protected virtual void ActivateLeave() {
            LeaveButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Overclock exosuit, gain additional fragments and increase level speed
        /// The overclock gain is added on top of the normal gain
        /// </summary>
        /// <param name="powerUpType"></param>
        protected virtual void Overclock(PowerUpTypes powerUpType) {
            switch (powerUpType) {
                case PowerUpTypes.EX:
                    break;
                case PowerUpTypes.Health:
                    PickHealth(OverclockGain);
                    break;
                case PowerUpTypes.Ammo:
                    PickAmmo(OverclockGain);
                    break;
            }

            ItemButtonContainer.SetActive(false);
            LeaveButton.gameObject.SetActive(true);

            IncreaseLevelSpeed();
        }

        protected virtual IEnumerator WaitToSelect(GameObject selected) {
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(selected);
        }

        /// <summary>
        /// Overclocking the exosuit increases running speed
        /// </summary>
        public virtual void IncreaseLevelSpeed() {
            LevelManagerIRE_PW.Instance.IncreaseLevelSpeed(OverclockSpeed);
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
            _currentItem = ModuleButtonList[0].DepotItem;
            _currentButton = ModuleButtonList[0];
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
