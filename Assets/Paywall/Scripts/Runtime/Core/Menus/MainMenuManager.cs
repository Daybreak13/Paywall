using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;
using Paywall.Documents;

namespace Paywall {

    public enum CanvasTypes { Main, Dialogue, Inventory, Store, Email }

    public class MainMenuManager : MonoBehaviour {
        [field:Header("Canvases")]

        /// The main canvas
        [field: Tooltip("The main canvas")]
        [field:SerializeField] public GameObject MainCanvas { get; protected set; } 
        /// The dialogue canvas
        [field: Tooltip("The dialogue canvas")]
        [field: SerializeField] public GameObject DialogueCanvas { get; protected set; }
        /// The inventory canvas
        [field: Tooltip("The inventory canvas")]
        [field: SerializeField] public GameObject InventoryCanvas { get; protected set; }
        /// The store canvas
        [field: Tooltip("The store canvas")]
        [field: SerializeField] public GameObject StoreCanvas { get; protected set; }
        /// The email canvas
        [field: Tooltip("The email canvas")]
        [field: SerializeField] public GameObject EmailCanvas { get; protected set; }

        [field:Header("Settings")]

        /// If true, turn certain canvases back on at start, and the rest off. Useful for debugging
        [field: Tooltip("If true, turn certain canvases back on at start, and the rest off. Useful for debugging")]
        [field: SerializeField] public bool ToggleCanvasOnStart { get; protected set; } = true;
        [field: SerializeField] public bool UseCanvasGroup { get; protected set; } = false;

        public List<GameObject> MenuList { get { return _menuList; } }
        protected List<GameObject> _menuList = new List<GameObject>();

        /// <summary>
        /// On start, hide containers if applicable and initialize our list of menus.
        /// </summary>
        protected virtual void Awake() {

            _menuList.AddIfNotNull(MainCanvas);
            _menuList.AddIfNotNull(InventoryCanvas);
            _menuList.AddIfNotNull(DialogueCanvas);
            _menuList.AddIfNotNull(StoreCanvas);
            _menuList.AddIfNotNull(EmailCanvas);

            if (ToggleCanvasOnStart) {
                foreach (GameObject menu in _menuList) {
                    if (menu == MainCanvas) {
                        menu.SetActive(true);
                    } else {
                        menu.SetActive(false);
                    }
                }
            }

            if (UseCanvasGroup) {
                foreach(GameObject menu in _menuList) {
                    CanvasGroup cg = menu.GetComponentInChildren<CanvasGroup>();
                    if (menu == MainCanvas) {
                        cg.alpha = 1;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }
                    else {
                        cg.alpha = 0;
                        cg.interactable = false;
                        cg.blocksRaycasts = false;
                    }
                }
                foreach (GameObject menu in _menuList) {
                    menu.SetActive(true);
                }
            }
        }

        public virtual void ActivateMain() {
            ActivateCanvas(MainCanvas);
        }

        public virtual void ActivateDialogue() {
            ActivateCanvas(DialogueCanvas);
        }

        public virtual void ActivateInventory() {
            ActivateCanvas(InventoryCanvas);
        }

        public virtual void ActivateStore() {
            ActivateCanvas(StoreCanvas);
        }

        public virtual void ActivateEmail() {
            ActivateCanvas(EmailCanvas);
        }

        /// <summary>
        /// Activate the canvas corresponding to the given type
        /// </summary>
        /// <param name="canvasType"></param>
        public virtual void ActivateCanvas(CanvasTypes canvasType) {
            GameObject target = SelectCanvas(canvasType);
            
            if (target == null) {
                return;
            }

            if (UseCanvasGroup) {
                foreach (GameObject menu in _menuList) {
                    CanvasGroup cg = menu.GetComponentInChildren<CanvasGroup>();
                    if (menu == target) {
                        cg.alpha = 1;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }
                    else {
                        cg.alpha = 0;
                        cg.interactable = false;
                        cg.blocksRaycasts = false;
                    }
                }
            }
            else {
                foreach (GameObject menu in _menuList) {
                    if (menu == target) {
                        menu.SetActive(true);
                    }
                    else {
                        menu.SetActive(false);
                    }
                }
            }

        }

        /// <summary>
        /// Activate the given canvas gameobject
        /// </summary>
        /// <param name="canvasType"></param>
        protected virtual void ActivateCanvas(GameObject target) {
            if (target == null) {
                return;
            }

            if (UseCanvasGroup) {
                foreach (GameObject menu in _menuList) {
                    CanvasGroup cg = menu.GetComponentInChildren<CanvasGroup>();
                    if (menu == target) {
                        cg.alpha = 1;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }
                    else {
                        cg.alpha = 0;
                        cg.interactable = false;
                        cg.blocksRaycasts = false;
                    }
                }
            }
            else {
                foreach (GameObject menu in _menuList) {
                    if (menu == target) {
                        menu.SetActive(true);
                    }
                    else {
                        menu.SetActive(false);
                    }
                }
            }

        }

        /// <summary>
        /// Returns one of the canvases based on given canvas type
        /// </summary>
        /// <param name="canvasType"></param>
        /// <returns></returns>
        protected virtual GameObject SelectCanvas(CanvasTypes canvasType) {
            GameObject target = null;
            switch (canvasType) {
                case CanvasTypes.Main:
                    target = MainCanvas;
                    break;
                case CanvasTypes.Dialogue:
                    target = DialogueCanvas;
                    break;
                case CanvasTypes.Inventory:
                    target = InventoryCanvas;
                    break;
                case CanvasTypes.Store:
                    target = StoreCanvas;
                    break;
                case CanvasTypes.Email:
                    target = EmailCanvas;
                    break;
            }
            return target;
        }

    }
}
