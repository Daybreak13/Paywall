using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

namespace Paywall {

    [System.Serializable]
    public class WeightedPooler {
        public MMSimpleObjectPooler Pooler;
        public int Weight = 1;
    }

    /// <summary>
    /// Uses a library to pull an specific object type at random depending on its weight
    /// </summary>
    public class WeightedObjectPooler : MonoBehaviour {

        /// the list of simple object pools
        [field: Tooltip("the list of simple object pools")]
		[field: SerializeField] public List<WeightedPooler> Pool { get; protected set; }

        public List<WeightedObjectPooler> Owner { get; set; }
        private void OnDestroy() { Owner?.Remove(this); }

        protected List<MMSimpleObjectPooler> _poolerList = new List<MMSimpleObjectPooler>();
        protected Dictionary<int, MMSimpleObjectPooler> _poolerDict = new Dictionary<int, MMSimpleObjectPooler>();
        protected IWeightedRandomizer<int> _randomizer = new DynamicWeightedRandomizer<int>();
        protected IWeightedRandomizer<int> _originalRandomizer;

        /// <summary>
        /// Initialize lists
        /// </summary>
        protected virtual void Start() {
            int key = 0;
            foreach (WeightedPooler pooler in Pool) {
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
