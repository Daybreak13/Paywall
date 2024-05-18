using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Paywall.Editors {

    [CustomEditor(typeof(LevelSegmentController), true)]
    [CanEditMultipleObjects]
    public class LevelSegmentControllerEditor : Editor {
        public LevelSegmentController levelSegmentController {
            get {
                return (LevelSegmentController)target;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            GUILayout.Label("Controls");
            EditorGUILayout.Space();

            if (GUILayout.Button("Get Spawn Points")) {
                levelSegmentController.SetSpawnPoints(levelSegmentController.gameObject.GetComponentsInChildren<SpawnPoint>().ToList());
            }

            if (GUILayout.Button("Clear Spawn Points")) {
                levelSegmentController.SetSpawnPoints(null);
            }

            if (levelSegmentController.BoundsLine == null) {
                levelSegmentController.GetBounds();
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                //SceneView.RepaintAll();
            }
        }


    }
}
