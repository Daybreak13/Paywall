using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public static class PaywallLayerManager {
        private static readonly int PlayerLayer = 7;

        public static int PlayerLayerMask = 1 << PlayerLayer;

    }
}
