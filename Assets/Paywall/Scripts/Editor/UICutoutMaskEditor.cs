using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;
using UnityEditor;
using UnityEditor.UI;

namespace Paywall.Editors {

    [CustomEditor(typeof(UICutoutMask))]
    public class UICutoutMaskEditor : UnityEditor.UI.ImageEditor {
        public UICutoutMask uICutoutMask {
            get {
                return (UICutoutMask)target;
            }
        }

        protected const string _invertMaskPropertyName = "InvertMask";

        protected SerializedProperty _invertMaskProperty;

        public override void OnInspectorGUI() {
            _invertMaskProperty = serializedObject.FindPropertyByAutoPropertyName(_invertMaskPropertyName);

            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(_invertMaskProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
