using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using System;
using UnityEngine.UI;

namespace Paywall.Documents {

	/// <summary>
	/// Document item asset
	/// </summary>
	[Serializable]
	public class EmailItemScriptable : ScriptableObject {

		[field: Header("ID and Target")]
		/// the (unique) ID of the item
		[field: MMInformation("The unique name of your object.", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string ItemID { get; protected set; } 

		/// the inventory name into which this item will be stored
		[field: SerializeField] public string TargetInventoryName { get; protected set; } = "MainEmailInventory";

		[field: Header("Basic info")]
		[field: TextArea]
		/// The email's subject line
		[field: MMInformation("The email's sender", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string Sender { get; protected set; }
		[field: TextArea]
		/// The email's subject line
		[field: MMInformation("The email's subject line", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string SubjectLine { get; protected set; }
		[field: TextArea]
		[field: Tooltip("The document's text body")]
		/// the document's text body
		[field: SerializeField] public string Details { get; protected set; }
		/// Has the document been read
		[field: Tooltip("Has the document been read")]
		[field: MMReadOnly]
		[field: SerializeField] public bool Read { get; protected set; }
		/// Is the document starred (indicates importance)
		[field: Tooltip("Is the document starred (indicates importance)")]
		[field: SerializeField] public bool Starred { get; protected set; }

		[field: Header("Image and Button")]
		/// the image that will be shown at the top of the document (if applicable)
		[field: MMInformation("The image that will be shown at the top of the document (if applicable).", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public Sprite Picture { get; protected set; }
		/// The special button in this email (if applicable)
		[field: MMInformation("The special button in this email (if applicable).", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public Button ButtonComponent { get; protected set; }

		[field: Header("Metadata")]
		/// Version number, for documents that can be updated
		[field: Tooltip("Version number, for documents that can be updated")]
		[field: SerializeField] public int Version { get; protected set; }

		public virtual void SetRead(bool read) {
			Read = read;
        }

		/// <summary>
		/// Determines if an item is null or not
		/// </summary>
		/// <returns><c>true</c> if is null the specified item; otherwise, <c>false</c>.</returns>
		/// <param name="item">Item.</param>
		public static bool IsNull(EmailItem item) {
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
