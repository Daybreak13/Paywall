using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    [System.Serializable]
    public class SegmentsList {
        [MMReadOnly, HideInInspector]
        public int CurrentIndex;
        public void SetCurrentIndex(int idx) {
            CurrentIndex = idx;
        }
    }

    [System.Serializable]
    public class LevelSegment {
        [field: SerializeField] public string Label { get; protected set; }
        [field: SerializeField] public LevelSegmentController SegmentController { get; protected set; }
        [field: SerializeField] public List<SegmentsList> NextLevelSegments { get; protected set; }
        public Dictionary<string, LevelSegment> NextSegmentDict { get; protected set; } = new();
    }

    [System.Serializable]
    public class WeightedLevelSegment {
        [field: SerializeField] public LevelSegmentPooler SegmentPooler { get; set; }
        [field: SerializeField] public int InitialWeight { get; protected set; }
        [field: SerializeField] public int StartingDifficulty { get; protected set; }
        public int CurrentWeight { get; protected set; }
        public void SetWeight(int weight) {
            CurrentWeight = weight;
        }

    }

    [System.Serializable]
    public class WeightedLevelSegmentList {
        [field: SerializeField] public string Category { get; protected set; }
        [field: SerializeField] public List<WeightedLevelSegment> LevelSegments { get; set; }
    }
}
