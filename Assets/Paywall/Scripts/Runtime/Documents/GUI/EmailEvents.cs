using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall.Documents {

	/// <summary>
	/// Document event types to trigger or listen to. Occurs during interactions with document items, or the inventory menu
	/// Pick: Triggered when an item is picked, prompting the Inventory to add it
	/// Add/Update: Triggered by Inventory when an item is added or changed, prompting the display to update
	/// TriggerLoad: Triggered by PaywallProgressManager on a load, to load the save's inventory data
	/// ContentChanged: Triggered by Inventory after an item is added/updated, for the PaywallProgressManager to save the new data
	/// </summary>
	public enum EmailEventType { Pick, Add, Update, Read, TriggerLoad, Destroy, ContentChanged, Error, MenuOpen, MenuCloseRequest, MenuClose, ShowDetails, Trigger, NotificationOpen, NotificationClose }

	/// <summary>
	/// A document event (Pick, Read, Destroy, ContentChanged, Error, MenuOpen, MenuCloseRequest, MenuClose)
	/// </summary>
	public struct EmailEvent {
		public EmailEventType EventType;
		public EmailItem Item;
		public string PlayerID;
		public Dictionary<string, EmailItem> EmailItems;
		public EmailItemScriptable ItemScriptable;

		public EmailEvent(EmailEventType eventType, EmailItem item, string playerID = "", Dictionary<string, EmailItem> emailItems = null, EmailItemScriptable itemScriptable = null) {
			EventType = eventType;
			Item = item;
			PlayerID = (playerID != "") ? playerID : "Player1";
			EmailItems = emailItems;
			ItemScriptable = itemScriptable;
		}

		static EmailEvent e;

		public static void Trigger(EmailEventType eventType, EmailItem item, string playerID = "", Dictionary<string, EmailItem> emailItems = null, EmailItemScriptable itemScriptable = null) {
			e.EventType = eventType;
			e.Item = item;
			e.PlayerID = (playerID != "") ? playerID : "Player1";
			e.EmailItems = emailItems;
			e.ItemScriptable = itemScriptable;
			if (eventType == EmailEventType.MenuOpen) {
				MMGameEvent.Trigger("agInventoryOpen");
			}
			if (eventType == EmailEventType.MenuClose) {
				MMGameEvent.Trigger("agInventoryClose");
			}
			MMEventManager.TriggerEvent(e);
		}

	}
}
