using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Paywall.Tools {
	// original implementation by http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
	[CustomPropertyDrawer(typeof(FieldConditionAttribute))]
	public class FieldConditionAttributeDrawer : PropertyDrawer {
#if UNITY_EDITOR
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			FieldConditionAttribute conditionAttribute = (FieldConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(conditionAttribute, property);
			bool previouslyEnabled = GUI.enabled;
			bool shouldDisplay = ShouldDisplay(conditionAttribute, enabled);
			if (shouldDisplay) {
				GUI.enabled = enabled;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = previouslyEnabled;
			}
		}
#endif

		private bool GetConditionAttributeResult(FieldConditionAttribute conditionAttribute, SerializedProperty property) {
			bool enabled = true;
			string propertyPath = property.propertyPath;
			string conditionPath = propertyPath.Replace(property.name, conditionAttribute.ConditionBoolean);
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
			sourcePropertyValue ??= property.serializedObject.FindProperty(string.Format("<{0}>k__BackingField", conditionPath));

			if (sourcePropertyValue != null) {
				enabled = sourcePropertyValue.boolValue;
			}
			else {
				Debug.LogWarning("No matching boolean found for ConditionAttribute in object: " + conditionAttribute.ConditionBoolean);
			}
			if (conditionAttribute.Negative) {
				enabled = !enabled;
			}
			return enabled;
		}

		private bool ShouldDisplay(FieldConditionAttribute conditionAttribute, bool result) {
			bool shouldDisplay = !conditionAttribute.Hidden || result;
			return shouldDisplay;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			FieldConditionAttribute conditionAttribute = (FieldConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(conditionAttribute, property);

			bool shouldDisplay = ShouldDisplay(conditionAttribute, enabled);
			if (shouldDisplay) {
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else {
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}
	}
}