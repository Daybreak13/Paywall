using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.UI;

namespace Paywall {

	/// <summary>
	/// Manages the email details display
	/// </summary>
    public class EmailDetailsDisplay : MonoBehaviour {

		/// the reference inventory from which we'll display item details
		[field:MMInformation("Specify here the name of the inventory whose content's details you want to display in this Details panel. You can also decide to make it global. If you do so, it'll display the details of all items, regardless of their inventory.", MMInformationAttribute.InformationType.Info, false)]
		[field: SerializeField] public string PlayerID { get; protected set; } = "Player1";
		/// if you make this panel global, it'll ignore 
		[field: SerializeField] public bool Global { get; protected set; } = false;
		/// whether the details are currently hidden or not 
		public bool Hidden { get; protected set; }

		[field:Header("Components")]
		[field:MMInformation("Here you need to bind the panel components.", MMInformationAttribute.InformationType.Info, false)]
		/// the title text component
		[Tooltip("the title text component")]
		[field: SerializeField] public TextMeshProUGUI Title { get; protected set; }
		/// the subject line text component
		[Tooltip("the subject line text component")]
		[field: SerializeField] public TextMeshProUGUI SubjectLine { get; protected set; }
		/// the sender text component
		[Tooltip("the sender text component")]
		[field: SerializeField] public TextMeshProUGUI Sender { get; protected set; }
		/// the details text component
		[Tooltip("the details text component")]
		[field: SerializeField] public TextMeshProUGUI Details { get; protected set; }
		/// the image container object
		[Tooltip("the image container object")]
		[field: SerializeField] public Image Picture { get; protected set; }
		/// the description container object
		[Tooltip("the description container object")]
		[field: SerializeField] public GameObject DescriptionContainer { get; protected set; }

		[field:Header("Document Item")]
		[field:MMInformation("The currently displayed document item.", MMInformationAttribute.InformationType.Info, false)]
		/// the currently displayed email item
		[Tooltip("the currently displayed email item")]
		[field: SerializeField] public EmailItem emailItem { get; protected set; }

		protected float _fadeDelay = 0.2f;
		protected CanvasGroup _canvasGroup;

		/// <summary>
		/// Display the given DocumentItem.
		/// </summary>
		/// <param name="item"></param>
		public virtual void Display(EmailItem item) {
			Title.text = item.ItemName;
			if (item.Picture == null) {
				Picture.sprite = null;
			}
			else {
				Picture.sprite = item.Picture;
			}
			Sender.text = item.Sender;
			Details.text = item.Details;
			SubjectLine.text = item.SubjectLine;
			emailItem = item;

			if (item.Picture == null) {

			}
		}
	}
}
