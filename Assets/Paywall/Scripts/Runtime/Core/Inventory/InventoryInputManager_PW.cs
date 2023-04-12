using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Paywall {

    public class InventoryInputManager_PW : MonoBehaviour {
		[field: Header("Targets")]
		[field: MMInformation("Bind here your inventory container (the CanvasGroup that you want to turn on/off when opening/closing the inventory), your main InventoryDisplay, and the overlay that will be displayed under the InventoryDisplay when opened.", MMInformationAttribute.InformationType.Info, false)]
		/// The CanvasGroup containing all the elements you want to show/hide when pressing the open/close inventory button
		[field: SerializeField] public CanvasGroup TargetInventoryContainer { get; protected set; }
		/// The main inventory display 
		[field:Tooltip("The main inventory display ")]
		[field: SerializeField] public InventoryDisplay TargetInventoryDisplay { get; protected set; }
		/// The Fader that will be used under it when opening/closing the inventory
		[field: Tooltip("The Fader that will be used under it when opening/closing the inventory")]
		[field: SerializeField] public CanvasGroup Overlay { get; protected set; }

		[field: Header("Start Behaviour")]
		[field: MMInformation("If you set HideContainerOnStart to true, the TargetInventoryContainer defined right above this field will be automatically hidden on Start, even if you've left it visible in Scene view. Useful for setup.", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		[field: SerializeField] public bool HideContainerOnStart = true;

		[field: Header("Permissions")]
		[field: MMInformation("Here you can decide to have your inventory catch input only when open, or not.", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		[field: SerializeField] public bool InputOnlyWhenOpen = true;

		[field: Header("Close Bindings")]
		/// a list of other inventories that should get force-closed when this one opens
		[field: SerializeField] public List<string> CloseList;

		/// returns the active slot
		public InventorySlot CurrentlySelectedInventorySlot { get; set; }

		[Header("State")]
		/// if this is true, the associated inventory is open, closed otherwise
		[MMReadOnly]
		public bool InventoryIsOpen;

		protected CanvasGroup _canvasGroup;
		protected GameObject _currentSelection;
		protected InventorySlot _currentInventorySlot;
		protected List<InventoryHotbar> _targetInventoryHotbars;
		protected InventoryDisplay _currentInventoryDisplay;

		protected bool _toggleInventoryKeyPressed;
		protected bool _openInventoryKeyPressed;
		protected bool _closeInventoryKeyPressed;
		protected bool _cancelKeyPressed;
		protected bool _prevInvKeyPressed;
		protected bool _nextInvKeyPressed;
		protected bool _moveKeyPressed;
		protected bool _equipOrUseKeyPressed;
		protected bool _equipKeyPressed;
		protected bool _useKeyPressed;
		protected bool _dropKeyPressed;
		protected bool _hotbarInputPressed = false;

		/// <summary>
		/// On start, we grab references and prepare our hotbar list
		/// </summary>
		protected virtual void Start() {
			_currentInventoryDisplay = TargetInventoryDisplay;
			InventoryIsOpen = false;
			_targetInventoryHotbars = new List<InventoryHotbar>();
			_canvasGroup = GetComponent<CanvasGroup>();
			foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[]) {
				_targetInventoryHotbars.Add(go);
			}
			if (HideContainerOnStart) {
				if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
				if (Overlay != null) { Overlay.alpha = 0; }
				EventSystem.current.sendNavigationEvents = false;
				if (_canvasGroup != null) {
					_canvasGroup.blocksRaycasts = false;
				}
			}
		}


	}
}
