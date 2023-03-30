using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall.Editors {

    /// <summary>
    /// Custom editor for the MainMenuManager
    /// Includes buttons to toggle canvases on and off
    /// </summary>
    [CustomEditor(typeof(MainMenuManager), true)]
    public class MainMenuManagerEditor : Editor {
        protected List<GameObject> _menus = new List<GameObject>();

        public MainMenuManager mainMenuManager {
            get {
                return (MainMenuManager)target;
            }
        }

        protected virtual void InitializeList() {
            if (mainMenuManager.MainCanvas != null) _menus.Add(mainMenuManager.MainCanvas);
            if (mainMenuManager.InventoryCanvas != null) _menus.Add(mainMenuManager.InventoryCanvas);
            if (mainMenuManager.DialogueCanvas != null) _menus.Add(mainMenuManager.DialogueCanvas);
            if (mainMenuManager.StoreCanvas != null) _menus.Add(mainMenuManager.StoreCanvas);
            if (mainMenuManager.EmailCanvas != null) _menus.Add(mainMenuManager.EmailCanvas);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            if (_menus.Count == 0) {
                InitializeList();
            }

            GUILayout.Label("Menu Toggles");
            EditorGUILayout.Space();

            // Turn main canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Main Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.MainCanvas) {
                        menu.gameObject.SetActive(false);
                    }
                }

                mainMenuManager.MainCanvas.SetActive(true);
            }
            // Turn dialogue canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Dialogue Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.DialogueCanvas) {
                        menu.gameObject.SetActive(false);
                    }
                }

                mainMenuManager.DialogueCanvas.SetActive(true);
            }
            // Turn inventory canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Inventory Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.InventoryCanvas) {
                        menu.gameObject.SetActive(false);
                    }
                }

                mainMenuManager.InventoryCanvas.SetActive(true);
            }
            // Turn store canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Store Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.StoreCanvas) {
                        menu.gameObject.SetActive(false);
                    }
                }

                mainMenuManager.StoreCanvas.SetActive(true);
            }
            // Turn email canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Email Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.EmailCanvas) {
                        menu.gameObject.SetActive(false);
                    }
                }

                mainMenuManager.EmailCanvas.SetActive(true);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

        }

    }
}
