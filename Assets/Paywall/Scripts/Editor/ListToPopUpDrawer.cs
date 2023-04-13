using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Paywall.Tools;

namespace Paywall.Editors {

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ListToPopupAttribute))]
    public class ListToPopUpDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ListToPopupAttribute atb = attribute as ListToPopupAttribute;
            List<string> stringList = null;

            if (atb.myType.GetField(atb.propertyName) != null) {
                stringList = atb.myType.GetField(atb.propertyName).GetValue(atb.myType) as List<string>;
            }

            if (stringList != null && stringList.Count != 0) {
                int selectedIndex = Mathf.Max(stringList.IndexOf(property.stringValue), 0);

                selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, stringList.ToArray());
                property.stringValue = stringList[selectedIndex];
            } else {
                EditorGUI.PropertyField(position, property, label);
            }


        }


    }
#endif

}
