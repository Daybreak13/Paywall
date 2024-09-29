using MoreMountains.Tools;
using Paywall.Interfaces;
using UnityEngine;

namespace Paywall
{
    public class LevelSpeedManager : MonoBehaviour, ILevelSpeedManager, MMEventListener<MMGameEvent>
    {
        [field: Header("Speed")]

        /// <inheritdoc />
        [field: Tooltip("The current speed the level is traveling at")]
        [field: SerializeField] public float Speed { get; private set; }
        /// <inheritdoc />
        [field: Tooltip("If the character's simulated x movement is blocked, we slow the level speed to this value")]
        [field: SerializeField] public float BlockedSpeed { get; private set; } = 0f;
        /// <inheritdoc />
        [field: Tooltip("Rate at which we slow to BlockedSpeed when blocked")]
        [field: SerializeField] public float BlockedDeceleration { get; private set; } = 20f;
        /// <inheritdoc />
        [field: Tooltip("Rate at which we accelerate to preblock speed after unblocking")]
        [field: SerializeField] public float BlockedAcceleration { get; private set; } = 20f;
        /// <inheritdoc />
        [field: Tooltip("The distance traveled since the start of the level")]
        [field: SerializeField] public float DistanceTraveled { get; private set; }
        /// <inheritdoc />
        [field: Tooltip("If the character's simulated x movement is blocked, we slow the level speed to this value")]
        /// the initial speed of the level
        [field: SerializeField] public float InitialSpeed { get; private set; } = 30f;
        /// <inheritdoc />
        [field: SerializeField] public float MaximumSpeed { get; private set; } = 50f;
        /// <inheritdoc />
        [field: SerializeField] public float SpeedIncrement { get; private set; } = 7.5f;
        /// <inheritdoc />
        [field: MMReadOnly]
        [field: SerializeField] public float CurrentUnmodifiedSpeed { get; private set; }
        /// <inheritdoc />
        [field: Tooltip("the global speed modifier for level segments")]
        [field: SerializeField] public float SegmentSpeed { get; private set; } = 0f;

        private void Update()
        {
        }

        private void FixedUpdate()
        {
        }

        /// <inheritdoc />
        public void SetSpeed(float speed)
        {
            throw new System.NotImplementedException();
        }

        public void OnMMEvent(MMGameEvent eventType)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Override this if needed
        /// </summary>
        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        /// <summary>
        /// Override this if needed
        /// </summary>
        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
