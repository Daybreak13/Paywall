using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

namespace Paywall {

    [System.Serializable]
    public class WeightedSegmentTypeList {
        [field: SerializeField] public SegmentTypes SegmentType { get; protected set; }
        [field: SerializeField] public List<WeightedLevelSegmentList> LevelSegments { get; protected set; }
        [field: SerializeField] public int GroundSegmentWeight { get; set; }
        [field: SerializeField] public int TransitionSegmentWeight { get; set; }
        [field: SerializeField] public int JumperSegmentWeight { get; set; }

    }

    public enum GapLengths { NoGap, Shortest, Short, Medium, Long, Longest }

    /// <summary>
    /// Generates level segments randomly
    /// </summary>
    public class ProceduralLevelGenerator : MMSingleton<ProceduralLevelGenerator> {

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

        [field: Header("Other Settings")]

        /// The maximum number of active segments to maintain
        [field: Tooltip("The maximum number of active segments to maintain")]
        [field: SerializeField] public int MaxActiveSegments { get; protected set; } = 5;
        /// How long to wait after scene load to start spawning segments
        [field: Tooltip("How long to wait after scene load to start spawning segments")]
        [field: SerializeField] public float StartDelay { get; protected set; } = 1f;

        #endregion

        protected List<MMSimpleObjectPooler> _poolerList = new();
        protected Dictionary<int, MMSimpleObjectPooler> _poolerDict = new();

        protected Dictionary<string, WeightedLevelSegment> _levelSegments = new();

        protected IWeightedRandomizer<int> _typeRandomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<string> _groundSegmentRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _transitionSegmentRandomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _jumperSegmentRandomizer = new DynamicWeightedRandomizer<string>();

        protected LevelSegmentController _previousSegment;
        protected LevelSegmentController _currentSegment;
        protected int _activeSegments = 0;
        protected GapLengths _currentGapLength;
        protected bool _shouldStart;
        protected Coroutine _startCoroutine;

        protected override void Awake() {
            base.Awake();

            _typeRandomizer.Add((int)SegmentTypes.Ground, GroundSegmentWeight);
            _typeRandomizer.Add((int)SegmentTypes.Transition, TransitionSegmentWeight);
            _typeRandomizer.Add((int)SegmentTypes.Jumper, JumperSegmentWeight);

            if ((GroundSegmentList != null) && (GroundSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in GroundSegmentList) {
                    _groundSegmentRandomizer.Add(segment.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentName, segment);
                }
            }
            if ((TransitionSegmentList != null) && (TransitionSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in TransitionSegmentList) {
                    _transitionSegmentRandomizer.Add(segment.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentName, segment);
                }
            }
            if ((JumperSegmentList != null) && (JumperSegmentList.Count > 0)) {
                foreach (WeightedLevelSegment segment in JumperSegmentList) {
                    _jumperSegmentRandomizer.Add(segment.SegmentName, segment.InitialWeight);
                    _levelSegments.Add(segment.SegmentName, segment);
                }
            }

            _startCoroutine = StartCoroutine(StartCo());
        }

        /// <summary>
        /// Delay before starting spawns
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator StartCo() {
            if (FirstLevelSegment.GetComponent<MovingRigidbody>() != null) {
                FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = false;
                yield return new WaitForSeconds(StartDelay);
                FirstLevelSegment.GetComponent<MovingRigidbody>().enabled = true;
            }
            _shouldStart = true;
        }

        protected virtual void Start() {
            Initialization();
        }

        protected virtual void Initialization() {
            
        }

        protected virtual void Update() {
            if (_shouldStart && (_activeSegments < MaxActiveSegments)) {
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
            SegmentTypes typeToUse = (SegmentTypes) _typeRandomizer.NextWithReplacement();
            typeToUse = SegmentTypes.Ground;
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

            _currentSegment = _levelSegments[key].SegmentObjectPooler.GetPooledGameObject().GetComponent<LevelSegmentController>();

        }

        /// <summary>
        /// Activates the current LevelSegmentController gameobject and sets its position based on the previous segment
        /// </summary>
        protected virtual void SpawnCurrentSegment() {
            _currentSegment.gameObject.SetActive(true);
            _activeSegments++;

            switch (_previousSegment.SegmentType) {
                case SegmentTypes.Ground:
                    _currentGapLength = (GapLengths)Random.Range(0, 3);
                    break;
                case SegmentTypes.Transition:
                    _currentGapLength = (GapLengths)Random.Range(0, 3);
                    break;
                case SegmentTypes.Jumper:
                    
                    break;
            }
            _currentGapLength = GapLengths.Medium;
            Vector3 gap = new Vector3(MediumGap, 0f);

            _currentSegment.transform.position = _previousSegment.transform.position + _previousSegment.RightBound + gap - _currentSegment.LeftBound;
        }

        /// <summary>
        /// Called when a spawned segment is recycled
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
