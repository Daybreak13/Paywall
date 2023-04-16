using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

namespace Paywall {

    [System.Serializable]
    public class WeightedPool {
        public MMSimpleObjectPooler Pooler;
        public int Weight = 10;
        public int ModWeight { 
            get {
                return (int)(Weight * DifficultyMod * ProceduralLevelGenerator.Instance.Difficulty); 
            }
        }
        public float DifficultyMod = 1f;
    }

    /// <summary>
    /// Uses a library to pull an specific object type at random depending on its weight
    /// </summary>
    public class WeightedObjectPooler : MonoBehaviour {

        /// the list of simple object pools
        [field: Tooltip("the list of simple object pools")]
		[field: SerializeField] public List<WeightedPool> Pool { get; protected set; }

        public List<WeightedObjectPooler> Owner { get; set; }
        private void OnDestroy() { Owner?.Remove(this); }

        protected List<MMSimpleObjectPooler> _poolerList = new();
        protected Dictionary<int, MMSimpleObjectPooler> _poolerDict = new();
        protected IWeightedRandomizer<int> _randomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<int> _originalRandomizer;

        /// <summary>
        /// Initialize lists
        /// </summary>
        protected virtual void Start() {
            int key = 0;
            foreach (WeightedPool pooler in Pool) {
                _poolerDict.Add(key, pooler.Pooler);
                _randomizer.Add(key, pooler.Weight);
                key++;
            }
            _originalRandomizer = _randomizer;
        }
        
        /// <summary>
        /// Pulls a random object from the pool
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetPooledGameObject() {
            if (Pool.Count == 0) {
                return null;
            }
            int key = _randomizer.NextWithReplacement();
            if (_poolerDict.TryGetValue(key, out MMSimpleObjectPooler pooler)) {
                return pooler.GetPooledGameObject();
            }
            return null;
        }

    }
}
