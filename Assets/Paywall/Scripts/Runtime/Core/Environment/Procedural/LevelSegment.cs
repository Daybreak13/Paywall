using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall
{

    [System.Serializable]
    public class SegmentsList
    {
        [MMReadOnly, HideInInspector]
        public int CurrentIndex;
        public void SetCurrentIndex(int idx)
        {
            CurrentIndex = idx;
        }
    }

    [System.Serializable]
    public class LevelSegment
    {
        [field: SerializeField] public string Label { get; protected set; }
        [field: SerializeField] public LevelSegmentController SegmentController { get; protected set; }
        [field: SerializeField] public List<SegmentsList> NextLevelSegments { get; protected set; }
        public Dictionary<string, LevelSegment> NextSegmentDict { get; protected set; } = new();
    }

    [System.Serializable]
    public class WeightedLevelSegment
    {
        [field: SerializeField] public LevelSegmentController Segment { get; set; }
        [field: SerializeField] public int InitialWeight { get; set; } = 10;
        [field: SerializeField] public int StartingDifficulty { get; set; }
        [field: NonSerialized]
        public int CurrentWeight { get; protected set; }
        public void SetWeight(int weight)
        {
            CurrentWeight = weight;
        }
    }
}
