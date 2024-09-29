using UnityEngine;

namespace Paywall
{

    [System.Serializable]
    public class SegmentListData
    {
        public GameObject PoolParent;
        public ScriptableSegmentList SegmentList;
    }

    [CreateAssetMenu(fileName = "SegmentList", menuName = "Paywall/Procedural/SegmentList")]
    public class ScriptableSegmentList : ScriptableList<WeightedLevelSegment>
    {
        /// Global initial weight for weighted segments in the list
        [field: Tooltip("Global initial weight for weighted segments in the list")]
        [field: SerializeField] public int GlobalInitialWeight { get; protected set; } = 10;
        /// Global starting difficulty for weighted segments in the list
        [field: Tooltip("Global starting difficulty for weighted segments in the list")]
        [field: SerializeField] public int GlobalStartingDifficulty { get; protected set; } = 0;

    }
}
