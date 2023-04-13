using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

namespace Paywall {

    public class LevelSegmentController : MonoBehaviour {
        public string SegmentName;
        /// the list of simple object pools
        [field: Tooltip("the list of simple object pools")]
        [field: SerializeField] public List<WeightedPool> Pools { get; protected set; }


    }

}
