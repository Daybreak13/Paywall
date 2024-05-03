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

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
