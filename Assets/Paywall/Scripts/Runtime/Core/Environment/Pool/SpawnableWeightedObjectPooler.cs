using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weighted_Randomizer;
using MoreMountains.Tools;

namespace Paywall {

    [System.Serializable]
    public class SpawnableWeightedPool {
        public SpawnableObjectPooler Pooler;
        public int InitialWeight = 10;
        public int StartingDifficulty;
        [field: MMReadOnly]
        [field: SerializeField] public int CurrentWeight { get; protected set; }
        public void SetWeight(int weight) {
            CurrentWeight = weight;
        }
    }

    /// <summary>
    /// Weighted pooler for spawnables
    /// </summary>
    public class SpawnableWeightedObjectPooler : MonoBehaviour, MMEventListener<PaywallDifficultyEvent> {
        /// the type of pooler this is
        [field: Tooltip("the type of pooler this is")]
        [field: SerializeField] public SpawnablePoolerTypes SpawnablePoolerType { get; protected set; }
        /// the list of simple object pools
        [field: Tooltip("the list of simple object pools")]
        [field: SerializeField] public List<SpawnableWeightedPool> WeightedPools { get; protected set; }

        public List<SpawnableWeightedObjectPooler> Owner { get; set; }
        private void OnDestroy() { Owner?.Remove(this); }

        protected List<SpawnableObjectPooler> _poolerList = new();
        protected Dictionary<string, SpawnableObjectPooler> _poolerDict = new();
        protected IWeightedRandomizer<string> _randomizer = new DynamicWeightedRandomizer<string>();
        protected IWeightedRandomizer<string> _originalRandomizer;

        /// <summary>
        /// Initialize lists
        /// </summary>
        protected virtual void Start() {
            Initialization();
        }

        public virtual void Initialization() {
            int key = 0;
            foreach (SpawnableWeightedPool pooler in WeightedPools) {
                _poolerDict.Add(pooler.Pooler.SpawnableToPool.name, pooler.Pooler);
                _randomizer.Add(pooler.Pooler.SpawnableToPool.name, pooler.InitialWeight);
                key++;
            }
            _originalRandomizer = _randomizer;
        }

        /// <summary>
        /// Pulls a random object from the pool
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetPooledGameObject() {
            if (WeightedPools.Count == 0) {
                return null;
            }
            string key = _randomizer.NextWithReplacement();
            if (_poolerDict.TryGetValue(key, out SpawnableObjectPooler pooler)) {
                return pooler.GetPooledGameObject();
            }
            return null;
        }

        protected virtual void SetDifficulty(int difficulty) {
            foreach (SpawnableWeightedPool pooler in WeightedPools) {
                int newWeight = _originalRandomizer.GetWeight(pooler.Pooler.SpawnableToPool.name) + difficulty * 10;
                _randomizer.SetWeight(pooler.Pooler.SpawnableToPool.name, newWeight);
            }
        }

        public virtual void OnMMEvent(PaywallDifficultyEvent difficultyEvent) {
            SetDifficulty(difficultyEvent.CurrentDifficulty);
        }

        protected virtual void OnEnable() {

        }

        protected virtual void OnDisable() {

        }

    }
}
