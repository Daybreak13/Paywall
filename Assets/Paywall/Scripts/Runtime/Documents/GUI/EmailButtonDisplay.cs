using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Paywall.Tools;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

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
        /// If true, the Sender and SubjectLine text goes in two separate text objects
        [field: Tooltip("If true, the Sender and SubjectLine text goes in two separate text objects")]
        [field: SerializeField] public bool UseTwoTexts { get; protected set; } = true;

        [field: Header("Read/Unread Settings")]

        /// If true, use a separate unread icon instead of read/unread color
        [field: Tooltip("If true, use a separate unread icon instead of read/unread color")]
        [field: SerializeField] public bool SeparateUnreadIcon { get; protected set; } = false;
        /// The email button's read color
        [field: Tooltip("The email button's read color")]
        [field: SerializeField] public Color ReadColor { get; protected set; }
        /// The email button's unread color
        [field: Tooltip("The email button's unread color")]
        [field: SerializeField] public Color UnreadColor { get; protected set; }

        [field: Header("Containers")]

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

        [field: Header("Button Settings")]

        [field: SerializeField] public bool UseToggle { get; protected set; }
        /// The toggle button's text component
        [field: Tooltip("The toggle button's text component")]
        [field: FieldCondition("UseToggle", true)]
        [field: SerializeField] public TextMeshProUGUI ContainerToggleButtonText { get; protected set; }
        /// Toggle buttons container
        [field: Tooltip("Toggle buttons container")]
        [field: FieldCondition("UseToggle", true, true)]
        [field: SerializeField] public GameObject ToggleButtonsContainer { get; protected set; }
        /// Mark as read button
        [field: Tooltip("Mark as read button")]
        [field: SerializeField] public GameObject MarkAsReadButton { get; protected set; }

        [field: Header("Other Settings")]

        /// If true, set email buttons' navigation mode to explicit, and set their onSelectUp/Down objects
        [field: Tooltip("If true, set email buttons' navigation mode to explicit, and set their onSelectUp/Down objects")]
        [field: SerializeField] public bool ExplicitNavigation { get; protected set; }

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
            this.MMEventStartListening<EmailEvent>();
        }

        protected virtual void Start() {
            if (TargetInventory == null) {
                Debug.LogError("The " + name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventory.name + "), or set that TargetInventoryName to one that exists.");
                return;
            }
            EmailButtonsContainer.SetActive(false);
            MarkAsReadButton.SetActive(false);
            EmailDetailsDisplay.SetActive(false);
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

        public virtual void SetupButtonsFromList(Dictionary<string, EmailItem> emailItems) {
            foreach (KeyValuePair<string, EmailItem> entry in emailItems) {
                SetupButton(entry.Value, false);
            }
            // Arrange the child objects in order
            foreach (KeyValuePair<string, Transform> child in _unreadDocumentButtons) {
                child.Value.SetAsLastSibling();
            }
            foreach (KeyValuePair<string, Transform> child in _readDocumentButtons) {
                child.Value.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Creates and sets up a button from the given DocumentClass item
        /// Used when a document is added to the inventory
        /// </summary>
        public virtual void SetupButton(EmailItem item, bool reorder = true) {
            if (TargetInventory == null) {
                Debug.LogError("The " + name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventory.name + "), or set that TargetInventoryName to one that exists.");
                return;
            }

            if (TargetInventory.EmailItems.ContainsKey(item.ItemID) 
                && !(_readDocumentButtons.ContainsKey(item.ItemID) || _unreadDocumentButtons.ContainsKey(item.ItemID))
                ) {
                // Instantiate the button and set its size
                GameObject newButton;
                if (item.Read) {
                    newButton = InitializeDocumentButton(item, ReadContainerContent.transform);    // The new button's gameobject parent should be the unread or read container
                    _readDocumentButtons.Add(newButton.name, newButton.transform);
                }
                else {
                    newButton = InitializeDocumentButton(item, UnreadContainerContent.transform);    // The new button's gameobject parent should be the unread or read container
                    _unreadDocumentButtons.Add(newButton.name, newButton.transform);
                }
                // After instantiating the new button, the document buttons need to be rearranged in alphabetical order
                if (reorder) {
                    // Arrange the child objects in order
                    foreach (KeyValuePair<string, Transform> child in _unreadDocumentButtons) {
                        child.Value.SetAsLastSibling();
                    }
                }
            }

        }

        /// <summary>
        /// Initializes a document's button in the display
        /// </summary>
        /// <param name="documentItem"></param>
        public virtual GameObject InitializeDocumentButton(EmailItem item, Transform parentTransform) {
            GameObject buttonObject = Instantiate(EmailButton, parentTransform);
            //EmailButtonControl control = buttonObject.GetComponent<EmailButtonControl>();
            //control.SetItem(item);

            buttonObject.name = item.ItemID;
            if (UseTwoTexts) {
                buttonObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.Sender;
                buttonObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = item.SubjectLine;
            }
            else {
                _stringBuilder.Length = 0;
                _stringBuilder.Append(item.Sender);
                _stringBuilder.Append("\n");
                _stringBuilder.Append(item.SubjectLine);
                buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = _stringBuilder.ToString();
            }
            // Set the button's onClick
            Button buttonComponent = buttonObject.GetComponent<Button>();
            if (buttonComponent == null) {
                buttonComponent = buttonObject.GetComponentInChildren<Button>();
            }
            buttonComponent.onClick.AddListener(delegate { Click(buttonObject); });
            if (SeparateUnreadIcon) {
                //buttonComponent.gameObject.GetComponent<Image>().color = ReadColor;
            }
            return buttonObject;
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
            //button.GetComponent<Image>().color = UnreadColor;
        }

        /// <summary>
        /// Method called when an email's button is clicked
        /// Displays the email's details
        /// </summary>
        /// <param name="documentItem"></param>
        public virtual void Click(GameObject button) {
            //EmailItem emailItem = button.GetComponent<EmailButtonControl>().emailItem;
            EmailItem emailItem = TargetInventory.EmailItems[button.name];

            //if (!emailItem.Read) {
            //    if (SeparateUnreadIcon) {
            //        button.transform.Find("UnreadIcon").gameObject.SetActive(false);
            //    }
            //    else {
            //        button.GetComponent<Image>().color = ReadColor;
            //    }
            //}

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
            //_currentButton.GetComponent<EmailButtonControl>().MarkAsRead();
            _currentButton.transform.SetParent(ReadContainerContent.transform);
            _unreadDocumentButtons.Remove(_currentButton.name);
            _readDocumentButtons.Add(_currentButton.name, _currentButton.transform);
            MarkAsReadButton.SetActive(false);
            EmailDetailsDisplay.SetActive(false);

            UpdateButtons();

            //EmailEvent.Trigger(EmailEventType.Read, _currentButton.GetComponent<EmailButtonControl>().emailItem, PlayerID);
            EmailEvent.Trigger(EmailEventType.Read, TargetInventory.EmailItems[_currentButton.name], PlayerID);
            _currentButton = null;
            _currentItem = null;
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
            if (ExplicitNavigation) {
                if (ReadContainerContent.TryGetComponent(out VerticalScroller vs)) {
                    vs.SetNavigation();
                }
                if (UnreadContainerContent.TryGetComponent(out vs)) {
                    vs.SetNavigation();
                }
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
            MarkAsReadButton.SetActive(false);
            if (ReadContainerContent.transform.childCount > 0) {
                EventSystem.current.SetSelectedGameObject(ReadContainerContent.transform.GetChild(0).gameObject, null);
            }
        }

        /// <summary>
        /// Activates unread container and buttons container
        /// </summary>
        public virtual void ActivateUnreadContainer() {
            ReadContainer.SetActive(false);
            UnreadContainer.SetActive(true);
            EmailButtonsContainer.SetActive(true);
            ToggleButtonsContainer.SetActive(false);
            MarkAsReadButton.SetActive(true);
            if (UnreadContainerContent.transform.childCount > 0) {
                EventSystem.current.SetSelectedGameObject(UnreadContainerContent.transform.GetChild(0).gameObject, null);
            }
        }

        /// <summary>
        /// Toggles the active state of the email buttons container
        /// </summary>
        public virtual void ActivateEmailButtonsContainer(bool active) {
            EmailButtonsContainer.SetActive(active);
        }

        /// <summary>
        /// Toggles the active state of the toggle buttons container
        /// </summary>
        /// <param name="active"></param>
        public virtual void ActivateToggleButtonsContainer(bool active) {
            ToggleButtonsContainer.SetActive(active);
            if (active) {
                if (ToggleButtonsContainer.transform.childCount > 0) {
                    EventSystem.current.SetSelectedGameObject(ToggleButtonsContainer.transform.GetChild(0).gameObject, null);
                }
                EmailDetailsDisplay.SetActive(false);
            }
        }

        public virtual void OnMMEvent(EmailEvent emailEvent) {
            if (emailEvent.EventType == EmailEventType.Add) {
                SetupButton(emailEvent.Item);
            }
            if (emailEvent.EventType == EmailEventType.Update) {
                UpdateDisplay(emailEvent.Item);
            }
            if (emailEvent.EventType == EmailEventType.TriggerLoad) {
                SetupButtonsFromList(emailEvent.EmailItems);
            }
        }

        /// <summary>
        /// On enable, we start listening for EmailEvents.
        /// </summary>
        protected virtual void OnEnable() {
            //this.MMEventStartListening<EmailEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for EmailEvents.
        /// </summary>
        protected virtual void OnDisable() {
            //this.MMEventStopListening<EmailEvent>();
            _currentButton = null;
            _currentItem = null;
        }

        protected virtual void OnDestroy() {
            this.MMEventStopListening<EmailEvent>();
        }
    }
}
