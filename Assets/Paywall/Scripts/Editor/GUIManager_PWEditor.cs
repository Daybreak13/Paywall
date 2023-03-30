using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall.Editors {

    [CustomEditor(typeof(GUIManager_PW), true)]
    public class GUIManager_PWEditor : Editor {

        public GUIManager_PW gUIManager {
            get {
                return (GUIManager_PW)target;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { "PointsText", "LevelText" });

            serializedObject.ApplyModifiedProperties();
        }

    }
}
