using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;
using Paywall.Tools;

namespace Paywall {

    public enum FunctionTypes { Linear, Exponential }

    [System.Serializable]
    public class FunctionProperty {
        [field: SerializeField] public FunctionTypes FunctionType { get; protected set; }
        [field: FieldEnumCondition("FunctionType", (int)FunctionTypes.Linear)]
        [field: SerializeField] public int LinearModifier { get; protected set; }
        [field: FieldEnumCondition("FunctionType", (int)FunctionTypes.Exponential)]
        [field: Range(0, 1)]
        [field: SerializeField] public int ExponentialModifier { get; protected set; }
    }

    [System.Serializable]
    public class WeightedSegmentTypeList {
        [field: SerializeField] public SegmentTypes SegmentType { get; protected set; }
        [field: SerializeField] public int GroundSegmentWeight { get; set; }
        [field: SerializeField] public int TransitionSegmentWeight { get; set; }
        [field: SerializeField] public int JumperSegmentWeight { get; set; }
        [field: Range(0, 1)]
        [field: SerializeField] public float RepeatModifier { get; set; }
    }

    /// <summary>
    /// NoGap, Shortest, Short, Medium, Long, Longest
    /// </summary>
    public enum GapLengths { NoGap, Shortest, Short, Medium, Long, Longest }

    /// <summary>
    /// Generates level segments randomly
    /// </summary>
    public class ProceduralLevelGenerator : Singleton_PW<ProceduralLevelGenerator> {

        #region Property Fields

        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: HideInInspector]
        [field: SerializeField] public List<LevelSegment> LevelSegments { get; protected set; }

        [field: Header("Level Segments")]

        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public List<WeightedLevelSegment> GroundSegmentList { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public List<WeightedLevelSegment> TransitionSegmentList { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public List<WeightedLevelSegment> JumperSegmentList { get; protected set; }
        /// The very first level segment
        [field: Tooltip("The very first level segment")]
        [field: SerializeField] public LevelSegmentController FirstLevelSegment { get; protected set; }

        [field: Header("Spawn Poolers")]

        /// List of poolers for spawnable objects/enemies/items
        [field: Tooltip("List of poolers for spawnable objects/enemies/items")]
        [field: SerializeField] public List<SpawnableWeightedObjectPooler> SpawnPoolers { get; protected set; }

        [field: Header("Type Weight")]

        /// Weight of ground segment type
        [field: Tooltip("Weight of ground segment type")]
        [field: SerializeField] public int GroundSegmentWeight { get; protected set; } = 10;
        /// Weight of transition segment type
        [field: Tooltip("Weight of transition segment type")]
        [field: SerializeField] public int TransitionSegmentWeight { get; protected set; } = 10;
        /// Weight of jumper segment type
        [field: Tooltip("Weight of jumper segment type")]
        [field: SerializeField] public int JumperSegmentWeight { get; protected set; } = 10;
        /// Weight of jumper segment type
        [field: Tooltip("Weight of jumper segment type")]
        [field: SerializeField] public List<WeightedSegmentTypeList> WeightedTypeList { get; protected set; }

        [field: Header("Gap Lengths")]

        /// Shortest gap length
        [field: Tooltip("Shortest gap length")]
        [field: SerializeField] public float ShortestGap { get; protected set; }
        /// Short gap length
        [field: Tooltip("Short gap length")]
        [field: SerializeField] public float ShortGap { get; protected set; }
        /// Medium gap length
        [field: Tooltip("Medium gap length")]
        [field: SerializeField] public float MediumGap { get; protected set; }
        /// Long gap length
        [field: Tooltip("Long gap length")]
        [field: SerializeField] public float LongGap { get; protected set; }
        /// Longest gap length
        [field: Tooltip("Longest gap length")]
        [field: SerializeField] public float LongestGap { get; protected set; }
        /// The gap length
        [field: Tooltip("The gap length")]
        [field: SerializeField] public float GapLength { get; protected set; } = 1.5f;
        /// Longest gap length
        [field: Tooltip("Longest gap length")]
        [field: Range(1, 5)]
        [field: SerializeField] public int NumberOfGapLengths { get; protected set; } = 5;

        [field: Header("Spawn Heights")]

        /// Lowest spawn height
        [field: Tooltip("Lowest spawn height")]
        [field: SerializeField] public float LowestHeight { get; protected set; }
        /// Low spawn height
        [field: Tooltip("Low spawn height")]
        [field: SerializeField] public float LowHeight { get; protected set; }
        /// Medium spawn height
        [field: Tooltip("Medium spawn height")]
        [field: SerializeField] public float MediumHeight { get; protected set; }
        /// High spawn height
        [field: Tooltip("High spawn height")]
        [field: SerializeField] public float HighHeight { get; protected set; }
        /// Highest spawn height
        [field: Tooltip("Highest spawn height")]
        [field: SerializeField] public float HighestHeight { get; protected set; }
        /// Spawn height intervals
        [field: Tooltip("Spawn height intervals")]
        [field: SerializeField] public float HeightInterval { get; protected set; } = 1.5f;
        /// How many heights can level segments spawn at
        [field: Tooltip("How many heights can level segments spawn at")]
        [field: Range(1, 8)]
        [field: SerializeField] public int NumberOfHeights { get; protected set; } = 3;

        [field: Header("Global Settings")]

        /// The maximum number of active segments to maintain
        [field: Tooltip("The maximum number of active segments to maintain")]
        [field: SerializeField] public float NoneChance { get; protected set; } = 5;
        /// The game's current difficulty
        [field: Tooltip("The game's current difficulty")]
        [field: SerializeField] public float MediumLaunchHeight { get; protected set; } = 2.5f;

        [field: Header("Other Settings")]

        /// The maximum number of active segments to maintain
        [field: Tooltip("The maximum number of active segments to maintain")]
        [field: SerializeField] public int MaxActiveSegments { get; protected set; } = 15;
        /// How long to wait after scene load to start spawning segments
        [field: Tooltip("How long to wait after scene load to start spawning segments")]
        [field: SerializeField] public float StartDelay { get; protected set; } = 1f;
        /// The game's current difficulty
        [field: Tooltip("The game's current difficulty")]
        [field: SerializeField] public int CurrentDifficulty { get; protected set; }
        /// How much to increase segment weight per difficulty increment
        [field: Tooltip("How much to increase segment weight per difficulty increment")]
        [field: SerializeField] public int DifficultyWeightIncrement { get; protected set; } = 10;

        [field: Header("Debug")]

        /// Enable debug mode? Generate a predetermined sequence of levels
        [field: Tooltip("Enable debug mode? Generate a predetermined sequence of levels")]
        [field: SerializeField] public bool DebugMode { get; protected set; }
        /// Override gap length
        [field: Tooltip("Override gap length")]
        [field: SerializeField] public bool OverrideGapLength { get; protected set; }
        /// Ordered list of level segments
        [field: Tooltip("Ordered list of level segments")]
        [field: SerializeField] public List<string> LevelSegmentSequence { get; protected set; }

        #endregion

        public Dictionary<string, SpawnableWeightedObjectPooler> SpawnPoolerDict { get; protected set; } = new();

        protected Dictionary<string, WeightedLevelSegment> _levelSegments = new();
        protected Dictionary<GapLengths, float> _gapLengthsDict = new();

        protected IWeightedRandomizer<int> _typeRandomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<string> _groundSegmentRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _transitionSegmentRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _jumperSegmentRandomizer = new DynamicWeightedRandomizer<string>();

        protected IWeightedRandomizer<int> _afterGroundRandomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<int> _afterTransitionRandomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<int> _afterJumperRandomizer = new DynamicWeightedRandomizer<int>();
        protected Dictionary<SegmentTypes, float> _repeatModifiers = new();
        protected Dictionary<SegmentTypes, int> _initialWeights = new();

        protected LevelSegmentController _previousSegment;
        protected LevelSegmentController _currentSegment;
        protected int _activeSegments = 0;
        protected GapLengths _currentGapLength;
        protected int _currentHeight = 1;
        protected int _previousHeight = 1;

        protected bool _shouldStart;
        protected Coroutine _startCoroutine;
        protected CharacterJumpIRE _characterJump;

        protected int _sequenceIndex = 0;
        protected bool _initialized;

        protected override void Awake() {
            base.Awake();

            //Initialization();
        }

        public virtual void Initialize() {
            Initialization();
        }

        protected virtual void Initialization() {
            if (_initialized) {
                return;
            }

            if (SpawnPoolers.Count > 0) {
                foreach (SpawnableWeightedObjectPooler pooler in SpawnPoolers) {
                    //SpawnPoolerDict.Add(pooler.gameObject.name, pooler);
                    SpawnPoolerDict.Add(pooler.SpawnablePoolerType.ToString(), pooler);
                }
            }

            // Fill out gap lengths dictionary, initialize gap lengths
            MediumGap = GenerateGapLength(JumpTypes.Low);
            _gapLengthsDict.Add(GapLengths.NoGap, 0f);
            _gapLengthsDict.Add(GapLengths.Shortest, ShortestGap);
            _gapLengthsDict.Add(GapLengths.Short, ShortGap);
            _gapLengthsDict.Add(GapLengths.Medium, MediumGap);
            _gapLengthsDict.Add(GapLengths.Long, LongGap);
            _gapLengthsDict.Add(GapLengths.Longest, LongestGap);

            _typeRandomizer.Add((int)SegmentTypes.Ground, GroundSegmentWeight);
            _typeRandomizer.Add((int)SegmentTypes.Transition, TransitionSegmentWeight);
            _typeRandomizer.Add((int)SegmentTypes.Jumper, JumperSegmentWeight);
            _initialWeights.Add(SegmentTypes.Ground, GroundSegmentWeight);
            _initialWeights.Add(SegmentTypes.Transition, TransitionSegmentWeight);
            _initialWeights.Add(SegmentTypes.Jumper, JumperSegmentWeight);

            foreach (WeightedSegmentTypeList w in WeightedTypeList) {
                switch (w.SegmentType) {
                    case SegmentTypes.Ground:
                        _afterGroundRandomizer.Add((int)SegmentTypes.Ground, w.GroundSegmentWeight);
                        _afterGroundRandomizer.Add((int)SegmentTypes.Transition, w.TransitionSegmentWeight);
                        _afterGroundRandomizer.Add((int)SegmentTypes.Jumper, w.JumperSegmentWeight);
                        _repeatModifiers.Add(SegmentTypes.Ground, w.RepeatModifier);
                        break;
                    case SegmentTypes.Transition:
                        _afterTransitionRandomizer.Add((int)SegmentTypes.Ground, w.GroundSegmentWeight);
                        _afterTransitionRandomizer.Add((int)SegmentTypes.Transition, w.TransitionSegmentWeight);
                        _afterTransitionRandomizer.Add((int)SegmentTypes.Jumper, w.JumperSegmentWeight);
                        _repeatModifiers.Add(SegmentTypes.Transition, w.RepeatModifier);
                        break;
                    case SegmentTypes.Jumper:
                        _afterJumperRandomizer.Add((int)SegmentTypes.Ground, w.GroundSegmentWeight);
                        _afterJumperRandomizer.Add((int)SegmentTypes.Transition, w.TransitionSegmentWeight);
                        _afterJumperRandomizer.Add((int)SegmentTypes.Jumper, w.JumperSegmentWeight);
                        _repeatModifiers.Add(SegmentTypes.Jumper, w.RepeatModifier);
                        break;
                }
            }

            if ((GroundSegmentList != null) && (GroundSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in GroundSegmentList) {
                    _groundSegmentRandomizer.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment);
                }
            }
            if ((TransitionSegmentList != null) && (TransitionSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in TransitionSegmentList) {
                    _transitionSegmentRandomizer.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment);
                }
            }
            if ((JumperSegmentList != null) && (JumperSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in JumperSegmentList) {
                    _jumperSegmentRandomizer.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentPooler.SegmentToPool.SegmentName, segment);
                }
            }

            _initialized = true;
            if (FirstLevelSegment.GetComponent<MovingRigidbody>() != null) {
                FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = false;
            }

        }

        /// <summary>
        /// Check if we should spawn the next segment, spawn if applicable
        /// </summary>
        protected virtual void Update() {
            // If the game is not in progress, do nothing
            if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
                return;
            } else {
                if (!_shouldStart) {
                    _shouldStart = true;
                    if (FirstLevelSegment.GetComponent<MovingRigidbody>() != null) {
                        FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = true;
                    }
                }
            }

            if ((_activeSegments < MaxActiveSegments)) {
                if (_currentSegment != null) {
                    // Don't spawn if we would be spawning outside of recycle bounds
                    if ((LevelManagerIRE_PW.Instance.RecycleBounds.max.x - _currentSegment.RightOut.position.x) < 10f) {
                        return;
                    }
                }
                SpawnNextSegment();
            }
        }

        /// <summary>
        /// Randomly chooses the next level segment and spawns it
        /// </summary>
        protected virtual void SpawnNextSegment() {
            if (_previousSegment == null) {
                _previousSegment = FirstLevelSegment;
                _activeSegments++;
            } else {
                _previousSegment = _currentSegment;
            }
            ChooseLevelSegment();
            SpawnCurrentSegment();
        }

        /// <summary>
        /// Randomly chooses a level segment
        /// </summary>
        protected virtual void ChooseLevelSegment() {

            if (DebugMode) {
                if ((LevelSegmentSequence == null) || (LevelSegmentSequence.Count == 0)) {
                    return;
                }
                if (_sequenceIndex >= LevelSegmentSequence.Count) {
                    _sequenceIndex = 0;
                }
                _currentSegment = _levelSegments[LevelSegmentSequence[_sequenceIndex]].SegmentPooler.GetPooledGameObject().GetComponent<LevelSegmentController>();
                _sequenceIndex++;
                return;
            }

            // The previous segment's segment type is less likely to be chosen (less chance of repeat segment types)
            // Its weight is reduced multiplicatively by its RepeatModifier
            int newWeight = (int)Mathf.Floor(_typeRandomizer.GetWeight((int)_previousSegment.SegmentType) * _repeatModifiers[_previousSegment.SegmentType]);
            _typeRandomizer.SetWeight((int)_previousSegment.SegmentType, newWeight);
            SegmentTypes typeToUse = (SegmentTypes) _typeRandomizer.NextWithReplacement();
            if (typeToUse != _previousSegment.SegmentType) {
                _typeRandomizer.SetWeight((int)_previousSegment.SegmentType, _initialWeights[_previousSegment.SegmentType]);
            }

            //typeToUse = SegmentTypes.Ground;
            string key = null;
            switch (typeToUse) {
                case SegmentTypes.Ground:
                    key = _groundSegmentRandomizer.NextWithReplacement();
                    break;
                case SegmentTypes.Transition:
                    key = _transitionSegmentRandomizer.NextWithReplacement();
                    break;
                case SegmentTypes.Jumper:
                    key = _jumperSegmentRandomizer.NextWithReplacement();
                    break;
            }

            _currentSegment = _levelSegments[key].SegmentPooler.GetPooledGameObject().GetComponent<LevelSegmentController>();

        }

        /// <summary>
        /// Activates the current LevelSegmentController gameobject and sets its position based on the previous segment
        /// </summary>
        protected virtual void SpawnCurrentSegment() {
            float xPos = _previousSegment.RightOut.position.x + GetGapLength() - _currentSegment.LeftIn.localPosition.x;
            float yPos = _previousSegment.transform.position.y + GetHeightDelta();
            _currentSegment.transform.position = new Vector3(xPos, yPos);

            _currentSegment.gameObject.SetActive(true);
            _activeSegments++;
        }

        /// <summary>
        /// Gets the next gap length
        /// </summary>
        /// <returns></returns>
        protected virtual float GetGapLength() {
            if (_currentSegment.SegmentType == SegmentTypes.Jumper) {
                _currentGapLength = GapLengths.Medium;
            }
            else {
                switch (_previousSegment.SegmentType) {
                    case SegmentTypes.Ground:
                        _currentGapLength = (GapLengths)UnityEngine.Random.Range(0, 3);
                        break;
                    case SegmentTypes.Transition:
                        _currentGapLength = (GapLengths)UnityEngine.Random.Range(0, 3);
                        break;
                    // If previous was jumper, keep the same gap length
                    case SegmentTypes.Jumper:

                        break;
                }
            }

            if (OverrideGapLength) {
                return ShortestGap;
            }

            return (_gapLengthsDict[_currentGapLength]);
        }

        /// <summary>
        /// Gets the next segment height
        /// </summary>
        /// <returns>Float difference between the previous height and the current one (height delta to apply to previous height)</returns>
        protected virtual float GetHeightDelta() {
            _previousHeight = _currentHeight;
            int prevHeight = _currentHeight;
            float height = FirstLevelSegment.transform.position.y;

            switch (_previousSegment.HeightLockSetting) {
                case HeightLockSettings.None:
                    int min = -1;
                    int max = 2;
                    if (_currentHeight == NumberOfHeights) {
                        max = 1;
                    }
                    if (_currentHeight == 1) {
                        min = 0;
                    }
                    int increment = UnityEngine.Random.Range(min, max);
                    _currentHeight += increment;
                    height = (_currentHeight - prevHeight) * HeightInterval;
                    break;
                case HeightLockSettings.Previous:
                    break;
                case HeightLockSettings.Next:
                    break;
                case HeightLockSettings.Both:
                    break;
            }
            if (_currentSegment.HeightLockSetting == HeightLockSettings.Previous) {

            }
            if (_currentSegment.HeightLockSetting == HeightLockSettings.Both) {

            }

            return height;
        }

        /// <summary>
        /// Generate gap length based on CharacterJump and level speed
        /// </summary>
        /// <returns></returns>
        protected virtual float GenerateGapLength(JumpTypes jumpType) {
            _characterJump = LevelManagerIRE_PW.Instance.CurrentPlayableCharacters[0].GetComponent<CharacterJumpIRE>();
            float jumpTime = _characterJump.CalculateJumpTime(jumpType);
            float velocity = (1f / 10f) * LevelManagerIRE_PW.Instance.Speed;
            float distance = jumpTime * velocity * 0.7f;
            return distance;
        }

        /// <summary>
        /// Increments the difficulty level and adjusts segment weights accordingly
        /// </summary>
        protected virtual void IncrementDifficulty(int increment) {
            CurrentDifficulty += increment;
            PaywallDifficultyEvent.Trigger(CurrentDifficulty);

            foreach (KeyValuePair<string, WeightedLevelSegment> entry in _levelSegments) {
                // Only add weight if the segment appears at this difficulty
                if (entry.Value.StartingDifficulty == CurrentDifficulty) {
                    int newWeight;
                    switch (entry.Value.SegmentPooler.SegmentToPool.SegmentType) {
                        case SegmentTypes.Ground:
                            newWeight = _groundSegmentRandomizer.GetWeight(entry.Key) + 10;
                            _groundSegmentRandomizer.SetWeight(entry.Key, newWeight);
                            break;
                        case SegmentTypes.Transition:
                            newWeight = _transitionSegmentRandomizer.GetWeight(entry.Key) + 10;
                            _transitionSegmentRandomizer.SetWeight(entry.Key, newWeight);
                            break;
                        case SegmentTypes.Jumper:
                            newWeight = _jumperSegmentRandomizer.GetWeight(entry.Key) + 10;
                            _jumperSegmentRandomizer.SetWeight(entry.Key, newWeight);
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Called by a spawned segment when it is recycled (OutOfBoundsRecycle_PW)
        /// </summary>
        public virtual void DecrementActiveObjects() {
            _activeSegments--;
        }

        protected virtual void OnDisable() {
            StopAllCoroutines();
        }

        #region Editor

        /// <summary>
        /// UNUSED
        /// </summary>
        /// <returns></returns>
        public virtual LevelSegment[] GetAttachedSegments() {
            return LevelSegments.ToArray();
        }

        /// <summary>
        /// Used by SegmentsListInspectorDrawer
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetAttachedSegmentNames() {
            string[] names = new string[LevelSegments.Count];
            for (int i = 0; i < LevelSegments.Count; i++) {
                names[i] = LevelSegments[i].Label;
            }
            return names;
        }

        /// <summary>
        /// UNUSED
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetAttachedSegmentNamesSorted() {
            List<string> namesList = new();
            foreach (LevelSegment segment in LevelSegments) {
                namesList.Add(segment.Label);
            }
            namesList.Sort();
            
            return namesList.ToArray();
        }

        #endregion

    }
}
