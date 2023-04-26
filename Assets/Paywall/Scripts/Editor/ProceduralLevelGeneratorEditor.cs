using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall;
using UnityEditor;
using MoreMountains.Tools;
using Paywall.Tools;

namespace Paywall.Editors {
    
    [CustomEditor(typeof(ProceduralLevelGenerator), true)]
    public class ProceduralLevelGeneratorEditor : Editor {
        #region Property Names

        protected const string _groundSegmentListPropertyName = "GroundSegmentList";
        protected const string _transitionSegmentListPropertyName = "TransitionSegmentList";
        protected const string _jumperSegmentListPropertyName = "JumperSegmentList";
        protected const string _firstLevelSegmentPropertyName = "FirstLevelSegment";

        protected const string _groundSegmentWeightPropertyName = "GroundSegmentWeight";
        protected const string _transitionSegmentWeightPropertyName = "TransitionSegmentWeight";
        protected const string _jumperSegmentWeightPropertyName = "JumperSegmentWeight";

        protected const string _shortestGapPropertyName = "ShortestGap";
        protected const string _shortGapPropertyName = "ShortGap";
        protected const string _mediumGapPropertyName = "MediumGap";
        protected const string _longGapPropertyName = "LongGap";
        protected const string _longestGapPropertyName = "LongestGap";
        protected const string _gapLengthPropertyName = "GapLength";
        protected const string _numberOfGapLengthsPropertyName = "NumberOfGapLengths";

        protected const string _lowestHeightPropertyName = "LowestHeight";
        protected const string _lowHeightPropertyName = "LowHeight";
        protected const string _mediumHeightPropertyName = "MediumHeight";
        protected const string _highHeightPropertyName = "HighHeight";
        protected const string _highestHeightPropertyName = "HighestHeight";
        protected const string _heightIntervalPropertyName = "HeightInterval";
        protected const string _numberOfHeightsPropertyName = "NumberOfHeights";

        protected const string _maxActiveSegmentsPropertyName = "MaxActiveSegments";
        protected const string _startDelayPropertyName = "StartDelay";

        #endregion

        #region Properties

        protected SerializedProperty _groundSegmentListProperty;
        protected SerializedProperty _transitionSegmentListProperty;
        protected SerializedProperty _jumperSegmentListProperty;
        protected SerializedProperty _firstLevelSegmentProperty;

        protected SerializedProperty _groundSegmentWeightProperty;
        protected SerializedProperty _transitionSegmentWeightProperty;
        protected SerializedProperty _jumperSegmentWeightProperty;

        protected SerializedProperty _shortestGapProperty;
        protected SerializedProperty _shortGapProperty;
        protected SerializedProperty _mediumGapProperty;
        protected SerializedProperty _longGapProperty;
        protected SerializedProperty _longestGapProperty;
        protected SerializedProperty _gapLengthProperty;
        protected SerializedProperty _numberOfGapLengthsProperty;

        protected SerializedProperty _lowestHeightProperty;
        protected SerializedProperty _lowHeightProperty;
        protected SerializedProperty _mediumHeightProperty;
        protected SerializedProperty _highHeightProperty;
        protected SerializedProperty _highestHeightProperty;
        protected SerializedProperty _heightIntervalProperty;
        protected SerializedProperty _numberOfHeightsProperty;

        protected SerializedProperty _maxActiveSegmentsProperty;
        protected SerializedProperty _startDelayProperty;

        #endregion

        protected bool _showLevelSegments;
        protected bool _showTypeWeights;
        protected bool _showGapLengths;
        protected bool _showHeights;
        protected bool _showOtherSettings;

        protected virtual void OnEnable() {
            _groundSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_groundSegmentListPropertyName);
            _transitionSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_transitionSegmentListPropertyName);
            _jumperSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_jumperSegmentListPropertyName);
            _firstLevelSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_firstLevelSegmentPropertyName);
            _groundSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_groundSegmentWeightPropertyName);
            _transitionSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_transitionSegmentWeightPropertyName);
            _jumperSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_jumperSegmentWeightPropertyName);
            _shortestGapProperty = serializedObject.FindPropertyByAutoPropertyName(_shortestGapPropertyName);
            _shortGapProperty = serializedObject.FindPropertyByAutoPropertyName(_shortGapPropertyName);
            _mediumGapProperty = serializedObject.FindPropertyByAutoPropertyName(_mediumGapPropertyName);
            _longGapProperty = serializedObject.FindPropertyByAutoPropertyName(_longGapPropertyName);
            _longestGapProperty = serializedObject.FindPropertyByAutoPropertyName(_longestGapPropertyName);
            _gapLengthProperty = serializedObject.FindPropertyByAutoPropertyName(_gapLengthPropertyName);
            _numberOfGapLengthsProperty = serializedObject.FindPropertyByAutoPropertyName(_numberOfGapLengthsPropertyName);

            _lowestHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_lowestHeightPropertyName);
            _lowHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_lowHeightPropertyName);
            _mediumHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_mediumHeightPropertyName);
            _highHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_highHeightPropertyName);
            _highestHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_highestHeightPropertyName);
            _heightIntervalProperty = serializedObject.FindPropertyByAutoPropertyName(_heightIntervalPropertyName);
            _numberOfHeightsProperty = serializedObject.FindPropertyByAutoPropertyName(_numberOfHeightsPropertyName);

            _maxActiveSegmentsProperty = serializedObject.FindPropertyByAutoPropertyName(_maxActiveSegmentsPropertyName);
            _startDelayProperty = serializedObject.FindPropertyByAutoPropertyName(_startDelayPropertyName);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space(10);
            _showLevelSegments = EditorGUILayout.Foldout(_showLevelSegments, "Level Segments", true);
            if (_showLevelSegments) {
                EditorGUILayout.PropertyField(_groundSegmentListProperty);
                EditorGUILayout.PropertyField(_transitionSegmentListProperty);
                EditorGUILayout.PropertyField(_jumperSegmentListProperty);
                EditorGUILayout.PropertyField(_firstLevelSegmentProperty);
            }

            EditorGUILayout.Space(10);
            _showTypeWeights = EditorGUILayout.Foldout(_showTypeWeights, "Type Weights", true);
            if (_showTypeWeights) {
                EditorGUILayout.PropertyField(_groundSegmentWeightProperty);
                EditorGUILayout.PropertyField(_transitionSegmentWeightProperty);
                EditorGUILayout.PropertyField(_jumperSegmentWeightProperty);
            }

            EditorGUILayout.Space(10);
            _showGapLengths = EditorGUILayout.Foldout(_showGapLengths, "Gap Lengths", true);
            if (_showGapLengths) {
                EditorGUILayout.PropertyField(_shortestGapProperty);
                EditorGUILayout.PropertyField(_shortGapProperty);
                EditorGUILayout.PropertyField(_mediumGapProperty);
                EditorGUILayout.PropertyField(_longGapProperty);
                EditorGUILayout.PropertyField(_longestGapProperty);
                //EditorGUILayout.PropertyField(_gapLengthProperty);
                //EditorGUILayout.PropertyField(_numberOfGapLengthsProperty);
            }

            EditorGUILayout.Space(10);
            _showHeights = EditorGUILayout.Foldout(_showHeights, "Spawn Heights", true);
            if (_showHeights) {
                //EditorGUILayout.PropertyField(_lowestHeightProperty);
                //EditorGUILayout.PropertyField(_lowHeightProperty);
                //EditorGUILayout.PropertyField(_mediumHeightProperty);
                //EditorGUILayout.PropertyField(_highHeightProperty);
                //EditorGUILayout.PropertyField(_highestHeightProperty);
                EditorGUILayout.PropertyField(_heightIntervalProperty);
                EditorGUILayout.PropertyField(_numberOfHeightsProperty);
            }

            EditorGUILayout.Space(10);
            _showOtherSettings = EditorGUILayout.Foldout(_showOtherSettings, "Other Settings", true);
            if (_showOtherSettings) {
                EditorGUILayout.PropertyField(_maxActiveSegmentsProperty);
                EditorGUILayout.PropertyField(_startDelayProperty);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

        }

    }

}
