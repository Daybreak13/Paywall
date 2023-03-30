using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System;
using UnityEngine.UI;

namespace Paywall {

	/// <summary>
	/// Document item asset
	/// </summary>
	[Serializable]
	public class EmailItem : ScriptableObject {

		[field: Header("ID and Target")]
		/// the (unique) ID of the item
		[field: MMInformation("The unique name of your object.", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string ItemID { get; protected set; } 
		/// the category the document will be grouped in
		[field: MMInformation("The category the document will be grouped in", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string CategoryID { get; protected set; }

		/// the inventory name into which this item will be stored
		[field: SerializeField] public string TargetInventoryName { get; protected set; } = "MainDocumentInventory";

		[field: Header("Basic info")]
		/// the name of the item - will be displayed in the details panel
		[field: MMInformation("The name of the item as you want it to appear in the display panel", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string ItemName { get; protected set; }
		/// The email's subject line
		[field: MMInformation("The email's sender", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string Sender { get; protected set; }
		/// The email's subject line
		[field: MMInformation("The email's subject line", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string SubjectLine { get; protected set; }
		[field:TextArea]
		[Tooltip("The document's text body")]
		/// the document's text body
		[field: SerializeField] public string Details { get; protected set; }
		/// Has the document been read
		[Tooltip("Has the document been read")]
		[field:MMReadOnly]
		[field: SerializeField] public bool Read { get; protected set; }

		[field: Header("Image")]
		/// the image that will be shown at the top of the document (if applicable)
		[field: MMInformation("The image that will be shown at the top of the document (if applicable).", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public Sprite Picture { get; protected set; }

		[field: Header("Metadata")]
		/// Version number, for documents that can be updated
		[Tooltip("Version number, for documents that can be updated")]
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
