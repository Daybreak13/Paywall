using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

	/// <summary>
	/// Document event types to trigger or listen to. Occurs during interactions with document items, or the inventory menu
	/// </summary>
	public enum EmailEventType { Pick, Add, Update, Read, Destroy, ContentChanged, Error, MenuOpen, MenuCloseRequest, MenuClose, ShowDetails, Trigger, NotificationOpen, NotificationClose }

	/// <summary>
	/// A document event (Pick, Read, Destroy, ContentChanged, Error, MenuOpen, MenuCloseRequest, MenuClose)
	/// </summary>
	public struct EmailEvent {
		public EmailEventType EventType;
		public EmailItem Item;
		public string PlayerID;

		public EmailEvent(EmailEventType eventType, EmailItem item, string playerID) {
			EventType = eventType;
			Item = item;
			PlayerID = (playerID != "") ? playerID : "Player1";
		}

		static EmailEvent e;

		public static void Trigger(EmailEventType eventType, EmailItem item, string playerID) {
			e.EventType = eventType;
			e.Item = item;
			e.PlayerID = (playerID != "") ? playerID : "Player1";
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
