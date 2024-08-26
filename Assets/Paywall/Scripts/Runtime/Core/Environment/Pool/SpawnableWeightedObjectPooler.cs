using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using KaimiraGames;

namespace Paywall {

    /// <summary>
    /// A weighted pool for a single spawnable object class
    /// </summary>
    [System.Serializable]
    public class SpawnableWeightedPool {
        [field: SerializeField] public SpawnableObjectPooler Pooler;
        [field: SerializeField] public int InitialWeight = 10;
        [field: SerializeField] public int MaxWeight { get; protected set; } = 10;
        [field: SerializeField] public int StartingDifficulty;
        [field: MMReadOnly]
        [field: SerializeField] public int CurrentWeight { get; protected set; }
        public void SetWeight(int weight) {
            CurrentWeight = weight;
        }
    }

    /// <summary>
    /// Weighted pooler for spawnables
    /// Each object of this class contains various spawnables of a single type, each with different weights
    /// When a specific type of spawnable is required, this pooler randomly spits one out of that type
    /// </summary>
    public class SpawnableWeightedObjectPooler : MonoBehaviour, MMEventListener<PaywallDifficultyEvent> {
        /// the type of spawnable this pooler contains
        [field: Tooltip("the type of spawnable this pooler contains")]
        [field: SerializeField] public ScriptableSpawnType SpawnablePoolerType { get; protected set; }
        /// the list of object pools
        [field: Tooltip("the list of object pools")]
        [field: SerializeField] public List<SpawnableWeightedPool> WeightedPools { get; protected set; }

        public List<SpawnableWeightedObjectPooler> Owner { get; set; }
        private void OnDestroy() { Owner?.Remove(this); }

        protected Dictionary<string, SpawnableWeightedPool> _poolerDict = new();    // Currently used poolers
        protected Dictionary<int, List<SpawnableWeightedPool>> _unusedPoolers = new();      // Currently unused poolers (not spawning at current difficulty)
        protected WeightedList<string> _randomizer;

        protected virtual void Start() {
            Initialization();
        }

        /// <summary>
        /// Initialize lists/dictionaries
        /// </summary>
        public virtual void Initialization() {
            _randomizer = new(new System.Random(PaywallProgressManager.RandomSeed));
            foreach (SpawnableWeightedPool pooler in WeightedPools) {
                if (pooler.StartingDifficulty == 0) {
                    _poolerDict.Add(pooler.Pooler.SpawnableToPool.name, pooler);
                    _randomizer.Add(pooler.Pooler.SpawnableToPool.name, pooler.InitialWeight);
                }
                // Poolers that don't spawn at Difficulty 0 are set aside to be added later
                else {
                    if (_unusedPoolers.ContainsKey(pooler.StartingDifficulty)) {
                        _unusedPoolers[pooler.StartingDifficulty].Add(pooler);
                    }
                    else {
                        _unusedPoolers.Add(pooler.StartingDifficulty, new List<SpawnableWeightedPool> { pooler });
                    }
                }
            }
        }

        /// <summary>
        /// Pulls a random object from a random pool
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetPooledGameObject() {
            if (WeightedPools.Count == 0) {
                return null;
            }
            if (_randomizer.Count == 0) {
                return null;
            }
            string key = _randomizer.Next();
            if (_poolerDict.TryGetValue(key, out SpawnableWeightedPool pooler)) {
                return pooler.Pooler.GetPooledGameObject();
            }
            return null;
        }

        /// <summary>
        /// Modify weights of each pooler when difficulty is incremented
        /// </summary>
        /// <param name="difficulty"></param>
        protected virtual void SetDifficulty(int difficulty) {
            foreach (SpawnableWeightedPool pool in _poolerDict.Values) {
                if (pool.CurrentWeight >= pool.MaxWeight) {
                    continue;
                }
                int newWeight = pool.InitialWeight + ProceduralLevelGenerator.Instance.WeightIncrement;
                _randomizer.SetWeight(pool.Pooler.SpawnableToPool.name, newWeight);
                pool.SetWeight(newWeight);
            }
            // Add any poolers that start spawning at this difficulty
            if (_unusedPoolers.ContainsKey(difficulty)) {
                foreach (SpawnableWeightedPool pool in _unusedPoolers[difficulty]) {
                    _poolerDict.Add(pool.Pooler.SpawnableToPool.name, pool);
                    _randomizer.Add(pool.Pooler.SpawnableToPool.name, pool.InitialWeight);
                }
            }
        }

        public virtual void OnMMEvent(PaywallDifficultyEvent difficultyEvent) {
            SetDifficulty(difficultyEvent.CurrentDifficulty);
        }

        protected virtual void OnEnable() {
            this.MMEventStartListening<PaywallDifficultyEvent>();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<PaywallDifficultyEvent>();
        }

    }
}
