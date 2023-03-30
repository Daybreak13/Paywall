using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class UICameraCanvasManager : MMSingleton<UICameraCanvasManager> {
        [Header("Canvases")]

        /// The main canvas
        [Tooltip("The main canvas")]
        public GameObject MainCanvas;
        /// The system canvas
        [Tooltip("The system canvas")]
        public GameObject SystemCanvas;
        /// The inventory canvas
        [Tooltip("The inventory canvas")]
        public GameObject InventoryCanvas;
        /// The dialogue canvas
        [Tooltip("The dialogue canvas")]
        public GameObject DialogueCanvas;
        /// The game over canvas
        [Tooltip("The game over canvas")]
        public GameObject GameOverCanvas;

        /// If true, turn certain canvases back on at start, and the rest off. Useful for debugging
        [Tooltip("If true, turn certain canvases back on at start, and the rest off. Useful for debugging")]
        public bool ToggleCanvasOnStart = true;

        public List<GameObject> MenuList { get { return _menuList; } }
        protected List<GameObject> _menuList = new List<GameObject>();

        /// <summary>
        /// On start, hide containers if applicable and initialize our list of menus.
        /// </summary>
        protected override void Awake() {
            base.Awake();

            if (MainCanvas != null) {
                _menuList.Add(MainCanvas);
            }
            if (SystemCanvas != null) {
                _menuList.Add(SystemCanvas);
            }
            if (InventoryCanvas != null) {
                _menuList.Add(InventoryCanvas);
            }
            if (DialogueCanvas != null) {
                _menuList.Add(DialogueCanvas);
            }
            if (GameOverCanvas != null) {
                _menuList.Add(GameOverCanvas);
            }

            if (ToggleCanvasOnStart) {
                /*
                if (MainCanvas != null) MainCanvas.SetActive(true);
                if (DialogueCanvas != null) DialogueCanvas.SetActive(false);
                if (InventoryCanvas != null) InventoryCanvas.SetActive(false);
                if (GameOverCanvas != null) GameOverCanvas.SetActive(false);
                if (SystemCanvas != null) SystemCanvas.SetActive(false);
                */
                foreach (GameObject menu in _menuList) {
                    if (menu == MainCanvas) {
                        menu.SetActive(true);
                    } else {
                        menu.SetActive(false);
                    }
                }
            }

        }

    }
}
