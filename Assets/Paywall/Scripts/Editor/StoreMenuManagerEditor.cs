using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Paywall {

    [CustomEditor(typeof(StoreMenuManager), true)]
    public class StoreMenuManagerEditor : Editor {

        public StoreMenuManager storeMenuManager {
            get {
                return (StoreMenuManager)target;
            }
        }

        protected virtual void GetUpgrades() {
            //var upgradeButtons = FindObjectsByType(typeof(UpgradeButton), FindObjectsInactive.Include, FindObjectsSortMode.None);
            UpgradeButton[] upgradeButtons = FindObjectsByType<UpgradeButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<ScriptableUpgrade> upgrades = new List<ScriptableUpgrade>();
            foreach (UpgradeButton button in upgradeButtons) {
                upgrades.Add(button.Upgrade);
            }
            storeMenuManager.SetUpgrades(upgrades);
        }

        protected virtual void ClearUpgrades() {
            storeMenuManager.SetUpgrades(null);
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
