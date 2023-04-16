using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

namespace Paywall {

    /// <summary>
    /// Generates level segments randomly
    /// </summary>
    public class ProceduralLevelGenerator : MMSingleton<ProceduralLevelGenerator> {
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public List<LevelSegment> LevelSegments { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public WeightedObjectPooler WeightedPooler { get; protected set; }
        /// List of level segments to generate in this level
        [field: Tooltip("List of level segments to generate in this level")]
        [field: SerializeField] public bool UseNextSegmentProbability { get; protected set; }
        public float Difficulty;

        protected List<MMSimpleObjectPooler> _poolerList = new();
        protected Dictionary<int, MMSimpleObjectPooler> _poolerDict = new();
        protected IWeightedRandomizer<int> _randomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<int> _originalRandomizer;
        protected Dictionary<string, LevelSegment> _levelSegments = new();

        protected virtual void Start() {
            Initialization();
        }

        protected virtual void Initialization() {
            foreach (LevelSegment segment in LevelSegments) {
                foreach (SegmentsList segmentsList in segment.NextLevelSegments) {

                }
            }
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
        /// UNUSED
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
        /// Used by SegmentsListInspectorDrawer
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
