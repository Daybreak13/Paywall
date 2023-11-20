using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Level segment controller for transition segments
    /// Each transition segment only works on certain height deltas
    /// </summary>
    public class TransitionSegmentController : LevelSegmentController {
        [field: Header("Transition")]

        /// 
        [field: Tooltip("")]
        [field: SerializeField] public bool NeutralTransition { get; protected set; }
        /// 
        [field: Tooltip("")]
        [field: SerializeField] public bool PlusTransition { get; protected set; }
        /// 
        [field: Tooltip("")]
        [field: SerializeField] public bool PlusTwoTransition { get; protected set; }
        /// 
        [field: Tooltip("")]
        [field: SerializeField] public bool MinusTransition { get; protected set; }

        public float StoredHeightDelta { get; protected set; }

        /// <summary>
        /// Sets stored height delta
        /// </summary>
        /// <param name="heightDelta"></param>
        public virtual void SetHeightDelta(float heightDelta) {
            StoredHeightDelta = heightDelta;
        }

        protected virtual void SpawnHeightHandler() {
            int heightIntervalDelta = (int) (StoredHeightDelta / ProceduralLevelGenerator.Instance.HeightInterval);

        }
    }
}
