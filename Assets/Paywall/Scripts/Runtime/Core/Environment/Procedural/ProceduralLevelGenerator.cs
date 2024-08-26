using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;
using Paywall.Tools;
using UnityEditor;
using MoreMountains.InfiniteRunnerEngine;
using UnityEngine.InputSystem;
using System.Linq;
using KaimiraGames;
using System;

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

    /// <summary>
    /// Weighted SpawnableWeightedObjectPooler. Essentially, provides a weight for the spawnable type the pooler corresponds to.
    /// </summary>
    [System.Serializable]
    public class WeightedSpawnPooler {
        [field: SerializeField] public SpawnableWeightedObjectPooler Pooler { get; set; }
        [field: SerializeField] public int InitialWeight { get; set; }
    }

    [System.Serializable]
    public class WeightedSegmentPooler {
        [field: SerializeField] public LevelSegmentPooler Pooler { get; set; }
        [field: SerializeField] public int InitialWeight { get; protected set; } = 10;
        [field: SerializeField] public int StartingDifficulty { get; protected set; }
        [field: NonSerialized]
        public int CurrentWeight { get; protected set; }
        public void SetWeight(int weight) {
            CurrentWeight = weight;
        }
    }

    [System.Serializable]
    public class WeightedHeightDelta {
        public int HeightDelta;
        public int Weight;
    }

    /// <summary>
    /// NoGap, Shortest, Short, Medium, Long, Longest
    /// </summary>
    public enum GapLengths { NoGap, Shortest, Medium, Longest }

    /// <summary>
    /// Generates level segments randomly
    /// Generates height delta, distance, depending on selected segment types
    /// </summary>
    public class ProceduralLevelGenerator : Singleton_PW<ProceduralLevelGenerator>, MMEventListener<MMGameEvent> {

        #region Property Fields

        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: HideInInspector]
        [field: SerializeField] public List<LevelSegment> LevelSegments { get; protected set; }

        [field: Header("Level Segments")]

        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public SegmentListData GroundSegmentList { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public SegmentListData JumperSegmentList { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public SegmentListData TransitionSegmentList { get; protected set; }
        /// The very first level segment
        [field: Tooltip("The very first level segment")]
        [field: SerializeField] public LevelSegmentController FirstLevelSegment { get; protected set; }
        /// The shop level segment
        [field: Tooltip("The shop level segment")]
        [field: SerializeField] public LevelSegmentPooler ShopLevelSegmentPooler { get; protected set; }
        /// Level segment pooler prefab
        [field: Tooltip("Level segment pooler prefab")]
        [field: SerializeField] public LevelSegmentPooler PoolerPrefab { get; protected set; }

        [field: MMReadOnly]
        [field: SerializeField] public LevelSegmentController PreviousSegment { get; protected set; }
        [field: MMReadOnly]
        [field: SerializeField] public LevelSegmentController CurrentSegment { get; protected set; }

        [field: Header("Stages")]

        /// The length of the first stage, off which all other stage lengths are derived
        [field: Tooltip("The length of the first stage, off which all other stage lengths are derived")]
        [field: SerializeField] public float BaseStageLength { get; protected set; }
        /// The current stage we are on
        [field: Tooltip("The current stage we are on")]
        [field: MMReadOnly]
        [field: SerializeField] public int CurrentStage { get; protected set; } = 1;

        [field: Header("Spawn Poolers")]

        /// List of poolers for spawnable objects/enemies/items
        [field: Tooltip("List of poolers for spawnable objects/enemies/items")]
        [field: SerializeField] public List<WeightedSpawnPooler> SpawnPoolers { get; protected set; }
        /// How much to increase weight if applicable, each time difficulty is incremented
        [field: Tooltip("How much to increase weight if applicable, each time difficulty is incremented")]
        [field: SerializeField] public int WeightIncrement { get; protected set; } = 1;

        [field: Header("Type Weight")]

        /// Weight of ground segment type
        [field: Tooltip("Weight of ground segment type")]
        [field: SerializeField] public int GroundSegmentWeight { get; protected set; } = 10;
        /// Weight of jumper segment type
        [field: Tooltip("Weight of jumper segment type")]
        [field: SerializeField] public int JumperSegmentWeight { get; protected set; } = 10;
        /// Percentage chance of spawning a transition segment when valid (not in betweeen jumpers)
        [field: Tooltip("Percentage chance of spawning a transition segment when valid (not in betweeen jumpers)")]
        [field: Range(0f, 1f)]
        [field: SerializeField] public float TransitionSegmentChance { get; protected set; } = 0.5f;

        /// <summary>
        /// Long and longest unused. Medium gap is variable, short/shortest are static.
        /// </summary>
        [field: Header("Gap Lengths")]

        /// Shortest gap length
        [field: Tooltip("Shortest gap length")]
        [field: SerializeField] public float ShortestGap { get; protected set; }
        /// Medium gap length. Calculated at runtime based on player jump duration
        [field: Tooltip("Medium gap length. Calculated at runtime based on player jump duration")]
        [field: SerializeField] public float MediumGap { get; protected set; }
        /// Longest gap length
        [field: Tooltip("Longest gap length")]
        [field: SerializeField] public float LongestGap { get; protected set; }
        /// The gap length
        [field: Tooltip("The gap length")]
        [field: SerializeField] public float GapLength { get; protected set; } = 1.5f;

        [field: Header("Spawn Heights")]

        /// Spawn height intervals
        [field: Tooltip("Spawn height intervals")]
        [field: SerializeField] public float HeightInterval { get; protected set; } = 1.5f;
        /// How many heights can level segments spawn at
        [field: Tooltip("How many heights can level segments spawn at")]
        [field: Range(1, 8)]
        [field: SerializeField] public int NumberOfHeights { get; protected set; } = 3;
        /// List of weighted height deltas
        [field: Tooltip("List of weighted height deltas")]
        [field: SerializeField] public List<WeightedHeightDelta> HeightDeltas { get; protected set; }

        [field: Header("Global Settings")]

        /// Global chance for a spawn point to spawn nothing
        [field: Tooltip("Global chance for a spawn point to spawn nothing")]
        [field: Range(0f, 1f)]
        [field: SerializeField] public float NoneChance { get; protected set; } = 0.05f;
        /// Launch height used by launch pads
        [field: Tooltip("Launch height used by launch pads")]
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
        /// Disable transition segment generation
        [field: Tooltip("Disable transition segment generation")]
        [field: SerializeField] public bool DisableTransitions { get; protected set; }
        /// Override gap length and just use ShortestGapLength
        [field: Tooltip("Override gap length and just use ShortestGapLength")]
        [field: SerializeField] public bool OverrideGapLength { get; protected set; }
        /// Do not spawn shop segment
        [field: Tooltip("Do not spawn shop segment")]
        [field: SerializeField] public bool DoNotSpawnShop { get; protected set; }
        /// The name of the transition segment to always use in Debug Mode
        [field: Tooltip("The name of the transition segment to always use in Debug Mode")]
        [field: SerializeField] public string ForcedTransitionSegment { get; protected set; }
        /// Ordered list of level segments
        [field: Tooltip("Ordered list of level segments")]
        [field: SerializeField] public List<string> LevelSegmentSequence { get; protected set; }

        #endregion

        public Dictionary<string, WeightedSpawnPooler> SpawnPoolerDict { get; protected set; } = new();
        public int CurrentHeight { get { return _currentHeightIdx; } }

        protected Dictionary<string, LevelSegmentPooler> _levelSegments = new();
        protected Dictionary<GapLengths, float> _gapLengthsDict = new();

        protected WeightedList<int> _typeRandomizer;
        protected WeightedList<string> _groundSegmentRandomizer;
        protected WeightedList<string> _transitionSegmentRandomizer;
        protected WeightedList<string> _jumperSegmentRandomizer;

        protected IWeightedRandomizer<string> _neutralTransitionRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _plusTransitionRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _plusTwoTransitionRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _minusTransitionRandomizer = new DynamicWeightedRandomizer<string>();

        protected Dictionary<SegmentTypes, float> _repeatModifiers = new();
        protected Dictionary<SegmentTypes, int> _initialWeights = new();
        protected Dictionary<int, List<WeightedLevelSegment>> UnusedSegments = new();       // Segments that do not spawn at Difficulty 0 and have to be added to the randomizer as difficulty increases

        public LevelSegmentController NextSegment { get; private set; }      // Lookahead segment used to determine transition type
        protected int _activeSegments = 0;
        protected GapLengths _currentGapLength;
        protected int _currentHeightIdx = 1;
        protected int _previousHeightIdx = 1;
        protected float _currentHeight;
        protected float _startingHeight;

        protected bool _shouldStart;
        protected Coroutine _startCoroutine;
        protected CharacterJumpIRE _characterJump;
        protected float _currentStageLength;
        protected float _previousStageCutoff;   // Distance at which previous stage ended
        protected bool _blockSpawn;

        protected int _sequenceIndex = 0;
        protected bool _initialized;

        protected System.Random _randomGenerator;

        /// <summary>
        /// This component is initialized after LevelManager, which signals to initialize
        /// </summary>
        public virtual void Initialize() {
            Initialization();
        }

        /// <summary>
        /// Called by SpawnablePoolableObject to reset its parent transform during OnDisable
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public virtual void ResetObjectParent(Transform child, Transform parent) {
            StartCoroutine(ResetObjectParentCo(child, parent));
        }

        protected virtual IEnumerator ResetObjectParentCo(Transform child, Transform parent) {
            yield return new WaitForEndOfFrame();
            child.SetParent(parent);
        }

        protected virtual void Initialization() {
            if (_initialized) {
                return;
            }

            // Initialize random number generators
            _randomGenerator = RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
            _typeRandomizer = new(RandomManager.NewRandom(PaywallProgressManager.RandomSeed));
            _groundSegmentRandomizer = new(RandomManager.NewRandom(PaywallProgressManager.RandomSeed));
            _jumperSegmentRandomizer = new(RandomManager.NewRandom(PaywallProgressManager.RandomSeed));
            _transitionSegmentRandomizer = new(RandomManager.NewRandom(PaywallProgressManager.RandomSeed));

            _startingHeight = FirstLevelSegment.transform.position.y;

            if (SpawnPoolers.Count > 0) {
                foreach (WeightedSpawnPooler pooler in SpawnPoolers) {
                    SpawnPoolerDict.Add(pooler.Pooler.SpawnablePoolerType.ID, pooler);
                }
            }

            // Fill out gap lengths dictionary, initialize gap lengths
            MediumGap = GenerateGapLength(JumpTypes.Normal);
            _gapLengthsDict.Add(GapLengths.NoGap, 0f);
            _gapLengthsDict.Add(GapLengths.Shortest, ShortestGap);
            _gapLengthsDict.Add(GapLengths.Medium, MediumGap);
            _gapLengthsDict.Add(GapLengths.Longest, LongestGap);

            if (GroundSegmentWeight > 0)
                _typeRandomizer.Add((int)SegmentTypes.Ground, GroundSegmentWeight);
            if (JumperSegmentWeight > 0)
                _typeRandomizer.Add((int)SegmentTypes.Jumper, JumperSegmentWeight);
            _initialWeights.Add(SegmentTypes.Ground, GroundSegmentWeight);
            _initialWeights.Add(SegmentTypes.Jumper, JumperSegmentWeight);

            if ((GroundSegmentList != null) && (GroundSegmentList.SegmentList != null) && (GroundSegmentList.SegmentList.Items.Count > 0)) {
                foreach (WeightedLevelSegment segment in GroundSegmentList.SegmentList.Items) {
                    segment.Segment.SetSegmentType(SegmentTypes.Ground);
                    PoolerPrefab.SetSegment(segment.Segment);
                    LevelSegmentPooler pooler = Instantiate(PoolerPrefab, GroundSegmentList.PoolParent.transform);
                    pooler.gameObject.name = segment.Segment.SegmentName;

                    if (segment.InitialWeight > 0) {
                        if (segment.StartingDifficulty == 0) {
                            _groundSegmentRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                        }
                        else {
                            if (UnusedSegments.ContainsKey(segment.StartingDifficulty)) {
                                UnusedSegments[segment.StartingDifficulty].Add(segment);
                            }
                            else {
                                UnusedSegments.Add(segment.StartingDifficulty, new List<WeightedLevelSegment> { segment });
                            }
                        }
                    }
                    _levelSegments.Add(segment.Segment.SegmentName, pooler);
                }
            }
            if ((TransitionSegmentList != null) && (TransitionSegmentList.SegmentList != null) && (TransitionSegmentList.SegmentList.Items.Count > 0)) {
                foreach (WeightedLevelSegment segment in TransitionSegmentList.SegmentList.Items) {
                    segment.Segment.SetSegmentType(SegmentTypes.Transition);
                    PoolerPrefab.SetSegment(segment.Segment);
                    LevelSegmentPooler pooler = Instantiate(PoolerPrefab, TransitionSegmentList.PoolParent.transform);
                    pooler.gameObject.name = segment.Segment.SegmentName;

                    if (segment.InitialWeight == 0) {
                        continue;
                    }

                    _transitionSegmentRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.Segment.SegmentName, pooler);

                    if ((segment.Segment as TransitionSegmentController).NeutralTransition) {
                        _neutralTransitionRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    }
                    if ((segment.Segment as TransitionSegmentController).PlusTransition) {
                        _plusTransitionRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    }
                    if ((segment.Segment as TransitionSegmentController).PlusTwoTransition) {
                        _plusTwoTransitionRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    }
                    if ((segment.Segment as TransitionSegmentController).MinusTransition) {
                        _minusTransitionRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    }
                }
            }
            if ((JumperSegmentList != null) && (JumperSegmentList.SegmentList != null) && (JumperSegmentList.SegmentList.Items.Count > 0)) {
                foreach (WeightedLevelSegment segment in JumperSegmentList.SegmentList.Items) {
                    segment.Segment.SetSegmentType(SegmentTypes.Jumper);
                    PoolerPrefab.SetSegment(segment.Segment);
                    LevelSegmentPooler pooler = Instantiate(PoolerPrefab, JumperSegmentList.PoolParent.transform);
                    pooler.gameObject.name = segment.Segment.SegmentName;

                    if (segment.InitialWeight > 0) {
                        if (segment.StartingDifficulty == 0) {
                            _jumperSegmentRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                        }
                        else {
                            if (UnusedSegments.ContainsKey(segment.StartingDifficulty)) {
                                UnusedSegments[segment.StartingDifficulty].Add(segment);
                            }
                            else {
                                UnusedSegments.Add(segment.StartingDifficulty, new List<WeightedLevelSegment> { segment });
                            }
                        }
                    }

                    if (segment.InitialWeight > 0)
                        _jumperSegmentRandomizer.Add(segment.Segment.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.Segment.SegmentName, pooler);
                }
            }

            if (FirstLevelSegment.GetComponent<MovingRigidbody>() != null) {
                FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = false;
            }

            _currentStageLength = BaseStageLength;

            _initialized = true;

        }

        /// <summary>
        /// Check if we should spawn the next segment, spawn if applicable
        /// </summary>
        protected virtual void Update() {
            // If the game is not in progress, do nothing
            if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress
                    || _blockSpawn) {
                return;
            }
            else {
                if (!_shouldStart) {
                    _shouldStart = true;
                    if (FirstLevelSegment.GetComponent<MovingRigidbody>() != null) {
                        FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = true;
                    }
                }
            }

            if ((_activeSegments < MaxActiveSegments)) {
                if (CurrentSegment != null) {
                    // Don't spawn if we would be spawning outside of recycle bounds
                    if ((LevelManagerIRE_PW.Instance.RecycleBounds.max.x - CurrentSegment.RightBound.x) < 30f) {
                        return;
                    }

                    // Handle stage. If we are spawning the shop segment, do not spawn anything else this frame
                    if (HandleStage()) {
                        return;
                    }
                }
                SpawnNextSegment();
            }
        }

        /// <summary>
        /// Manages stage count and depot spawn. If we've reached the end of the stage, handle it, advance to next stage.
        /// </summary>
        protected virtual bool HandleStage() {
            float charPos = LevelManagerIRE_PW.Instance.CurrentPlayableCharacters[0].transform.position.x;
            float farRight = CurrentSegment.RightBound.x;
            // Distance traveled since entered stage = total distance - distance entered current stage
            if (LevelManagerIRE_PW.Instance.DistanceTraveled - _previousStageCutoff + (farRight - charPos) >= _currentStageLength) {
                if (DoNotSpawnShop) {
                    _previousStageCutoff = LevelManagerIRE_PW.Instance.DistanceTraveled;
                }
                IncrementStage();
                GetNextStageLength();

                // If we're spawning the shop
                if (ShopLevelSegmentPooler != null && !DoNotSpawnShop) {
                    PreviousSegment = CurrentSegment;
                    CurrentSegment = ShopLevelSegmentPooler.GetPooledGameObject().GetComponent<LevelSegmentController>();
                    NextSegment = null;
                    SpawnCurrentSegment();
                    _blockSpawn = true;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles what happens when stage number is incremented
        /// Increments difficulty by 1
        /// </summary>
        public virtual void IncrementStage() {
            CurrentStage++;
            GUIManagerIRE_PW.Instance.UpdateStageText(CurrentStage);
            IncrementDifficulty(1);
        }

        /// <summary>
        /// Randomly chooses the next level segment and spawns it
        /// </summary>
        protected virtual void SpawnNextSegment() {
            if (PreviousSegment == null) {
                PreviousSegment = FirstLevelSegment;
                _activeSegments++;
            }
            else {
                PreviousSegment = CurrentSegment;
            }

            ChooseLevelSegment();
            HandleTransitionSegment();
            SpawnCurrentSegment();
        }

        /// <summary>
        /// Randomly chooses a level segment
        /// </summary>
        protected virtual void ChooseLevelSegment() {

            // Debug mode overrides random segment generation
            if (DebugMode) {
                if ((LevelSegmentSequence == null) || (LevelSegmentSequence.Count == 0)) {
                    return;
                }
                if (_sequenceIndex >= LevelSegmentSequence.Count) {
                    _sequenceIndex = 0;
                }
                CurrentSegment = _levelSegments[LevelSegmentSequence[_sequenceIndex]].GetPooledGameObject().GetComponent<LevelSegmentController>();
                _sequenceIndex++;
                return;
            }

            // If we have a lookahead segment stored, spawn that one instead
            if (NextSegment != null) {
                CurrentSegment = NextSegment;
                NextSegment = null;
                return;
            }

            // The previous segment's segment type is less likely to be chosen (less chance of repeat segment types)
            // Its weight is reduced by its RepeatModifier
            //switch (PreviousSegment.SegmentType) {
            //    case SegmentTypes.Ground:
            //        break;
            //    case SegmentTypes.Jumper:
            //        int newWeight = _typeRandomizer.GetWeightOf((int)PreviousSegment.SegmentType) - 1;
            //        if (newWeight < 1) {
            //            newWeight = 1;
            //        }
            //        _typeRandomizer.SetWeight((int)PreviousSegment.SegmentType, newWeight);
            //        break;
            //}
            SegmentTypes typeToUse = (SegmentTypes)_typeRandomizer.Next();
            // If it is a new segment type, reset segment weights
            if (typeToUse != PreviousSegment.SegmentType) {
                _typeRandomizer.SetWeight((int)PreviousSegment.SegmentType, _initialWeights[PreviousSegment.SegmentType]);
            }

            string key = null;
            switch (typeToUse) {
                case SegmentTypes.Ground:
                    key = _groundSegmentRandomizer.Next();
                    while (_levelSegments[key].GetPooledGameObject().GetComponent<LevelSegmentController>().StaticHeight > NumberOfHeights) {
                        key = _groundSegmentRandomizer.Next();
                    }
                    break;
                case SegmentTypes.Transition:
                    key = _transitionSegmentRandomizer.Next();
                    while (_levelSegments[key].GetPooledGameObject().GetComponent<LevelSegmentController>().StaticHeight > NumberOfHeights) {
                        key = _transitionSegmentRandomizer.Next();
                    }
                    break;
                case SegmentTypes.Jumper:
                    key = _jumperSegmentRandomizer.Next();
                    while (_levelSegments[key].GetPooledGameObject().GetComponent<LevelSegmentController>().StaticHeight > NumberOfHeights) {
                        key = _jumperSegmentRandomizer.Next();
                    }
                    break;
            }

            if (key == null) {
                Debug.Log("null key");
            }
            CurrentSegment = _levelSegments[key].GetPooledGameObject().GetComponent<LevelSegmentController>();
        }

        /// <summary>
        /// Determines if we should spawn a transition segment, and sets settings
        /// </summary>
        protected virtual void HandleTransitionSegment() {
            if (DisableTransitions) {
                return;
            }
            if (TransitionSegmentList == null || TransitionSegmentList.SegmentList.Items.Count == 0 || TransitionSegmentChance == 0) {
                return;
            }

            // If previous segment was not a transition, current and previous segments aren't both jumpers
            // Spawn a transition segment
            if (PreviousSegment.SegmentType != SegmentTypes.Transition
                    && (PreviousSegment.SegmentType != SegmentTypes.Jumper && CurrentSegment.SegmentType != SegmentTypes.Jumper)) {

                // Chance to spawn transition
                float rng = UnityEngine.Random.Range(0f, 1f);
                if (rng <= TransitionSegmentChance) {
                    NextSegment = CurrentSegment;      // Set _nextSegment to CurrentSegment, since we will spawn a transition instead
                    int heightDelta = GetHeightDelta();
                    string key = null;

                    if (DebugMode && !string.IsNullOrEmpty(ForcedTransitionSegment)) {
                        TransitionSegmentController controller = (TransitionSegmentController) _levelSegments[ForcedTransitionSegment].GetPooledGameObject().GetComponent<LevelSegmentController>();
                        if ((heightDelta == -1 && controller.MinusTransition)
                                || (heightDelta == 0 && controller.NeutralTransition)
                                || (heightDelta == 1 && controller.PlusTransition)
                                || (heightDelta == 2 && controller.PlusTwoTransition)) {
                            CurrentSegment = controller;
                            controller.SetHeightDelta(heightDelta);
                        }
                        else {
                            return;
                        }
                    }

                    switch (heightDelta) {
                        case -1:
                            if (_minusTransitionRandomizer.Count == 0) {
                                NextSegment = null;
                                return;
                            }
                            key = _minusTransitionRandomizer.NextWithReplacement();
                            break;
                        case 0:
                            if (_neutralTransitionRandomizer.Count == 0) {
                                NextSegment = null;
                                return;
                            }
                            key = _neutralTransitionRandomizer.NextWithReplacement();
                            break;
                        case 1:
                            if (_plusTransitionRandomizer.Count == 0) {
                                NextSegment = null;
                                return;
                            }
                            key = _plusTransitionRandomizer.NextWithReplacement();
                            break;
                        case 2:
                            if (_plusTwoTransitionRandomizer.Count == 0) {
                                NextSegment = null;
                                return;
                            }
                            key = _plusTwoTransitionRandomizer.NextWithReplacement();
                            break;
                    }

                    if (key == null) {
                        Debug.Log("null transition key");
                        NextSegment = null;
                        return;
                    }
                    CurrentSegment = _levelSegments[key].GetPooledGameObject().GetComponent<LevelSegmentController>();
                    (CurrentSegment as TransitionSegmentController).SetHeightDelta(heightDelta);
                }
            }
        }

        /// <summary>
        /// Activates the current LevelSegmentController gameobject and sets its position based on the previous segment
        /// </summary>
        /// <param name="useDefaultHeight">If true, use the segment's current height</param>
        protected virtual void SpawnCurrentSegment(bool useDefaultHeight = false) {
            float gapLength = GetGapLength();
            int heightDelta;

            // Segment set active before positioning it, so that OnEnable might change its bounds first
            CurrentSegment.gameObject.SetActive(true);

            // Height delta is locked in if the previous segment was transition
            if (PreviousSegment.SegmentType == SegmentTypes.Transition) {
                heightDelta = (int)(PreviousSegment as TransitionSegmentController).StoredHeightDelta;
                _currentHeightIdx += heightDelta;
            }
            else if (CurrentSegment.SegmentType != SegmentTypes.Transition) {
                heightDelta = GetHeightDelta();
                _currentHeightIdx += heightDelta;
            }
            else {
                heightDelta = 0;
            }

            // Get x position based on bounds of previous and current segments (and gap length)
            float xPos = PreviousSegment.RightBound.x + gapLength + (CurrentSegment.transform.position.x - CurrentSegment.LeftBound.x);
            float yPos;
            // If the segment forces default height, don't change its height
            if (useDefaultHeight) {
                yPos = CurrentSegment.transform.position.y;
            }
            // Otherwise change height based on heightdelta
            else {
                yPos = PreviousSegment.transform.position.y + PreviousSegment.BoundsLine.offset.y - CurrentSegment.BoundsLine.offset.y + (heightDelta * HeightInterval);
            }
            CurrentSegment.transform.position = new Vector3(xPos, yPos);

            _activeSegments++;
        }

        /// <summary>
        /// Gets the next gap length
        /// _currentSegment is the segment that is now being spawned
        /// </summary>
        /// <returns></returns>
        protected virtual float GetGapLength() {
            if (OverrideGapLength) {
                _currentGapLength = GapLengths.Shortest;
                return ShortestGap;
            }

            // If the new or previous segment is a jumper, there must be a medium gap length
            if (CurrentSegment.SegmentType == SegmentTypes.Jumper || PreviousSegment.SegmentType == SegmentTypes.Jumper) {
                _currentGapLength = GapLengths.Medium;
            }
            // Gaps into or out of transitions with ground are always zero
            else if (PreviousSegment.SegmentType == SegmentTypes.Transition || CurrentSegment.SegmentType == SegmentTypes.Transition) {
                _currentGapLength = GapLengths.NoGap;
            }
            // Otherwise choose the gap length randomly. 50% chance for no gap or medium gap
            else {
                int rng = _randomGenerator.Next(0, 2);
                if (rng == 0) {
                    _currentGapLength = GapLengths.NoGap;
                }
                else {
                    _currentGapLength = GapLengths.Medium;
                }
            }

            return (_gapLengthsDict[_currentGapLength]);
        }

        /// <summary>
        /// Gets the height delta between the current segment and the next segment
        /// </summary>
        /// <returns>Int difference between the previous height index and the current height index (height delta to apply to previous height)</returns>
        protected virtual int GetHeightDelta() {
            int increment;

            // If there is a static set height for the segment, use that
            if (CurrentSegment.SetHeight) {
                increment = CurrentSegment.StaticHeight - _currentHeightIdx;
                return increment;
            }

            // If the last two segments are Ground and have no gap, do not change height
            if (NextSegment == null
                    && (PreviousSegment.SegmentType == SegmentTypes.Ground && CurrentSegment.SegmentType == SegmentTypes.Ground)) {
                if (_currentGapLength == GapLengths.NoGap) {
                    return 0;
                }
            }

            // Height delta can be +0, +1, +2, or -1
            // -1 is more likely if applicable
            int min = -2;
            int max = 3;
            // If we are at max height, do not allow increase in height
            if (_currentHeightIdx == NumberOfHeights) {
                max = 1;
            }
            if (NumberOfHeights - _currentHeightIdx == 1) {
                max = 2;
            }
            // If we are at min height, do not allow decrease in height
            if (_currentHeightIdx == 1) {
                min = 0;
            }
            // Range is [min,max)
            increment = _randomGenerator.Next(min, max);
            if (increment == -2) {
                increment = -1;
            }

            return increment;
        }

        /// <summary>
        /// Generate gap length based on CharacterJump and level speed
        /// </summary>
        /// <returns></returns>
        protected virtual float GenerateGapLength(JumpTypes jumpType) {
            _characterJump = LevelManagerIRE_PW.Instance.CurrentPlayableCharacters[0].GetComponent<CharacterJumpIRE>();
            float jumpTime = _characterJump.CalculateJumpTime(jumpType);
            float velocity = (LevelManagerIRE_PW.Instance.SegmentSpeed + LevelManagerIRE_PW.Instance.CurrentUnmodifiedSpeed) * LevelManagerIRE_PW.Instance.SpeedMultiplier;
            float distance = jumpTime * velocity * 0.7f;
            return distance;
        }

        /// <summary>
        /// Increments the difficulty level and adjusts segment weights accordingly
        /// </summary>
        protected virtual void IncrementDifficulty(int increment) {
            CurrentDifficulty += increment;
            PaywallDifficultyEvent.Trigger(CurrentDifficulty);
            MediumGap = GenerateGapLength(JumpTypes.Normal);

            //foreach (KeyValuePair<string, WeightedSegmentPooler> entry in _levelSegments) {
            //    // Only add weight if the segment appears at this difficulty
            //    if (entry.Value.StartingDifficulty == CurrentDifficulty) {
            //        int newWeight;
            //        switch (entry.Value.SegmentPooler.SegmentToPool.SegmentType) {
            //            case SegmentTypes.Ground:
            //                newWeight = _groundSegmentRandomizer.GetWeight(entry.Key) + 10;
            //                _groundSegmentRandomizer.SetWeight(entry.Key, newWeight);
            //                break;
            //            case SegmentTypes.Transition:
            //                newWeight = _transitionSegmentRandomizer.GetWeight(entry.Key) + 10;
            //                _transitionSegmentRandomizer.SetWeight(entry.Key, newWeight);
            //                break;
            //            case SegmentTypes.Jumper:
            //                newWeight = _jumperSegmentRandomizer.GetWeight(entry.Key) + 10;
            //                _jumperSegmentRandomizer.SetWeight(entry.Key, newWeight);
            //                break;
            //        }
            //    }
            //}

        }

        /// <summary>
        /// Gets the length of the next stage based on a formula
        /// Sets _currentStageLength
        /// </summary>
        protected virtual void GetNextStageLength() {
            _currentStageLength = BaseStageLength * Mathf.Pow(1.75f, CurrentStage - 1);
        }

        protected virtual void EnterDepot() {
            _previousStageCutoff = LevelManagerIRE_PW.Instance.DistanceTraveled;
        }

        protected virtual void LeaveDepot() {
            _blockSpawn = false;
        }

        /// <summary>
        /// Called by a spawned segment when it is recycled (OutOfBoundsRecycle_PW)
        /// </summary>
        public virtual void DecrementActiveObjects() {
            _activeSegments--;
        }

        public void OnMMEvent(MMGameEvent gameEvent) {
            if (gameEvent.EventName.Equals("EnterDepot")) {
                EnterDepot();
            }
            if (gameEvent.EventName.Equals("LeaveDepot")) {
                LeaveDepot();
            }
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<MMGameEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<MMGameEvent>();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
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
