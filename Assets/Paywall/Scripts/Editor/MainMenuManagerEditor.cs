using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Paywall.Tools;

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
                        menu.SetActiveIfNotNull(false);
                    }
                }

                mainMenuManager.MainCanvas.SetActiveIfNotNull(true);
            }
            // Turn dialogue canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Dialogue Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.DialogueCanvas) {
                        menu.SetActiveIfNotNull(false);
                    }
                }

                mainMenuManager.DialogueCanvas.SetActiveIfNotNull(true);
            }
            // Turn inventory canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Inventory Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.InventoryCanvas) {
                        menu.SetActiveIfNotNull(false);
                    }
                }

                mainMenuManager.InventoryCanvas.SetActiveIfNotNull(true);
            }
            // Turn store canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Store Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.StoreCanvas) {
                        menu.SetActiveIfNotNull(false);
                    }
                }

                mainMenuManager.StoreCanvas.SetActiveIfNotNull(true);
            }
            // Turn email canvas on and all others off
            if (GUILayout.Button(new GUIContent("Toggle Email Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu != mainMenuManager.EmailCanvas) {
                        menu.SetActiveIfNotNull(false);
                    }
                }

                mainMenuManager.EmailCanvas.SetActiveIfNotNull(true);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

        }

    }
}
