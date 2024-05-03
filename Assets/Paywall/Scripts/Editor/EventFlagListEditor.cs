using UnityEditor;
using UnityEngine;

namespace Paywall.Editors {

    [CustomEditor(typeof(EventFlagList))]
    public class EventFlagListEditor : Editor {
        public EventFlagList FlagsList {
            get {
                return (EventFlagList)target;
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, new string[] { });

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Sort")) {
                if (FlagsList.Items != null && FlagsList.Items.Count > 0) {
                    FlagsList.Items.Sort((a, b) => string.Compare(a.EventID, b.EventID));
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
