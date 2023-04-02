using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Paywall.Tools;
using TMPro;
using UnityEngine.UI;
using System.Text;

namespace Paywall.Documents {

    /// <summary>
    /// Manages the email button display
    /// Initializes, instantiates new buttons when new emails are added
    /// </summary>
    public class EmailButtonDisplay : MonoBehaviour, MMEventListener<EmailEvent> {

        #region

        /// The ID of the player character. Default is Player1
        [field: Tooltip("The ID of the player character. Default is Player1")]
        [field: SerializeField] public string PlayerID { get; protected set; } = "Player1";
        /// The gameobject with the DocumentInventoryDetails component attached
        [field: Tooltip("The gameobject with the DocumentInventoryDetails component attached")]
        [field: SerializeField] public GameObject EmailDetailsDisplay { get; protected set; } 
        /// The prefab for newly instantiated buttons
        [field: Tooltip("The prefab for newly instantiated buttons")]
        [field: SerializeField] public GameObject EmailButton { get; protected set; }
        /// If true, hides the document display on start
        [field: Tooltip("If true, hides the document display on start")]
        [field: SerializeField] public bool HideContainerOnStart { get; protected set; }
        /// The indent size to use for new buttons. Should be as same as the global.
        [field: Tooltip("The indent size to use for new buttons. Should be as same as the global.")]
        [field: SerializeField] public float TabSize { get; protected set; } = 27f;

        [field:Header("Read/Unread Settings")]

        /// If true, use a separate unread icon instead of read/unread color
        [field: Tooltip("If true, use a separate unread icon instead of read/unread color")]
        [field: SerializeField] public bool SeparateUnreadIcon { get; protected set; } = false;
        /// The email button's read color
        [field: Tooltip("The email button's read color")]
        [field: SerializeField] public Color ReadColor { get; protected set; }
        /// The email button's unread color
        [field: Tooltip("The email button's unread color")]
        [field: SerializeField] public Color UnreadColor { get; protected set; }
        /// The email buttons' container
        [field: Tooltip("The email buttons' container")]
        [field: SerializeField] public GameObject EmailButtonsContainer { get; protected set; }
        /// The unread container
        [field: Tooltip("The unread container")]
        [field: SerializeField] public GameObject UnreadContainer { get; protected set; }
        /// The unread container content
        [field: Tooltip("The unread container content")]
        [field: SerializeField] public GameObject UnreadContainerContent { get; protected set; }
        /// The read container
        [field: Tooltip("The read container")]
        [field: SerializeField] public GameObject ReadContainer { get; protected set; }
        /// The read container content
        [field: Tooltip("The read container content")]
        [field: SerializeField] public GameObject ReadContainerContent { get; protected set; }
        /// If true, have a button toggle the unread/read container on and off
        [field: Tooltip("If true, have a button toggle the unread/read container on and off")]
        [field: SerializeField] public bool UseToggle { get; protected set; }
        /// The toggle button's text component
        [field: Tooltip("The toggle button's text component")]
        [field: FieldCondition("UseToggle", true)]
        [field: SerializeField] public TextMeshProUGUI ContainerToggleButtonText { get; protected set; }
        /// Toggle buttons container
        [field: Tooltip("Toggle buttons container")]
        [field: FieldCondition("UseToggle", true, true)]
        [field: SerializeField] public GameObject ToggleButtonsContainer { get; protected set; }

        #endregion

        protected SortedDictionary<string, Transform> _unreadDocumentButtons = new SortedDictionary<string, Transform>();
        protected SortedDictionary<string, Transform> _readDocumentButtons = new SortedDictionary<string, Transform>();
        protected EmailItem _currentItem;
        protected GameObject _currentButton;
        protected const string _unreadButtonLabel = "See Unread Messages";
        protected const string _readButtonLabel = "See Read Messages";
        protected EmailInventory _emailInventory = null;
        protected List<GameObject> _activeButtons = new List<GameObject>();
        protected bool _redrawRequired;
        protected bool _categoriesInitialized;
        protected StringBuilder _stringBuilder = new StringBuilder();

        protected virtual void Awake() {
            UnreadContainer.SetActive(true);
            //ReadContainer.SetActive(false);
            ContainerToggleButtonText.text = _readButtonLabel;
            if (UnreadContainerContent == null) {
                UnreadContainerContent = UnreadContainer.transform.Find("Content").gameObject;
            }
            if (ReadContainerContent == null) {
                //ReadContainerContent = ReadContainerContent.transform.Find("Content").gameObject;
            }
            if (EmailButtonsContainer == null) {
                EmailButtonsContainer = transform.GetChild(0).gameObject;
            }
        }

        /// Get the scene's DocumentInventory
        public EmailInventory TargetInventory {
            get {
                foreach (EmailInventory inventory in UnityEngine.Object.FindObjectsOfType<EmailInventory>()) {
                    if (inventory.PlayerID == PlayerID) {
                        _emailInventory = inventory;
                    }
                }
                return _emailInventory;
            }
        }

        /// <summary>
        /// Creates and sets up a button from the given DocumentClass item
        /// Used when a document is added to the inventory
        /// </summary>
        public virtual void SetupButton(EmailItem item) {
            if (TargetInventory == null) {
                Debug.LogError("The " + name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventory.name + "), or set that TargetInventoryName to one that exists.");
                return;
            }

            if (TargetInventory.UnreadEmails.ContainsKey(item.ItemID)) {
                // Instantiate the button and set its size
                GameObject newButton = InitializeDocumentButton(item, UnreadContainerContent.transform);    // The new button's gameobject parent should be the unread container
                // After instantiating the new button, the document buttons need to be rearranged in alphabetical order
                _unreadDocumentButtons.Add(newButton.name, newButton.transform);
                // Arrange the child objects in order
                foreach (KeyValuePair<string, Transform> child in _unreadDocumentButtons) {
                    child.Value.SetAsLastSibling();
                }
            } else {
                Debug.LogError("The document item has an invalid ItemID.");
            }

        }

        /// <summary>
        /// When a document is updated, call this function
        /// It will update unread graphics
        /// </summary>
        public virtual void UpdateDisplay(EmailItem item) {
            if (EmailDetailsDisplay.GetComponent<EmailDetailsDisplay>().Email != null &&
                EmailDetailsDisplay.GetComponent<EmailDetailsDisplay>().Email.ItemID == item.ItemID) {
                EmailDetailsDisplay.SetActive(false);
            }

            Transform button = FindChild.FindDeepChild(transform, item.ItemID);
            button.GetComponent<Image>().color = UnreadColor;
        }

        /// <summary>
        /// Initializes a document's button in the display
        /// </summary>
        /// <param name="documentItem"></param>
        public virtual GameObject InitializeDocumentButton(EmailItem item, Transform parentTransform) {
            GameObject buttonObject = Instantiate(EmailButton, parentTransform);
            EmailButtonControl control = buttonObject.GetComponent<EmailButtonControl>();
            control.SetItem(item);

            buttonObject.name = item.ItemID;
            _stringBuilder.Length = 0;
            _stringBuilder.Append(item.Sender);
            _stringBuilder.Append("\n");
            _stringBuilder.Append(item.SubjectLine);
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = _stringBuilder.ToString();
            // Set the button's onClick
            Button buttonComponent = buttonObject.GetComponent<Button>();
            if (buttonComponent == null) {
                buttonComponent = buttonObject.GetComponentInChildren<Button>();
            }
            buttonComponent.onClick.AddListener(delegate { Click(buttonObject); });
            if (SeparateUnreadIcon) {
                buttonComponent.gameObject.GetComponent<Image>().color = ReadColor;
            }
            return buttonObject;
        }

        /// <summary>
        /// Method called when a document's button is clicked
        /// </summary>
        /// <param name="documentItem"></param>
        public virtual void Click(GameObject button) {
            EmailItem emailItem = button.GetComponent<EmailButtonControl>().emailItem;
            if (!emailItem.Read) {
                if (SeparateUnreadIcon) {
                    button.transform.Find("Unread").gameObject.SetActive(false);
                }
                else {
                    button.GetComponent<Image>().color = ReadColor;
                }
            }
            _currentItem = emailItem;
            _currentButton = button;
            EmailDetailsDisplay.SetActive(true);
            EmailDetailsDisplay.GetComponent<EmailDetailsDisplay>().Display(emailItem);
        }

        /// <summary>
        /// Called by the OnClick of the Mark as Read button
        /// Set the EmailItem to read, and move it to the Read container
        /// </summary>
        public virtual void ReadCurrentItem() {
            _currentButton.GetComponent<EmailButtonControl>().MarkAsRead();
            _currentButton.transform.parent = ReadContainerContent.transform;
            _unreadDocumentButtons.Remove(_currentButton.name);
            _readDocumentButtons.Add(_currentButton.name, _currentButton.transform);

            UpdateButtons();

            _currentButton = null;
            _currentItem = null;
            EmailEvent.Trigger(EmailEventType.Read, _currentButton.GetComponent<EmailButtonControl>().emailItem, PlayerID);
        }

        /// <summary>
        /// Updates button display by arranging them alphabetically (by ItemID)
        /// Updates the explicit navigation if applicable
        /// </summary>
        protected virtual void UpdateButtons() {
            // Arrange the child objects in order
            foreach (KeyValuePair<string, Transform> child in _unreadDocumentButtons) {
                child.Value.SetAsLastSibling();
            }
            // Arrange the child objects in order
            foreach (KeyValuePair<string, Transform> child in _readDocumentButtons) {
                child.Value.SetAsLastSibling();
            }
            if (ReadContainerContent.TryGetComponent(out VerticalScroller vs)) {
                vs.SetNavigation();
            }
            if (UnreadContainerContent.TryGetComponent(out vs)) {
                vs.SetNavigation();
            }
        }

        /// <summary>
        /// Toggles the active state of Unread/Read containers
        /// Called by the Unread/Read toggle button
        /// </summary>
        public virtual void ToggleContainers() {
            if (UnreadContainer.activeSelf) {
                UnreadContainer.SetActive(false);
                ReadContainer.SetActive(true);
                if (UseToggle) {
                    ContainerToggleButtonText.text = _unreadButtonLabel;
                }
            } else {
                UnreadContainer.SetActive(true);
                ReadContainer.SetActive(false);
                if (UseToggle) { 
                    ContainerToggleButtonText.text = _readButtonLabel; 
                }
            }
        }

        /// <summary>
        /// Activates read container and buttons container
        /// </summary>
        public virtual void ActivateReadContainer() {
            ReadContainer.SetActive(true);
            UnreadContainer.SetActive(false);
            EmailButtonsContainer.SetActive(true);
            ToggleButtonsContainer.SetActive(false);
        }

        /// <summary>
        /// Activates unread container and buttons container
        /// </summary>
        public virtual void ActivateUnreadContainer() {
            ReadContainer.SetActive(false);
            UnreadContainer.SetActive(true);
            EmailButtonsContainer.SetActive(true);
            ToggleButtonsContainer.SetActive(false);
        }

        /// <summary>
        /// Toggles the active state of the email buttons container
        /// </summary>
        public virtual void ActivateButtonsContainer(bool active) {
            EmailButtonsContainer.SetActive(active);
        }

        public virtual void OnMMEvent(EmailEvent emailEvent) {
            if (emailEvent.EventType == EmailEventType.Add) {
                SetupButton(emailEvent.Item);
            }
            if (emailEvent.EventType == EmailEventType.Update) {
                UpdateDisplay(emailEvent.Item);
            }
        }

        /// <summary>
        /// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
        /// </summary>
        protected virtual void OnEnable() {
            this.MMEventStartListening<EmailEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
        /// </summary>
        protected virtual void OnDisable() {
            this.MMEventStopListening<EmailEvent>();
            _currentButton = null;
            _currentItem = null;
        }
    }
}
