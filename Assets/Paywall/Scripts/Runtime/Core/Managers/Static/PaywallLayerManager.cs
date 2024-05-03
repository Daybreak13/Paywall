using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public static class PaywallLayerManager {
        private static readonly int PlayerLayer = 7;
        private static readonly int GroundLayer = 8;
        private static readonly int OneWayPlatformsLayer = 9;
        private static readonly int EnemyLayer = 13;
        private static readonly int DodgingLayer = 20;

        public static readonly int PlayerLayerMask = 1 << PlayerLayer;
        public static readonly int GroundLayerMask = 1 << GroundLayer;
        public static readonly int OneWayPlatformsLayerMask = 1 << OneWayPlatformsLayer;
        public static readonly int EnemyLayerMask = 1 << EnemyLayer;
        public static readonly int DodgingLayerMask = 1 << DodgingLayer;

        public static readonly int ObstaclesLayerMask = GroundLayerMask | OneWayPlatformsLayerMask;
    }
}
