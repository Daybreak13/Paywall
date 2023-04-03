using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.UI;
using System;

namespace Paywall.Documents {

	[Serializable]
    public class EmailItem {
		[field: Header("ID and Target")]
		/// the (unique) ID of the item
		[field: MMInformation("The unique name of your object.", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string ItemID;

		/// the inventory name into which this item will be stored
		[field: SerializeField] public string TargetInventoryName = "MainEmailInventory";

		[field: Header("Basic info")]
		[field: TextArea]
		/// The email's subject line
		[field: MMInformation("The email's sender", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string Sender;
		[field: TextArea]
		/// The email's subject line
		[field: MMInformation("The email's subject line", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string SubjectLine;
		[field: TextArea]
		[field: Tooltip("The document's text body")]
		/// the document's text body
		[field: SerializeField] public string Details;
		/// Has the document been read
		[field: Tooltip("Has the document been read")]
		[field: MMReadOnly]
		[field: SerializeField] public bool Read;
		/// Is the document starred (indicates importance)
		[field: Tooltip("Is the document starred (indicates importance)")]
		[field: SerializeField] public bool Starred;

		[field: Header("Metadata")]
		/// Version number, for documents that can be updated
		[field: Tooltip("Version number, for documents that can be updated")]
		[field: SerializeField] public int Version;

		public EmailItem() {

        }

		public EmailItem(EmailItemScriptable s) {
			ConvertToClass(s);
        }

		public virtual void SetRead(bool read) {
			Read = read;
		}

		public virtual EmailItem ConvertToClass(EmailItemScriptable s) {
			ItemID = s.ItemID;
			TargetInventoryName = s.TargetInventoryName;
			Sender = s.Sender;
			SubjectLine = s.SubjectLine;
			Details = s.Details;
			Read = s.Read;
			Starred = s.Starred;
			Version = s.Version;
			return this;
        }

		/// <summary>
		/// Determines if an item is null or not
		/// </summary>
		/// <returns><c>true</c> if is null the specified item; otherwise, <c>false</c>.</returns>
		/// <param name="item">Item.</param>
		public bool IsNull(EmailItem item) {
			if (item == null) {
				return true;
			}
			if (item.ItemID == null) {
				return true;
			}
			if (item.ItemID == string.Empty) {
				return true;
			}
			return false;
		}
	}
}
