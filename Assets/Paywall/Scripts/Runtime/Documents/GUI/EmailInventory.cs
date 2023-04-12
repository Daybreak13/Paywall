using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.Tools;

namespace Paywall.Documents {

    /// <summary>
    /// Stores the information regarding the email inventory. Can save it in memory for save/load games.
    /// </summary>
    [Serializable]
    public class EmailInventory : MonoBehaviour, MMEventListener<EmailEvent>, MMEventListener<MMGameEvent> {

        [field:Header("Player ID")]
        /// a unique ID used to identify the owner of this inventory
        [field: Tooltip("a unique ID used to identify the owner of this inventory")]
        [field: SerializeField] public string PlayerID { get; protected set; } = "Player1";

        /// the complete list of inventory items in this inventory
        [field: Tooltip("This is a realtime view of your Inventory's contents. Don't modify this list via the inspector, it's visible for control purposes only.")]
        [field: SerializeField] public List<EmailItem> Content { get; protected set; } = new List<EmailItem>();
        /// Scriptable email dictionary
        [field: Tooltip("Scriptable email dictionary")]
        [field: SerializeField] public EmailDictionary emailDictionary { get; protected set; }

        [field:Header("Persistency")]
        [field: Tooltip("Here you can define whether or not this inventory should respond to Load and Save events. If you don't want to have your inventory saved to disk, set this to false. You can also have it reset on start, to make sure it's always empty at the start of this level.")]
        /// whether this inventory will be saved and loaded
        [field:SerializeField ] public bool Persistent { get; protected set; } = true;
        /// whether or not this inventory should be reset on start
        [field: Tooltip("whether or not this inventory should be reset on start")]
        public bool ResetThisInventorySaveOnStart = false;

        [field:Header("Debug")]
        /// If true, will draw the contents of the inventory in its inspector
        [field: Tooltip("The Inventory component is like the database and controller part of your inventory. It won't show anything on screen, you'll need also an InventoryDisplay for that. Here you can decide whether or not you want to output a debug content in the inspector (useful for debugging).")]
        [field: SerializeField] public bool DrawContentInInspector { get; protected set; } = false;

        public Dictionary<string, EmailItem> EmailItems { get { return emailDictionary.EmailItems; } protected set { } }

        public const string _resourceItemPath = "Emails/";
        protected const string _saveFolderName = "EmailInventory/";
        protected const string _saveFileExtension = ".emailinventory";

        protected virtual void Start() {
            //EmailItems = emailDictionary.EmailItems;
        }

        public virtual void SetDictionary(EmailDictionary dict) {
            emailDictionary = dict;
        }

        /// <summary>
        /// Adds an EmailItem to the inventory. EmailItems are sorted alphabetically by ItemName.
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, item already added).</returns>
        public virtual bool AddItem(EmailItem itemToAdd) {

            // if the item to add is null, we do nothing and exit
            if (itemToAdd == null) {
                Debug.LogWarning(name + " : The item you want to add to the inventory is null");
                return false;
            }

            if (!EmailItems.ContainsKey(itemToAdd.ItemID)) {
                Content.Add(itemToAdd);
                EmailItems.Add(itemToAdd.ItemID, itemToAdd);
                EmailEvent.Trigger(EmailEventType.Add, itemToAdd, PlayerID);
                return true;
            } 
            else if (EmailItems[itemToAdd.ItemID].Version < itemToAdd.Version) {
                EmailEvent.Trigger(EmailEventType.Update, itemToAdd, PlayerID);
            }
            EmailEvent.Trigger(EmailEventType.ContentChanged, null, PlayerID, EmailItems);

            return false;
        }

        protected virtual void ReadItem(EmailItem item) {
            if (EmailItems.ContainsKey(item.ItemID)) {
                EmailItems[item.ItemID].SetRead(true);
                EmailEvent.Trigger(EmailEventType.ContentChanged, null, PlayerID, EmailItems);
            }
        }

        public virtual bool ReplaceItem(EmailItem item) {

            return false;
        }

        /// <summary>
        /// Return true if document item has been read
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public virtual bool IsItemRead(string itemID) {
            foreach (EmailItem entry in Content) {
                if (entry.ItemID == itemID) {
                    if (entry.Read) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }

            return false;
        }

        public virtual bool DestroyDocument() {
            return false;
        }

        public virtual void SaveInventory() {

        }

        public virtual void LoadSavedInventory() {

        }

        public virtual void ResetSavedInventory() {

        }

        public void OnMMEvent(EmailEvent emailEvent) {
            if (emailEvent.EventType == EmailEventType.Pick) {
                if (emailEvent.Item != null) {
                    AddItem(emailEvent.Item);
                }
                if (emailEvent.ItemScriptable != null) {
                    EmailItem emailItem = new EmailItem(emailEvent.ItemScriptable);
                    AddItem(emailItem);
                }
            }
            if (emailEvent.EventType == EmailEventType.Read) {
                ReadItem(emailEvent.Item);
            }
        }

        /// <summary>
        /// When we catch an MMGameEvent, we do stuff based on its name
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent) {
            if ((gameEvent.EventName == "Save") && Persistent) {
                //SaveInventory();
            }
            if ((gameEvent.EventName == "Load") && Persistent) {
                if (ResetThisInventorySaveOnStart) {
                    ResetSavedInventory();
                }
                //LoadSavedInventory();
            }
        }

        /// <summary>
        /// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
        /// </summary>
        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<EmailEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
        /// </summary>
        protected virtual void OnDisable() {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<EmailEvent>();
        }
    }
}
