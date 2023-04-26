using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class DMSimpleObjectPooler : MMSimpleObjectPooler {
        [field: SerializeField] public string SegmentName { get; protected set; }
    }
}
