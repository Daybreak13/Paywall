using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [System.Serializable]
    public struct SpawnableObject {
        // The prefab to spawn
        public GameObject prefab;
        // Spawn chance for the prefab
        [Range(0f, 1f)]
        public float spawnChance;
    }

    public class Spawner : MonoBehaviour {
        
        /// List of spawnable object prefabs
        [Tooltip("List of spawnable object prefabs")]
        public List<SpawnableObject> Spawnables = new List<SpawnableObject>();

        /// The minimum delay to wait to spawn another object
        [Tooltip("The minimum delay to wait to spawn another object")]
        public float MinSpawnDelay = 1f;
        /// The maximum delay to wait to spawn another object
        [Tooltip("The maximum delay to wait to spawn another object")]
        public float MaxSpawnDelay = 2f;

        protected virtual void Start() {
            SpawnableComparer comp = new SpawnableComparer();
            Spawnables.Sort(comp);
        }

        protected virtual void OnEnable() {
            if (Spawnables.Count > 0) {
                Invoke(nameof(Spawn), Random.Range(MinSpawnDelay, MaxSpawnDelay));
            }            
        }

        protected virtual void OnDisable() {
            CancelInvoke();
        }

        protected virtual void Spawn() {
            float spawnChance = Random.value;

            foreach (var obj in Spawnables) {
                if (spawnChance < obj.spawnChance) {
                    GameObject obstacle = Instantiate(obj.prefab);
                    obstacle.transform.position += transform.position;
                    break;
                }
            }

            Invoke(nameof(Spawn), Random.Range(MinSpawnDelay, MaxSpawnDelay));
        }

    }
}
