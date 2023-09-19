using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weighted_Randomizer;
using MoreMountains.Tools;
using Paywall.Tools;

namespace Paywall {

    /// <summary>
    /// A single spawn pooler with weight and associated spawn patterns
    /// </summary>
    [System.Serializable]
    public class SingleSpawner {
        /// The name of the pooler from ProceduralLevelGenerator to get a spawnable from
        [field: Tooltip("The name of the pooler from ProceduralLevelGenerator to get a spawnable from")]
        [field: MMReadOnly]
        [field: SerializeField] public string SpawnablePoolerName { get; protected set; }
        /// The name of the pooler from ProceduralLevelGenerator to get a spawnable from
        [field: Tooltip("The name of the pooler from ProceduralLevelGenerator to get a spawnable from")]
        [field: SerializeField] public SpawnablePoolerTypes SpawnablePoolerType { get; protected set; }
        /// The list of spawn patterns that this spawner can spawn objects in (optional)
        [field: Tooltip("The list of spawn patterns that this spawner can spawn objects in (optional)")]
        [field: SerializeField] public List<SerializedSpawnPattern> SpawnPatterns { get; protected set; }
        /// The weight of this pooler
        [field: Tooltip("The weight of this pooler")]
        [field: SerializeField] public int InitialWeight { get; protected set; } = 10;

        public bool UsingPatterns => (SpawnPatterns != null) && (SpawnPatterns.Count > 0);
        public IWeightedRandomizer<int> PatternRandomizer = new DynamicWeightedRandomizer<int>();

        /// <summary>
        /// Sets the name of this pooler based on its SpawnablePoolerType
        /// </summary>
        public void SetName() {
            SpawnablePoolerName = SpawnablePoolerType.ToString();
        }

        /// <summary>
        /// Sets the name of this pooler
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name) {
            SpawnablePoolerName = name;
        }
    }

    [System.Serializable]
    public class SerializedSpawnPattern {
        [field: SerializeField] public SpawnPattern Pattern { get; set; }
        [field: SerializeField] public int Weight { get; set; } = 10;
    }

    /// <summary>
    /// Spawn point used by LevelSegmentController. Randomly spawns a spawnable in its position.
    /// </summary>
    [System.Serializable]
    public class SpawnPoint : MonoBehaviour, MMEventListener<PaywallChanceUpdateEvent> {
        /// If false, use ProceduralLevelGenerator.NoneChance instead
        [field: Tooltip("If false, use ProceduralLevelGenerator.NoneChance instead")]
        [field: SerializeField] public bool UseLocalNoneChance { get; protected set; } = false;
        /// Chance of this spawn point spawning nothing on Spawn()
        [field: Tooltip("Chance of this spawn point spawning nothing on Spawn()")]
        [field: FieldCondition("UseLocalNoneChance", true)]
        [field: SerializeField] public float LocalNoneChance { get; protected set; } = 5f;
        /// The list of spawners that this spawn point will retrieve spawnables from
        [field: Tooltip("The list of spawners that this spawn point will retrieve spawnables from")]
        [field: SerializeField] public List<SingleSpawner> Spawners { get; protected set; }

        protected IWeightedRandomizer<string> _spawnerRandomizer = new DynamicWeightedRandomizer<string>();
        protected Dictionary<string, SingleSpawner> _singleSpawners = new();
        protected List<SpawnablePoolableObject> _spawnables = new();
        protected LevelSegmentController _parentController;
        protected Rigidbody2D _parentRigidbody;

        /// <summary>
        /// Fill out dictionaries and randomizers
        /// </summary>
        protected virtual void Awake() {
            _parentController = GetComponentInParent<LevelSegmentController>();
            _parentRigidbody = GetComponentInParent<Rigidbody2D>();

            foreach (SingleSpawner ss in Spawners) {
                if (ss.UsingPatterns) {
                    int i = 0;
                    foreach (SerializedSpawnPattern pattern in ss.SpawnPatterns) {
                        ss.PatternRandomizer.Add(i, pattern.Weight);
                        i++;
                    }
                }
                ss.SetName();
                _singleSpawners.Add(ss.SpawnablePoolerName, ss);
                _spawnerRandomizer.Add(ss.SpawnablePoolerName, ss.InitialWeight);
            }
        }

        protected virtual bool CheckShouldSpawn() {
            float chance = UnityEngine.Random.Range(0f, 100f);
            if (UseLocalNoneChance) {
                if (LocalNoneChance > chance) {
                    return true;
                }
            }
            else {
                if (ProceduralLevelGenerator.Instance.NoneChance > chance) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Spawns and positions pooled spawnables
        /// </summary>
        protected virtual void Spawn() {
            if (GameManagerIRE_PW.HasInstance) {
                if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
                    return;
                }
            }
            string key = _spawnerRandomizer.NextWithReplacement();
            SingleSpawner ss = _singleSpawners[key];

            // If using spawn patterns, retrieve pooled objects and spawn them at the positions in the pattern
            if (ss.UsingPatterns) {
                int i = ss.PatternRandomizer.NextWithReplacement();
                foreach (Transform child in ss.SpawnPatterns[i].Pattern.transform) {
                    GameObject spawnable = ProceduralLevelGenerator.Instance.SpawnPoolerDict[_singleSpawners[key].SpawnablePoolerName].Pooler.GetPooledGameObject();

                    // Safely set position
                    Vector2 destination = child.transform.position + spawnable.GetComponent<SpawnablePoolableObject>().SpawnOffset;
                    spawnable.transform.SafeSetTransformPosition(destination, LayerMask.GetMask("Ground"));

                    if (spawnable.TryGetComponent(out SpawnablePoolableObject spo)) {
                        spo.SetParentSpawnPoint(this);

                        if (spo.AnchorToSpawn) {
                            spawnable.transform.parent = child;
                        }
                    }
                    spawnable.SetActive(true);
                    _spawnables.Add(spawnable.GetComponent<SpawnablePoolableObject>());
                }
            }
            // Else, get a pooled object and spawn at this location
            else {
                GameObject spawnable = ProceduralLevelGenerator.Instance.SpawnPoolerDict[_singleSpawners[key].SpawnablePoolerName].Pooler.GetPooledGameObject();

                Vector2 destination = transform.position + spawnable.GetComponent<SpawnablePoolableObject>().SpawnOffset;
                spawnable.transform.SafeSetTransformPosition(destination, LayerMask.GetMask("Ground"));

                spawnable.SetActive(true);
                _spawnables.Add(spawnable.GetComponent<SpawnablePoolableObject>());
            }
        }

        /// <summary>
        /// Called by a SpawnablePoolableObject when it is disabled to remove it from the SpawnPoint's list of spawnables
        /// </summary>
        /// <param name="spawnable"></param>
        public virtual void RemoveFromList(SpawnablePoolableObject spawnable) {
            _spawnables.Remove(spawnable);
        }

        public virtual void OnMMEvent(PaywallChanceUpdateEvent updateEvent) {
            LocalNoneChance = updateEvent.Chance;
        }

        /// <summary>
        /// On enable, randomly retrieve and spawn new objects in a random spawn pattern (if applicable)
        /// </summary>
        protected virtual void OnEnable() {
            this.MMEventStartListening<PaywallChanceUpdateEvent>();
            Spawn();
        }

        protected virtual void OnDisable() {
            this.MMEventStopListening<PaywallChanceUpdateEvent>();
        }

    }
}
