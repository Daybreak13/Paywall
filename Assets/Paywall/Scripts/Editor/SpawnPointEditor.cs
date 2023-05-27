using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Paywall;

namespace Paywall.Editors {
    
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : Editor {
        public SpawnPoint spawnPoint {
            get {
                return (SpawnPoint)target;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

        }

    }
}
