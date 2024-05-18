using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall.Editors {

    [CustomEditor(typeof(ScriptableSegmentList))]
    public class SegmentListEditor : Editor {
        public ScriptableSegmentList SegmentsList {
            get {
                return (ScriptableSegmentList)target;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Sort")) {
                if (SegmentsList.Items != null &&  SegmentsList.Items.Count > 0) {
                    SegmentsList.Items.Sort((a, b) => string.Compare(a.Segment.SegmentName, b.Segment.SegmentName));
                }
            }
            if (GUILayout.Button("Apply Initial Weight")) {
                if (SegmentsList.Items != null && SegmentsList.Items.Count > 0) {
                    foreach (WeightedLevelSegment segment in SegmentsList.Items) {
                        segment.InitialWeight = SegmentsList.GlobalInitialWeight;
                    }
                }
            }
            if (GUILayout.Button("Apply Starting Difficulty")) {
                if (SegmentsList.Items != null && SegmentsList.Items.Count > 0) {
                    foreach (WeightedLevelSegment segment in SegmentsList.Items) {
                        segment.StartingDifficulty = SegmentsList.GlobalStartingDifficulty;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
