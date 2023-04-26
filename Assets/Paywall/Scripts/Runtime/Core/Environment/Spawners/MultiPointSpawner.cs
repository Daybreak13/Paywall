using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;

namespace Paywall {

    /// <summary>
    /// Spawns a set of poolableobjects in a set pattern
    /// </summary>
    public class MultiPointSpawner : MonoBehaviour {
        /// List of spawn points
        [field: Tooltip("List of spawn points")]
        [field: SerializeField] public List<Transform> SpawnPointTransforms { get; protected set; } = new();
        /// List of spawn points
        [field: Tooltip("List of spawn points")]
        [field: SerializeField] public List<Vector3> SpawnPointVectors { get; protected set; } = new();
        /// The object pooler to pull objects from
        [field: Tooltip("The object pooler to pull objects from")]
        [field: SerializeField] public MMObjectPooler ObjectPooler { get; protected set; }
        /// If true, repopulate spawn points OnEnable
        [field: Tooltip("If true, repopulate spawn points OnEnable")]
        [field: SerializeField] public bool RepopulateOnEnable { get; protected set; } = true;

        protected List<GameObject> _poolableObjects = new();
        protected bool _initialized = false;

        protected virtual void Start() {
            Initialization();
        }

        protected virtual void Initialization() {
            if (!_initialized) {

                _initialized = true;
            }
        }

        protected virtual void OnEnable() {
            if (!_initialized) {
                Initialization();
            }
            if (RepopulateOnEnable) {
                Repopulate();
            }
        }

        protected virtual void Repopulate() {
            if (SpawnPointTransforms != null) {
                foreach (Transform spawnPoint in SpawnPointTransforms) {
                    GameObject obj = ObjectPooler.GetPooledGameObject();
                    obj.transform.position = spawnPoint.position;
                    _poolableObjects.Add(obj);
                }
            } else {
                foreach (Vector3 vector in SpawnPointVectors) {
                    GameObject obj = ObjectPooler.GetPooledGameObject();
                    obj.transform.position = vector;
                    _poolableObjects.Add(obj);
                }
            }
        }

        protected virtual void DestroyObjects() {
            foreach (GameObject obj in _poolableObjects) {
                obj.GetComponent<MMPoolableObject>().Destroy();
            }
            _poolableObjects = new();
        }

        protected virtual void OnDisable() {
            DestroyObjects();
        }

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            foreach (Vector3 vector in SpawnPointVectors) {
                Gizmos.DrawWireSphere(this.transform.position + vector, 0.1f);
            }
        }


    }
}
