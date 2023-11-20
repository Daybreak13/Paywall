using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall;
using UnityEditor;
using MoreMountains.Tools;
using Paywall.Tools;
using System.Text;

namespace Paywall.Editors {
    
    [CustomEditor(typeof(ProceduralLevelGenerator), true)]
    public class ProceduralLevelGeneratorEditor : Editor {
        public ProceduralLevelGenerator proceduralLevelGenerator {
            get {
                return (ProceduralLevelGenerator)target;
            }
        }

        #region Property Names

        protected const string _groundSegmentListPropertyName = "GroundSegmentList";
        protected const string _transitionSegmentListPropertyName = "TransitionSegmentList";
        protected const string _jumperSegmentListPropertyName = "JumperSegmentList";
        protected const string _firstLevelSegmentPropertyName = "FirstLevelSegment";
        protected const string _shopLevelSegmentPropertyName = "ShopLevelSegmentPooler";
        protected const string _previousSegmentPropertyName = "PreviousSegment";
        protected const string _currentSegmentPropertyName = "CurrentSegment";

        protected const string _baseStageSegmentPropertyName = "BaseStageLength";
        protected const string _currentStageSegmentPropertyName = "CurrentStage";

        protected const string _spawnPoolersPropertyName = "SpawnPoolers";

        protected const string _groundSegmentWeightPropertyName = "GroundSegmentWeight";
        protected const string _transitionSegmentWeightPropertyName = "TransitionSegmentWeight";
        protected const string _jumperSegmentWeightPropertyName = "JumperSegmentWeight";
        protected const string _weightedTypeListPropertyName = "WeightedTypeList";

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

        protected const string _noneChancePropertyName = "NoneChance";
        protected const string _mediumLaunchHeightPropertyName = "MediumLaunchHeight";

        protected const string _maxActiveSegmentsPropertyName = "MaxActiveSegments";
        protected const string _startDelayPropertyName = "StartDelay";
        protected const string _currentDifficultyPropertyName = "CurrentDifficulty";

        protected const string _debugModePropertyName = "DebugMode";
        protected const string _overrideGapLengthPropertyName = "OverrideGapLength";
        protected const string _doNotSpawnShopPropertyName = "DoNotSpawnShop";
        protected const string _levelSegmentSequencePropertyName = "LevelSegmentSequence";

        #endregion

        #region Properties

        protected SerializedProperty _groundSegmentListProperty;
        protected SerializedProperty _transitionSegmentListProperty;
        protected SerializedProperty _jumperSegmentListProperty;
        protected SerializedProperty _firstLevelSegmentProperty;
        protected SerializedProperty _shopLevelSegmentProperty;
        protected SerializedProperty _previousSegmentProperty;
        protected SerializedProperty _currentSegmentProperty;

        protected SerializedProperty _baseStageLengthSegmentProperty;
        protected SerializedProperty _currentStageSegmentProperty;

        protected SerializedProperty _spawnPoolersProperty;

        protected SerializedProperty _groundSegmentWeightProperty;
        protected SerializedProperty _transitionSegmentWeightProperty;
        protected SerializedProperty _jumperSegmentWeightProperty;
        protected SerializedProperty _weightedTypeListProperty;

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

        protected SerializedProperty _noneChanceProperty;
        protected SerializedProperty _mediumLaunchHeightProperty;

        protected SerializedProperty _maxActiveSegmentsProperty;
        protected SerializedProperty _startDelayProperty;
        protected SerializedProperty _currentDifficultyProperty;

        protected SerializedProperty _debugModeProperty;
        protected SerializedProperty _overrideGapLengthProperty;
        protected SerializedProperty _doNotSpawnShopProperty;
        protected SerializedProperty _levelSegmentSequenceProperty;

        #endregion

        protected bool _showLevelSegments;
        protected bool _showStages;
        protected bool _showSpawnPoolers;
        protected bool _showTypeWeights;
        protected bool _showGapLengths;
        protected bool _showHeights;
        protected bool _showOtherSettings;
        protected bool _showDebug;
        protected bool _showGlobalSettings;

        protected virtual void OnEnable() {
            _groundSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_groundSegmentListPropertyName);
            _transitionSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_transitionSegmentListPropertyName);
            _jumperSegmentListProperty = serializedObject.FindPropertyByAutoPropertyName(_jumperSegmentListPropertyName);
            _firstLevelSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_firstLevelSegmentPropertyName);
            _shopLevelSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_shopLevelSegmentPropertyName);
            _previousSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_previousSegmentPropertyName);
            _currentSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_currentSegmentPropertyName);

            _baseStageLengthSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_baseStageSegmentPropertyName);
            _currentStageSegmentProperty = serializedObject.FindPropertyByAutoPropertyName(_currentStageSegmentPropertyName);

            _spawnPoolersProperty = serializedObject.FindPropertyByAutoPropertyName(_spawnPoolersPropertyName);

            _groundSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_groundSegmentWeightPropertyName);
            _transitionSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_transitionSegmentWeightPropertyName);
            _jumperSegmentWeightProperty = serializedObject.FindPropertyByAutoPropertyName(_jumperSegmentWeightPropertyName);
            _weightedTypeListProperty = serializedObject.FindPropertyByAutoPropertyName(_weightedTypeListPropertyName);

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

            _noneChanceProperty = serializedObject.FindPropertyByAutoPropertyName(_noneChancePropertyName);
            _mediumLaunchHeightProperty = serializedObject.FindPropertyByAutoPropertyName(_mediumLaunchHeightPropertyName);

            _maxActiveSegmentsProperty = serializedObject.FindPropertyByAutoPropertyName(_maxActiveSegmentsPropertyName);
            _startDelayProperty = serializedObject.FindPropertyByAutoPropertyName(_startDelayPropertyName);
            _currentDifficultyProperty = serializedObject.FindPropertyByAutoPropertyName(_currentDifficultyPropertyName);

            _debugModeProperty = serializedObject.FindPropertyByAutoPropertyName(_debugModePropertyName);
            _overrideGapLengthProperty = serializedObject.FindPropertyByAutoPropertyName(_overrideGapLengthPropertyName);
            _doNotSpawnShopProperty = serializedObject.FindPropertyByAutoPropertyName(_doNotSpawnShopPropertyName);
            _levelSegmentSequenceProperty = serializedObject.FindPropertyByAutoPropertyName(_levelSegmentSequencePropertyName);
        }

        /// <summary>
        /// Sets the name of each pool to the name of the prefab which it is pooling
        /// </summary>
        protected virtual void RenamePools() {
            string name;
            StringBuilder sb = new();
            if ((proceduralLevelGenerator.GroundSegmentList != null) && (proceduralLevelGenerator.GroundSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in proceduralLevelGenerator.GroundSegmentList) {
                    if (segment.SegmentPooler == null || segment.SegmentPooler.SegmentToPool == null) {
                        break;
                    }
                    name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    if (name.Equals(string.Empty) || (name == null)) {
                        Debug.LogWarning("The LevelSegmentPooler \"" + segment.SegmentPooler.gameObject.name + "\" does not have a valid name!");
                    }
                    else {
                        segment.SegmentPooler.gameObject.name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    }
                }
            }
            if ((proceduralLevelGenerator.TransitionSegmentList != null) && (proceduralLevelGenerator.TransitionSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in proceduralLevelGenerator.TransitionSegmentList) {
                    if (segment.SegmentPooler == null || segment.SegmentPooler.SegmentToPool == null) {
                        break;
                    }
                    name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    if (name.Equals(string.Empty) || (name == null)) {
                        Debug.LogWarning("The LevelSegmentPooler \"" + segment.SegmentPooler.gameObject.name + "\" does not have a valid name!");
                    }
                    else {
                        segment.SegmentPooler.gameObject.name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    }
                }
            }
            if ((proceduralLevelGenerator.JumperSegmentList != null) && (proceduralLevelGenerator.JumperSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in proceduralLevelGenerator.JumperSegmentList) {
                    if (segment.SegmentPooler == null || segment.SegmentPooler.SegmentToPool == null) {
                        break;
                    }
                    name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    if (name.Equals(string.Empty) || (name == null)) {
                        sb.Clear();
                        sb.Append("The LevelSegmentPooler \"");
                        sb.Append(segment.SegmentPooler.gameObject.name);
                        sb.Append("\" does not have a valid name!");
                        Debug.LogWarning(sb.ToString());
                    }
                    else {
                        segment.SegmentPooler.gameObject.name = segment.SegmentPooler.SegmentToPool.SegmentName;
                    }
                }
            }
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
                EditorGUILayout.PropertyField(_shopLevelSegmentProperty);
                EditorGUILayout.PropertyField(_previousSegmentProperty);
                EditorGUILayout.PropertyField(_currentSegmentProperty);
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Rename Pools")) {
                    RenamePools();
                }
            }

            EditorGUILayout.Space(10);
            _showStages = EditorGUILayout.Foldout(_showStages, "Stages", true);
            if (_showStages) {
                EditorGUILayout.PropertyField(_baseStageLengthSegmentProperty);
                EditorGUILayout.PropertyField(_currentStageSegmentProperty);
            }

            EditorGUILayout.Space(10);
            _showTypeWeights = EditorGUILayout.Foldout(_showTypeWeights, "Type Weights", true);
            if (_showTypeWeights) {
                EditorGUILayout.PropertyField(_groundSegmentWeightProperty);
                EditorGUILayout.PropertyField(_transitionSegmentWeightProperty);
                EditorGUILayout.PropertyField(_jumperSegmentWeightProperty);
                EditorGUILayout.PropertyField(_weightedTypeListProperty);
            }

            EditorGUILayout.Space(10);
            _showSpawnPoolers = EditorGUILayout.Foldout(_showSpawnPoolers, "Spawn Poolers", true);
            if (_showSpawnPoolers) {
                EditorGUILayout.PropertyField(_spawnPoolersProperty);
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
            _showGlobalSettings = EditorGUILayout.Foldout(_showGlobalSettings, "Global Settings", true);
            if (_showGlobalSettings) {
                EditorGUILayout.PropertyField(_noneChanceProperty);
                EditorGUILayout.PropertyField(_mediumLaunchHeightProperty);
            }

            EditorGUILayout.Space(10);
            _showOtherSettings = EditorGUILayout.Foldout(_showOtherSettings, "Other Settings", true);
            if (_showOtherSettings) {
                EditorGUILayout.PropertyField(_maxActiveSegmentsProperty);
                EditorGUILayout.PropertyField(_startDelayProperty);
                EditorGUILayout.PropertyField(_currentDifficultyProperty);
            }

            EditorGUILayout.Space(10);
            _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug", true);
            if (_showDebug) {
                EditorGUILayout.PropertyField(_debugModeProperty);
                EditorGUILayout.PropertyField(_overrideGapLengthProperty);
                EditorGUILayout.PropertyField(_doNotSpawnShopProperty);
                EditorGUILayout.PropertyField(_levelSegmentSequenceProperty);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

        }

    }

}
