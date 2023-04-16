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
        public int Weight;
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


}
