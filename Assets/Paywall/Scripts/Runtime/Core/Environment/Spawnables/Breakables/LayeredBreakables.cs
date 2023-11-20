using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Controls layered breakable chains
    /// </summary>
    public class LayeredBreakables : MonoBehaviour {
        [field: SerializeField] public List<GameObject> EndCaps { get; protected set; } = new();
        /// First (bottom) breakable chain
        [field: Tooltip("First (bottom) breakable chain")]
        [field: SerializeField] public BreakableChain FirstChain { get; protected set; }
        /// Second (middle) breakable chain
        [field: Tooltip("Second (middle) breakable chain")]
        [field: SerializeField] public BreakableChain SecondChain { get; protected set; }
        /// Third (top) breakable chain
        [field: Tooltip("Third (top) breakable chain")]
        [field: SerializeField] public BreakableChain ThirdChain { get; protected set; }
        /// Ordered list (first to last) of breakable chains
        [field: Tooltip("Ordered list (first to last) of breakable chains")]
        [field: SerializeField] public List<BreakableChain> Chains { get; protected set; } = new();
        /// Parent level segment controller
        [field: Tooltip("Parent level segment controller")]
        [field: SerializeField] public LevelSegmentController ParentController { get; protected set; }

        protected virtual void Awake() {
            if (ParentController == null) {
                ParentController = GetComponentInParent<LevelSegmentController>();
            }
        }

        /// <summary>
        /// Spawns and positions BreakableChain spawnables
        /// </summary>
        protected virtual void SpawnChains() {
            Vector2 end = transform.position;
            foreach (BreakableChain chain in Chains) {
                chain.transform.position = end;
                end = chain.ForceSpawn();
            }
            ParentController.SetBounds(Vector2.zero, transform.InverseTransformPoint(end));

        }

        /// <summary>
        /// Spawn breakable chains
        /// </summary>
        protected virtual void OnEnable() {
            SpawnChains();
        }

    }
}
