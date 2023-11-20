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
        /// If true, use global weights from ProceduralLevelGenerator for each spawnable
        [field: Tooltip("If true, use global weights from ProceduralLevelGenerator for each spawnable")]
        [field: SerializeField] public bool UseGlobalSpawnWeight { get; protected set; }

        [field: Header("Boundaries")]

        /// The point at which the previous segment connects to this one
        [field: Tooltip("The point at which the previous segment connects to this one")]
        [field: SerializeField] public Transform LeftIn { get; protected set; }
        /// The point at which the next segment connects to this one
        [field: Tooltip("The point at which the next segment connects to this one")]
        [field: SerializeField] public Transform RightOut { get; protected set; }
        /// The box collider for this segment, used to detect if the segment is out of bounds for OutOfBoundsRecycle
        [field: Tooltip("The point at which the next segment connects to this one")]
        [field: SerializeField] public BoxCollider2D BoundsBox { get; protected set; }
        public Vector2 LeftBound { get { return new Vector2(BoundsBox.bounds.min.x, BoundsBox.bounds.center.y); } }
        public Vector2 RightBound { get { return new Vector2(BoundsBox.bounds.max.x, BoundsBox.bounds.center.y); } }

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
        /// Set a specific height for this level segment
        [field: Tooltip("Set a specific height for this level segment")]
        [field: SerializeField] public bool SetHeight { get; protected set; }
        /// List of valid heights for this segment
        [field: Tooltip("List of valid heights for this segment")]
        [field: FieldCondition("SetHeight", true)]
        [field: SerializeField] public int MaxHeight { get; protected set; } = 0;
        /// Height delta between previous segment and this one
        [field: Tooltip("Height delta between previous segment and this one")]
        [field: MMReadOnly]
        [field: SerializeField] public int PreviousHeightDelta { get; protected set; }

        protected virtual void Awake() {
            if (BoundsBox == null) {
                BoundsBox = GetComponent<BoxCollider2D>();
            }
        }

        /// <summary>
        /// Sets the bounds for this segment
        /// </summary>
        /// <param name="leftIn"></param>
        /// <param name="rightOut"></param>
        public virtual void SetBounds(Vector2 leftIn, Vector2 rightOut) {
            LeftIn.localPosition = leftIn;
            RightOut.localPosition = rightOut;
            float length = rightOut.x - leftIn.x;
            float center = leftIn.x + length / 2;
            BoundsBox.offset = new Vector2(center, 0);
            BoundsBox.size = new Vector2(length, 1);
        }

        /// <summary>
        /// Used by editor
        /// </summary>
        /// <param name="spawnPoints"></param>
        public virtual void SetSpawnPoints(List<SpawnPoint> spawnPoints) {
            SpawnPoints = spawnPoints;
        }

        protected virtual void OnEnable() {
            LeftWall.SetActiveIfNotNull(true);
            RightWall.SetActiveIfNotNull(true);
        }

        /// <summary>
        /// Draw leftin rightout bounds gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            if ((LeftIn != null) && (RightOut != null)) {
                Gizmos.DrawWireSphere(LeftIn.position, 0.2f);
                Gizmos.DrawWireSphere(RightOut.position, 0.2f);
            }
        }
    }

}
