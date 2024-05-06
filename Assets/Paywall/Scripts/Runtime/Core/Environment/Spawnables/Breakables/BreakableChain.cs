using Paywall.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Spawns random breakables in a pattern
    /// </summary>
    public class BreakableChain : MonoBehaviour_PW {
        /// What type of breakable to get from poolers
        [field: Tooltip("What type of breakable to get from poolers")]
        [field: SerializeField] public ScriptableSpawnType SpawnablePoolerType { get; protected set; }
        /// Minimum possible length of breakable chain
        [field: Tooltip("Minimum possible length of breakable chain")]
        [field: SerializeField] public int MinLength { get; protected set; } = 3;
        /// Maximum possible length of breakable chain
        [field: Tooltip("Maximum possible length of breakable chain")]
        [field: SerializeField] public int MaxLength { get; protected set; } = 3;
        /// Spawn via script call instead of OnEnable
        [field: Tooltip("Spawn via script call instead of OnEnable")]
        [field: SerializeField] public bool ManualSpawn { get; protected set; } = true;

        protected List<GameObject> _spawnables = new();
        protected static System.Random _random;

        /// <summary>
        /// Set min and max lengths
        /// Pass in a negative value for min or max to retain the previous length
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public virtual void SetLengths(int min, int max) {
            if (min > 0) MinLength = min;
            if (max > 0 && max >= MinLength) MaxLength = max;
        }

        /// <summary>
        /// Call this to manually trigger Spawn()
        /// </summary>
        public virtual Vector2 ForceSpawn() {
            if (ManualSpawn) return Spawn();
            return Vector2.zero;
        }

        /// <summary>
        /// Spawn breakables at regular intervals in a horizontal chain with random variable length
        /// Spawn starts offset length / 2, so that the parent transform remains in the middle of the chain
        /// Spawn goes left to right
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Vector2 position of the rightmost border</returns>
        protected virtual Vector2 Spawn(int min = -1, int max = -1) {
            int a, b;
            if (min < 0 && max < 0) {
                a = MinLength; b = MaxLength;
            }
            else {
                a = min; b = max;
            }
            _random ??= RandomManager.NewRandom(PaywallProgressManager.RandomSeed);
            int length = _random.Next(a, b + 1);
            float offset =  0.5f;     // -length / 2f + 0.5f
            Vector2 prevPosition = new(transform.position.x + offset, transform.position.y);
            Vector2 currentPos = Vector2.zero;
            for (int i = 0; i < length; i++) {
                GameObject spawnable = ProceduralLevelGenerator.Instance.SpawnPoolerDict[SpawnablePoolerType.ID].Pooler.GetPooledGameObject();
                spawnable.SetActive(true);
                spawnable.transform.SetParent(transform);
                _spawnables.Add(spawnable);
                spawnable.transform.position = prevPosition;
                currentPos = prevPosition;
                prevPosition.x += 1f;
            }

            currentPos.x += 0.5f;
            return currentPos;
        }

        protected virtual void OnEnable() {
            if (!ManualSpawn) Spawn();
        }
    }
}
