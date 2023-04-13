using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall;
using UnityEditor;
using MoreMountains.Tools;

namespace Paywall.Editors {

    /// <summary>
    /// Currently unused
    /// </summary>
    
    /*
    [CustomEditor(typeof(ProceduralLevelGenerator), true)]
    public class ProceduralLevelGeneratorEditor : Editor {
        protected MMReorderableList _list;
        protected SerializedProperty _levelSegments;
        protected const string _levelSegmentsName = "LevelSegments";

        protected virtual void OnEnable() {
            _list = new MMReorderableList(serializedObject.FindProperty(_levelSegmentsName));
            _list.elementNameProperty = _levelSegmentsName;
            _list.elementDisplayType = MMReorderableList.ElementDisplayType.Expandable;

        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

        }

    }
    */
}
