using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;
using System.Linq;
using Paywall.Tools;

namespace Paywall {
    /// <summary>
    /// Various types of level segments
    /// Ground: Solid ground
    /// Transition: Attached directly to the end of the previous/next segment
    /// Jumper: Has holes to fall into
    /// </summary>
    public enum SegmentTypes { Ground, Transition, Jumper }

    public enum SpawnTypes { Enemy, Platform, Brick }

    public enum HeightLockSettings { None, Previous, Next, Both }

    /// <summary>
    /// Represents a spawn point for spawnables in this level segment
    /// </summary>
    [System.Serializable]
    public class SpawnPointClass {
        public string PoolerName;
        public SpawnableWeightedObjectPooler WeightedPooler;
        public Transform Location;
    }

    /// <summary>
    /// Stores spawn points, bounds, link points, and other level segment data
    /// </summary>
    public class LevelSegmentController : MonoBehaviour {

        /// The segment's name
        //[field: Tooltip("The segment's name")]
        public string SegmentName { get { return gameObject.name; } protected set { } }

        [field: Header("Segment Info")]

        /// The segment's type
        [field: Tooltip("The segment's type")]
        [field: SerializeField] public SegmentTypes SegmentType { get; protected set; }

        [field: Header("Spawners")]

        /// The weighted object poolers
        [field: Tooltip("The weighted object poolers")]
        [field: SerializeField] public List<SpawnPoint> SpawnPoints { get; protected set; }

        [field: Header("Boundaries")]

        /// The point at which the previous segment connects to this one
        [field: Tooltip("The point at which the previous segment connects to this one")]
        [field: SerializeField] public Transform LeftIn { get; protected set; }
        /// The point at which the next segment connects to this one
        [field: Tooltip("The point at which the next segment connects to this one")]
        [field: SerializeField] public Transform RightOut { get; protected set; }

        [field: Header("Walls")]

        /// The left wall (for ground type segments)
        [field: Tooltip("The left wall (for ground type segments)")]
        [field: SerializeField] public GameObject LeftWall { get; protected set; }
        /// The right wall (for ground type segments)
        [field: Tooltip("The right wall (for ground type segments)")]
        [field: SerializeField] public GameObject RightWall { get; protected set; }

        [field: Header("Height Lock")]

        /// Lock the height of the previous/next/both segments to be the same height as this one
        [field: Tooltip("Lock the height of the previous/next/both segments to be the same height as this one")]
        [field: SerializeField] public HeightLockSettings HeightLockSetting { get; protected set; } = HeightLockSettings.None;
        /// Lock the height of the previous/next/both segments to be the same height as this one
        [field: Tooltip("Lock the height of the previous/next/both segments to be the same height as this one")]
        [field: SerializeField] public int PreviousHeightDelta { get; protected set; }

        protected virtual void OnEnable() {
            LeftWall.SetActiveIfNotNull(true);
            RightWall.SetActiveIfNotNull(true);
        }

        /// <summary>
        /// Used by editor
        /// </summary>
        /// <param name="spawnPoints"></param>
        public virtual void SetSpawnPoints(List<SpawnPoint> spawnPoints) {
            SpawnPoints = spawnPoints;
        }

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            if ((LeftIn != null) && (RightOut != null)) {
                Gizmos.DrawWireSphere(LeftIn.position, 0.2f);
                Gizmos.DrawWireSphere(RightOut.position, 0.2f);
            }
        }
    }

}
