using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class LevelSpeedManager : MonoBehaviour, ISpeedManager {
        [field: Header("Speed")]
        /// the initial speed of the level
        [field: SerializeField] public float InitialSpeed { get; protected set; } = 30f;
        /// the maximum speed the level will run at
        [field: SerializeField] public float MaximumSpeed { get; protected set; } = 50f;
        /// the maximum speed the level will run at
        [field: SerializeField] public float SpeedIncrement { get; protected set; } = 7.5f;
        /// Current speed not counting temp speed modifiers
        [field: MMReadOnly]
        [field: SerializeField] public float CurrentUnmodifiedSpeed { get; protected set; }
        /// the global speed modifier for level segments
        [field: Tooltip("the global speed modifier for level segments")]
        [field: SerializeField] public float SegmentSpeed { get; protected set; } = 0f;

        public float GetSpeed() {
            throw new System.NotImplementedException();
        }

        public void SetSpeed(float speed) {
            throw new System.NotImplementedException();
        }
    }
}
