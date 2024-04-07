using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using Paywall.Tools;

namespace Paywall.Editors {

    [CustomEditor(typeof(ButtonDataReference), true)]
    [CanEditMultipleObjects]
    public class ButtonDataReferenceEditor : ButtonEditor {
        protected const string _outerImageComponentPropertyName = "OuterImageComponent";
        protected const string _innerImageComponentPropertyName = "InnerImageComponent";
        protected const string _textComponentPropertyName = "TextComponent";
        protected const string _selectComponentPropertyName = "ButtonSelect";
        protected const string _containerComponentPropertyName = "Container";
        protected const string _selectEventComponentPropertyName = "OnSelectEvent";

        protected SerializedProperty _outerImageComponentProperty;
        protected SerializedProperty _innerImageComponentProperty;
        protected SerializedProperty _textComponentProperty;
        protected SerializedProperty _selectComponentProperty;
        protected SerializedProperty _containerComponentProperty;
        protected SerializedProperty _selectEventComponentProperty;

        protected override void OnEnable() {
            base.OnEnable();
            _outerImageComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_outerImageComponentPropertyName);
            _innerImageComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_innerImageComponentPropertyName);
            _textComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_textComponentPropertyName);
            _selectComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_selectComponentPropertyName);
            _containerComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_containerComponentPropertyName);
            _selectEventComponentProperty = serializedObject.FindPropertyByAutoPropertyName(_selectEventComponentPropertyName);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(_outerImageComponentProperty);
            EditorGUILayout.PropertyField(_innerImageComponentProperty);
            EditorGUILayout.PropertyField(_textComponentProperty);
            EditorGUILayout.PropertyField(_selectComponentProperty);
            EditorGUILayout.PropertyField(_containerComponentProperty);
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_selectEventComponentProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
