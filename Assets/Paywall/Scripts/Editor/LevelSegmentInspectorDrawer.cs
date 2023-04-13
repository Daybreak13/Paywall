using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Paywall.Editors {

	/// <summary>
	/// Currently unused
	/// </summary>
	//[CustomPropertyDrawer(typeof(LevelSegment), true)]
	public class LevelSegmentInspectorDrawer : PropertyDrawer {

		const float LineHeight = 16f;
		private const string _nextLevelSegmentsPropertyName = "NextLevelSegments";
		private const string _labelPropertyName = "Label";
		private const string _currentPropertyName = "CurrentSegment";
		private int _choiceIndex;
		string[] test = { "1", "2", "3" };

#if UNITY_EDITOR

		/// <summary>
		/// Draws 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="prop"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty labelProperty = property.FindPropertyRelative(_labelPropertyName);
			SerializedProperty nextLevelSegmentsProperty = property.FindPropertyRelative(_nextLevelSegmentsPropertyName);
			SerializedProperty currentProperty = property.FindPropertyRelative(_currentPropertyName);

			EditorGUI.BeginProperty(position, label, property);

			Rect rect = position;
			var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(labelProperty));
			rect.height = height;

			EditorGUI.PropertyField(rect, labelProperty, new GUIContent(labelProperty.name));
			rect.y += height;

			EditorGUI.PropertyField(rect, nextLevelSegmentsProperty, new GUIContent(nextLevelSegmentsProperty.name));
			rect.y += height;

			EditorGUI.EndProperty();
		}

#endif

		protected virtual void Dropdown(Rect rect, SerializedProperty property) {
			SerializedProperty current = property.FindPropertyRelative(_currentPropertyName);
			string[] _segmentNames = (property.serializedObject.targetObject as ProceduralLevelGenerator).GetAttachedSegmentNames();
			//EditorGUI.BeginChangeCheck();
			current.intValue = EditorGUI.Popup(rect, current.intValue, _segmentNames);
			/*
			if (EditorGUI.EndChangeCheck()) {
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(property.serializedObject.targetObject);
            }*/
		}

		/// <summary>
		/// Draws a selector letting the user pick any action associated with the AIBrain this action is on
		/// </summary>
		/// <param name="position"></param>
		/// <param name="prop"></param>
		protected virtual void DrawSelectionDropdown(Rect position, SerializedProperty prop) {
			//LevelSegment thisAction = prop.objectReferenceValue as LevelSegment;
			LevelSegment[] segments = (prop.serializedObject.targetObject as ProceduralLevelGenerator).GetAttachedSegments();
			int selected = 0;
			int i = 1;
			string[] options = new string[segments.Length + 1];
			options[0] = "None";
			foreach (LevelSegment action in segments) {
				string name = string.IsNullOrEmpty(action.Label) ? action.GetType().Name : action.Label;
				options[i] = i.ToString() + " - " + name;
				//if (action == thisAction) {
				//	selected = i;
				//}
				i++;
			}

			EditorGUI.BeginChangeCheck();
			selected = EditorGUI.Popup(position, selected, options);
			if (EditorGUI.EndChangeCheck()) {
				//prop.objectReferenceValue = (selected == 0) ? null : segments[selected - 1];
				prop.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(prop.serializedObject.targetObject);
			}
		}

		/// <summary>
		/// Returns the height of the full property
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(property));
			float height = h * 2; // 2 lines, one for the dropdown, one for the property field
			return height;
		}
	}

}
