using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;
using Paywall.Tools;

namespace Paywall {

    public enum DepotItemTypes { EX, Health, Ammo, OverclockHealth, OverclockAmmo }

    /// <summary>
    /// Handles UI button presses, input, while in the supply depot menu
    /// </summary>
    public class SupplyDepotMenuManager : Singleton_PW<SupplyDepotMenuManager>, MMEventListener<MMGameEvent>, MMEventListener<PaywallDialogueEvent> {
        /// How much to increase level speed when overclocking
        [field: Tooltip("How much to increase level speed when overclocking")]
        [field: SerializeField] public float OverclockSpeed { get; protected set; } = 5f;
        /// How much to increase level speed when overclocking
        [field: Tooltip("How much to increase level speed when overclocking")]
        [field: SerializeField] public GameObject SupplyDepotMenuCanvas { get; protected set; }

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

        [field: Header("Containers")]

        /// The container for all item buttons
        [field: Tooltip("The container for all item buttons")]
        [field: SerializeField] public GameObject ItemButtonContainer { get; protected set; }
        /// The container for all item buttons
        [field: Tooltip("The container for all item buttons")]
        [field: SerializeField] public GameObject StandardSelectionContainer { get; protected set; }
        /// The leave depot button
        [field: Tooltip("The leave depot button")]
        [field: SerializeField] public GameObject OverclockContainer { get; protected set; }

        [field: Header("Descriptions")]

        /// The selected item's description
        [field: Tooltip("The selected item's description")]
        [field: SerializeField] public TextMeshProUGUI ItemDescriptionText { get; protected set; }
        /// The gain EX description
        [field: Tooltip("The gain EX description")]
        [field: TextArea]
        [field: SerializeField] public string EXDescription { get; protected set; }
        /// The gain health description
        [field: Tooltip("The gain health description")]
        [field: TextArea]
        [field: SerializeField] public string HealthDescription { get; protected set; }
        /// The gain ammo description
        [field: Tooltip("The gain ammo description")]
        [field: TextArea]
        [field: SerializeField] public string AmmoDescription { get; protected set; }
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

        protected PowerUpTypes _currentPowerUpType;
        protected DepotItemTypes _currentDepotItemType;
        protected bool _firstEntered;

        /// <summary>
        /// What to do when the supply depot is entered
        /// </summary>
        protected virtual void EnterSupplyDepot() {
            DialogueMask.SetActiveIfNotNull(false);
            EventSystem.current.SetSelectedGameObject(EXButton.gameObject);

            if (!PaywallProgressManager.Instance.EventFlags.EnterDepotFirstTime) {
                EventSystem.current.SetSelectedGameObject(null);
                EXButton.GetComponent<UIGetFocus>().enabled = false;
                PaywallDialogueEvent.Trigger(DialogueEventTypes.Open, EnterDepotDialogueLines);
                PaywallProgressManager.Instance.EventFlags.EnterDepotFirstTime = true;
                DialogueMask.SetActiveIfNotNull(true);
                _firstEntered = true;
            }
            else {
                SetDialogueText(EnterDepotDialogue);
            }

            EventSystem.current.sendNavigationEvents = true;

            // Set active state of all containers
            LeaveButton.gameObject.SetActive(false);
            OverclockContainer.SetActive(false);
            ItemButtonContainer.SetActive(true);
            StandardSelectionContainer.SetActive(true);
        }

        protected virtual IEnumerator EnterSupplyDepotCo() {
            yield return null;
        }

        #region On Click

        /// <summary>
        /// Assign to OnClick event of EX button
        /// </summary>
        public virtual void EXButtonPressed() {
            ItemButtonContainer.SetActive(false);
            LeaveButton.gameObject.SetActive(true);
            StartCoroutine(WaitToSelect(LeaveButton.gameObject));
            PaywallEXChargeEvent.Trigger(EXGain, ChangeAmountMethods.Add);
        }

        /// <summary>
        /// Assign to OnClick event of health button
        /// Gain one health fragment
        /// </summary>
        public virtual void HealthButtonPressed() {
            PickHealth(HealthGain);
            ChangeToOverclockDisplay(DepotItemTypes.Health);
        }

        /// <summary>
        /// Assign to OnClick event of ammo button
        /// Gain one ammo fragment
        /// </summary>
        public virtual void AmmoButtonPressed() {
            PickAmmo(AmmoGain);
            ChangeToOverclockDisplay(DepotItemTypes.Ammo);
        }

        /// <summary>
        /// Assign to OnClick of overclock choice buttons (yes or no)
        /// </summary>
        public virtual void OverclockButtonPressed(bool overclock) {
            if (overclock) {
                Overclock(_currentPowerUpType);
            }

            OverclockContainer.SetActive(false);
            LeaveButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(LeaveButton.gameObject);
        }

        /// <summary>
        /// Leave depot immediately. Use for debugging
        /// </summary>
        public virtual void ForceLeave() {
            LeaveDepot();
        }

        #endregion

        /// <summary>
        /// Overclock exosuit, gain additional fragments and increase level speed
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

        #region Set Description

        /// <summary>
        /// Called by OnSelect event. Set description when hovering EX button
        /// </summary>
        public virtual void SetDescriptionEX() {
            SetDescription(DepotItemTypes.EX);
        }

        /// <summary>
        /// Called by OnSelect event. Set description when hovering health button
        /// </summary>
        public virtual void SetDescriptionHealth() {
            SetDescription(DepotItemTypes.Health);
        }

        /// <summary>
        /// Called by OnSelect event. Set description when hovering ammo button
        /// </summary>
        public virtual void SetDescriptionAmmo() {
            SetDescription(DepotItemTypes.Ammo);
        }

        /// <summary>
        /// Sets the description text
        /// </summary>
        /// <param name="depotItemType"></param>
        public virtual void SetDescription(DepotItemTypes depotItemType) {
            StringBuilder sb = new();
            string newText;

            switch (depotItemType) {
                case DepotItemTypes.EX:
                    ItemDescriptionText.text = EXDescription;
                    break;
                case DepotItemTypes.Health:
                    ItemDescriptionText.text = HealthDescription;
                    break;
                case DepotItemTypes.Ammo:
                    ItemDescriptionText.text = AmmoDescription;
                    break;
                case DepotItemTypes.OverclockHealth:
                    newText = OverclockDescription.Replace("RESOURCE", "Health");
                    ItemDescriptionText.text = newText;
                    break;
                case DepotItemTypes.OverclockAmmo:
                    newText = OverclockDescription.Replace("RESOURCE", "Ammo");
                    ItemDescriptionText.text = newText;
                    break;
            }
        }

        #endregion

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

        /// <summary>
        /// Activates overclock button display and sets description
        /// </summary>
        /// <param name="depotItemType"></param>
        protected virtual void ChangeToOverclockDisplay(DepotItemTypes depotItemType) {
            StandardSelectionContainer.SetActive(false);
            OverclockContainer.SetActive(true);
            EventSystem.current.SetSelectedGameObject(NoButton.gameObject);
            SetDescription(depotItemType);
            SetDialogueText(OverclockDialogue);
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
        /// Wait for end of frame to close dialogue, otherwise hitting the submit button to close the dialogue also hits the EX button
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator CloseDialogueCo() {
            yield return new WaitForEndOfFrame();
            DialogueMask.SetActiveIfNotNull(false);
            if (_firstEntered) {
                _firstEntered = false;
                SetDialogueText(EnterDepotDialogue);
                EventSystem.current.SetSelectedGameObject(EXButton.gameObject);
                EXButton.GetComponent<UIGetFocus>().enabled = true;
            }
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
        /// Component is enabled when supply depot is entered
        /// </summary>
        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<PaywallDialogueEvent>();
            EnterSupplyDepot();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<PaywallDialogueEvent>();
        }
    }
}
