using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [System.Serializable]
    public class SegmentListData {
        public GameObject PoolParent;
        public ScriptableSegmentList SegmentList;
    }

    [CreateAssetMenu(fileName = "SegmentList", menuName = "Paywall/Procedural/SegmentList")]
    public class ScriptableSegmentList : ScriptableList<WeightedLevelSegment> {
        
    }
}
