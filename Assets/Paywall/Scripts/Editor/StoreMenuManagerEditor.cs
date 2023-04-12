using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Paywall {

    [CustomEditor(typeof(StoreMenuManager), true)]
    public class StoreMenuManagerEditor : Editor {

        protected StoreMenuManager _storeMenuManager {
            get {
                return (StoreMenuManager)target;
            }
        }

        protected virtual void GetUpgrades() {
            UpgradeButton[] upgradeButtons = FindObjectsByType<UpgradeButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<UpgradeButton> buttons = new();
            foreach (UpgradeButton button in upgradeButtons) {
                buttons.Add(button);
            }
            _storeMenuManager.SetUpgradesEditor(buttons);
        }

        protected virtual void ClearUpgrades() {
            _storeMenuManager.SetUpgradesEditor(null);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            GUILayout.Label("Controls");
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Get Upgrades"))) {
                GetUpgrades();
            }
            if (GUILayout.Button(new GUIContent("Clear Upgrades"))) {
                ClearUpgrades();
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

        }

    }
}
