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
        [field: SerializeField] public List<Transform> SpawnPoints { get; protected set; } = new();
        /// The object pooler to pull objects from
        [field: Tooltip("The object pooler to pull objects from")]
        [field: SerializeField] public MMObjectPooler ObjectPooler { get; protected set; }
        /// If true, repopulate spawn points OnEnable
        [field: Tooltip("If true, repopulate spawn points OnEnable")]
        [field: SerializeField] public bool RepopulateOnEnable { get; protected set; } = true;

        protected List<GameObject> _poolableObjects = new();

        protected virtual void OnEnable() {
            if (RepopulateOnEnable) {
                foreach (Transform spawnPoint in SpawnPoints) {
                    GameObject obj = ObjectPooler.GetPooledGameObject();
                    obj.transform.position = spawnPoint.position;
                    _poolableObjects.Add(obj);
                }
            }
        }

        protected virtual void OnDisable() {
            foreach (GameObject obj in _poolableObjects) {
                obj.GetComponent<MMPoolableObject>().Destroy();
            }
            _poolableObjects = new();
        }
    }
}
