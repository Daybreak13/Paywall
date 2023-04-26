using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using Weighted_Randomizer;

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

    public class LevelSegmentController : MonoBehaviour {
        /// The segment's name
        [field: Tooltip("The segment's name")]
        [field: SerializeField] public string SegmentName { get; protected set; }
        /// The weighted object poolers
        [field: Tooltip("The weighted object poolers")]
        [field: SerializeField] public List<WeightedObjectPooler> WeightedPoolers { get; protected set; }
        /// The segment's type
        [field: Tooltip("The segment's type")]
        [field: SerializeField] public SegmentTypes SegmentType { get; protected set; }

        [field: Header("Boundaries")]

        /// The left bound of the platform
        [field: Tooltip("The left bound of the platform")]
        [field: SerializeField] public Vector3 LeftBound { get; protected set; }
        /// The right bound of the platform
        [field: Tooltip("The right bound of the platform")]
        [field: SerializeField] public Vector3 RightBound { get; protected set; }
        /// The point at which the previous segment connects to this one
        [field: Tooltip("The point at which the previous segment connects to this one")]
        [field: SerializeField] public Vector3 LeftIn { get; protected set; }
        /// The point at which the next segment connects to this one
        [field: Tooltip("The point at which the next segment connects to this one")]
        [field: SerializeField] public Vector3 RightOut { get; protected set; }

        [field: Header("Walls")]

        /// The left wall (for ground type segments)
        [field: Tooltip("The left wall (for ground type segments)")]
        [field: SerializeField] public GameObject LeftWall { get; protected set; }
        /// The right wall (for ground type segments)
        [field: Tooltip("The right wall (for ground type segments)")]
        [field: SerializeField] public GameObject RightWall { get; protected set; }

        [field: Header("Other Settings")]

        /// Lock the height of the previous/next/both segments to be the same height as this one
        [field: Tooltip("Lock the height of the previous/next/both segments to be the same height as this one")]
        [field: SerializeField] public HeightLockSettings HeightLockSetting { get; protected set; } = HeightLockSettings.None;

        public virtual void SetLeftBound(Vector3 vector) {
            LeftBound = vector;
        }

        public virtual void SetRightBound(Vector3 vector) {
            RightBound = vector;
        }

        public virtual void SetLeftIn(Vector3 vector) {
            LeftIn = vector;
        }

        public virtual void SetRightOut(Vector3 vector) {
            RightOut = vector;
        }

        protected virtual void OnEnable() {
            if (LeftWall != null) {
                LeftWall.SetActive(true);
            }
            if (RightWall != null) {
                RightWall.SetActive(true);
            }
        }

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(LeftBound), 0.2f);
            Gizmos.DrawWireSphere(transform.TransformPoint(RightBound), 0.2f);
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.TransformPoint(LeftIn), 0.2f);
            Gizmos.DrawWireSphere(transform.TransformPoint(RightOut), 0.2f);
        }
    }

}
