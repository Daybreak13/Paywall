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

        // The parent object pooler (if applicable)
        public Transform ParentPooler { get; protected set; }

        protected SpawnPoint _parentSpawnPoint;

        /// <summary>
        /// Set parent (may or may not be SpawnPoint type)
        /// </summary>
        /// <param name="sp"></param>
        public virtual void SetParentSpawnPoint(SpawnPoint sp) {
            _parentSpawnPoint = sp;
        }

        public virtual void RemoveFromSpawnPoint() {
            if (_parentSpawnPoint != null) {
                _parentSpawnPoint.RemoveFromList(this);
            }
            _parentSpawnPoint = null;
        }

        /// <summary>
        /// Stores the parent pooler of this spawnable (if applicable), so that it can be reset when the object is recycled
        /// </summary>
        /// <param name="newParent"></param>
        public virtual void SetPoolerParent(Transform newParent) {
            ParentPooler = newParent;
        }

        /// <summary>
        /// Resets the parent as the original pooler
        /// </summary>
        protected virtual void ResetParent() {
            if (ParentPooler != null) {
                transform.SetParent(ParentPooler);
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            RemoveFromSpawnPoint();
            Invoke(nameof(ResetParent), 0f);
        }
    }
}
