using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Paywall.Editors {

    [CustomPropertyDrawer(typeof(SegmentsList))]
    public class SegmentsListInspectorDrawer : PropertyDrawer {
        private const string _currentIndexPropertyName = "CurrentIndex";
        private const string _weightPropertyName = "Weight";

#if UNITY_EDITOR

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string[] sortedNames = (property.serializedObject.targetObject as ProceduralLevelGenerator).GetAttachedSegmentNames();
            string[] segmentNames = new string[sortedNames.Length + 1];
            segmentNames[0] = "None";
            sortedNames.CopyTo(segmentNames, 1);

            SerializedProperty currentIndexProperty = property.FindPropertyRelative(_currentIndexPropertyName);

            EditorGUI.BeginProperty(position, label, property);

            currentIndexProperty.intValue = EditorGUI.Popup(position, currentIndexProperty.intValue, segmentNames);

            EditorGUI.EndProperty();
        }

#endif


    }
}
