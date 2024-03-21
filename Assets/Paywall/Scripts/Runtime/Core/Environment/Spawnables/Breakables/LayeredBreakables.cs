using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Controls layered breakable chains
    /// </summary>
    public class LayeredBreakables : MonoBehaviour {
        [field: SerializeField] public List<GameObject> EndCaps { get; protected set; } = new();
        /// Ordered list (first to last) of breakable chains
        [field: Tooltip("Ordered list (first to last) of breakable chains")]
        [field: SerializeField] public List<BreakableChain> Chains { get; protected set; } = new();
        /// Parent level segment controller
        [field: Tooltip("Parent level segment controller")]
        [field: SerializeField] public LevelSegmentController ParentController { get; protected set; }
        /// Set parent controller bounds when spawning?
        [field: Tooltip("Set parent controller bounds when spawning?")]
        [field: SerializeField] public bool SetParentBounds { get; protected set; } = true;

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
            if (ParentController == null) {
                foreach (BreakableChain chain in Chains) {
                    chain.transform.position = end;
                    end = chain.ForceSpawn();
                    end = new Vector2(end.x, end.y + ProceduralLevelGenerator.Instance.HeightInterval);
                }
            }
            else {
                int increments = (int)((ParentController as TransitionSegmentController).StoredHeightDelta / ProceduralLevelGenerator.Instance.HeightInterval);
                int mod = increments > 0 ? 1 : -1;
                for (int i = 0; i <= Mathf.Abs(increments); i++) {
                    Chains[i].transform.position = end;
                    end = Chains[i].ForceSpawn();
                    end = new Vector2(end.x, end.y + mod * ProceduralLevelGenerator.Instance.HeightInterval);
                }

                if (SetParentBounds) {
                    ParentController.SetBounds(Vector2.zero, transform.InverseTransformPoint(end));
                }
            }
        }

        /// <summary>
        /// Spawn breakable chains
        /// </summary>
        protected virtual void OnEnable() {
            SpawnChains();
        }

    }
}
