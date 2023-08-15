using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall.Editors {

    /// <summary>
    /// Custom editor for the UICameraCanvasManager
    /// Includes buttons to toggle canvases on and off
    /// </summary>
    [CustomEditor(typeof(UICameraCanvasManager))]
    public class UICameraCanvasManagerEditor : Editor {
        protected List<GameObject> _menus = new List<GameObject>();

        public UICameraCanvasManager uICameraCanvasManager {
            get {
                return (UICameraCanvasManager)target;
            }
        }

        protected virtual void InitializeList() {
            if (uICameraCanvasManager.MainCanvas != null) _menus.Add(uICameraCanvasManager.MainCanvas);
            if (uICameraCanvasManager.SystemCanvas != null) _menus.Add(uICameraCanvasManager.SystemCanvas);
            if (uICameraCanvasManager.InventoryCanvas != null) _menus.Add(uICameraCanvasManager.InventoryCanvas);
            if (uICameraCanvasManager.DialogueCanvas != null) _menus.Add(uICameraCanvasManager.DialogueCanvas);
            if (uICameraCanvasManager.SupplyDepotMenuCanvas != null) _menus.Add(uICameraCanvasManager.SupplyDepotMenuCanvas);
            if (uICameraCanvasManager.GameOverCanvas != null) _menus.Add(uICameraCanvasManager.GameOverCanvas);
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
            if (GUILayout.Button(new GUIContent("Toggle Main Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu == uICameraCanvasManager.MainCanvas) {
                        menu.SetActive(true);
                    } else {
                        menu.SetActive(false);
                    }
                }
            }
			if (GUILayout.Button(new GUIContent("Toggle System Canvas"))) {
				foreach (GameObject menu in _menus) {
					if (menu == uICameraCanvasManager.SystemCanvas) {
						menu.SetActive(true);
					} else {
                        menu.SetActive(false);
                    }
				}

				uICameraCanvasManager.SystemCanvas?.SetActive(true);
			}
			if (GUILayout.Button(new GUIContent("Toggle Inventory Canvas"))) {
				foreach (GameObject menu in _menus) {
					if (menu == uICameraCanvasManager.InventoryCanvas) {
						menu.SetActive(true);
					} else {
                        menu.SetActive(false);
                    }
				}
			}
			if (GUILayout.Button(new GUIContent("Toggle Dialogue Canvas"))) {
				foreach (GameObject menu in _menus) {
					if (menu == uICameraCanvasManager.DialogueCanvas) {
                        menu.SetActive(true);
                    } else {
                        menu.SetActive(false);
                    }
				}
			}
            if (GUILayout.Button(new GUIContent("Toggle Supply Depot Menu Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu == uICameraCanvasManager.SupplyDepotMenuCanvas) {
                        menu.SetActive(true);
                    }
                    else {
                        menu.SetActive(false);
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Toggle Game Over Canvas"))) {
                foreach (GameObject menu in _menus) {
                    if (menu == uICameraCanvasManager.GameOverCanvas) {
                        menu.SetActive(true);
                    } else {
                        menu.SetActive(false);
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Reactivate All"))) {
                foreach (GameObject menu in _menus) {
                    menu.SetActive(true);
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

        }

    }

}

