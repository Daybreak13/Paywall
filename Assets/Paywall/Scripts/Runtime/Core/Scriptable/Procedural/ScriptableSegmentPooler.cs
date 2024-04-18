using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class ScriptableSegmentPooler : ScriptableObject {
        /// The segment prefab to assign to a new pooler
		[field: Tooltip("The segment prefab to assign to a new pooler")]
        [field: SerializeField] public LevelSegmentController SegmentPrefab { get; protected set; }
        /// The weight of this level segment
		[field: Tooltip("The weight of this level segment")]
        [field: SerializeField] public int InitialWeight { get; protected set; }
    }
}
