using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

    /// <summary>
    /// Poolable object class for spawnables (items, pickups, enemies, etc.)
    /// Can set the spawn offset of the object here
    /// </summary>
    public class SpawnablePoolableObject : MMPoolableObject {
        [field: Header("Spawnable")]

        /// The object's spawn offset (offset to the position in which it is spawned)
        [field: Tooltip("The object's spawn offset (offset to the position in which it is spawned)")]
        [field: SerializeField] public Vector3 SpawnOffset { get; protected set; }
        /// If true, set this object's parent as the spawn point when spawned
        [field: Tooltip("If true, set this object's parent as the spawn point when spawned")]
        [field: SerializeField] public bool AnchorToSpawn { get; protected set; }

        protected SpawnPoint _parentSpawnPoint;

        public virtual void SetParentSpawnPoint(SpawnPoint sp) {
            _parentSpawnPoint = sp;
        }

        public virtual void RemoveFromSpawnPoint() {
            if (_parentSpawnPoint != null) {
                _parentSpawnPoint.RemoveFromList(this);
            }
            _parentSpawnPoint = null;
        }

        protected override void OnDisable() {
            base.OnDisable();
            RemoveFromSpawnPoint();
        }

    }
}
