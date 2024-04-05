using Paywall.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Paywall.Editors {

    [CustomEditor(typeof(DepotButtonDataReference), true)]
    [CanEditMultipleObjects]
    public class DepotButtonDataReferenceEditor : ButtonDataReferenceEditor {
        protected const string _depotItemComponentPropertyName = "ImageComponent";

        protected SerializedProperty _depotItemComponentProperty;

        protected override void OnEnable() {
            base.OnEnable();
            _depotItemComponentProperty = serializedObject.FindPropertyByAutoPropertyName( _depotItemComponentPropertyName );
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(_depotItemComponentProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
