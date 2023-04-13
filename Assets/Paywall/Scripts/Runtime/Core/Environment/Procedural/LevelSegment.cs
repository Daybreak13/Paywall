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
    }

    [System.Serializable]
    public class LevelSegment {
        public string Label;
        public LevelSegmentController SegmentController;
        public List<SegmentsList> NextLevelSegments;
        //public List<string> BlockedLevelSegments;
    }


}
