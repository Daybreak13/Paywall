#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Paywall.Tools {

    [CustomPropertyDrawer(typeof(FieldNullConditionAttribute))]
    public class FieldNullConditionAttributeDrawer : PropertyDrawer {
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            FieldNullConditionAttribute nullAttribute = (FieldNullConditionAttribute)attribute;
            bool enabled = GetNullAttributeResult(nullAttribute, property);
            bool previouslyEnabled = GUI.enabled;
            bool shouldDisplay = ShouldDisplay(nullAttribute, enabled);
            if (shouldDisplay) {
                GUI.enabled = enabled;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = previouslyEnabled;
            }
        }
#endif

        private bool GetNullAttributeResult(FieldNullConditionAttribute nullAttribute, SerializedProperty property) {
            bool enabled = true;
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, nullAttribute.FieldName);
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
            sourcePropertyValue ??= property.serializedObject.FindProperty(string.Format("<{0}>k__BackingField", conditionPath));

            if (sourcePropertyValue != null) {
                enabled = sourcePropertyValue.objectReferenceValue == null;
            }
            else {
                Debug.LogWarning("No matching field found for NullAttribute in object: " + nullAttribute.FieldName);
            }
            if (nullAttribute.Negative) {
                enabled = !enabled;
            }
            return enabled;
        }

        private bool ShouldDisplay(FieldNullConditionAttribute nullAttribute, bool result) {
            bool shouldDisplay = !nullAttribute.Hidden || result;
            return shouldDisplay;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            FieldNullConditionAttribute nullAttribute = (FieldNullConditionAttribute)attribute;
            bool enabled = GetNullAttributeResult(nullAttribute, property);

            bool shouldDisplay = ShouldDisplay(nullAttribute, enabled);
            if (shouldDisplay) {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
