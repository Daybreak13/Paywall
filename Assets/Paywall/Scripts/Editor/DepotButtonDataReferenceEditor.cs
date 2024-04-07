using Paywall.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Paywall.Editors {

    [CustomEditor(typeof(DepotButtonDataReference), true)]
    [CanEditMultipleObjects]
    public class DepotButtonDataReferenceEditor : ButtonDataReferenceEditor {
        protected const string _depotItemComponentPropertyName = "DepotItem";
        protected const string _selectTypeComponentPropertyName = "SelectType";

        protected SerializedProperty _depotItemComponentProperty;
        protected SerializedProperty _selectTypeComponentProperty;

        protected override void OnEnable() {
            base.OnEnable();
            _depotItemComponentProperty = serializedObject.FindPropertyByAutoPropertyName( _depotItemComponentPropertyName );
            _selectTypeComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_selectTypeComponentPropertyName);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_depotItemComponentProperty);
            EditorGUILayout.PropertyField(_selectTypeComponentProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
