namespace Paywall.Interfaces
{
    /// <summary>
    /// Interface for LevelSpeedManager
    /// </summary>
    public interface ILevelSpeedManager
    {
        /// <summary>
        /// The current speed the level is traveling at
        /// </summary>
        public float Speed { get; }
        /// <summary>
        /// If the character's simulated x movement is blocked, we slow the level speed to this value
        /// </summary>
        public float BlockedSpeed { get; }
        /// <summary>
        /// Rate at which we slow to BlockedSpeed when blocked
        /// </summary>
        public float BlockedDeceleration { get; }
        /// <summary>
        /// Rate at which we accelerate to preblock speed after unblocking
        /// </summary>
        public float BlockedAcceleration { get; }
        /// <summary>
        /// The distance traveled since the start of the level
        /// </summary>
        public float DistanceTraveled { get; }
        /// <summary>
        /// If the character's simulated x movement is blocked, we slow the level speed to this value
        /// </summary>
        public float InitialSpeed { get; }
        /// <summary>
        /// The maximum speed the level will run at
        /// </summary>
        public float MaximumSpeed { get; }
        /// <summary>
        /// Increment at which speed increases
        /// </summary>
        public float SpeedIncrement { get; }
        /// <summary>
        /// Current speed not counting temp speed modifiers
        /// </summary>
        public float CurrentUnmodifiedSpeed { get; }
        /// <summary>
        /// the global speed modifier for level segments
        /// </summary>
        public float SegmentSpeed { get; }

        /// <summary>
        /// Set the Speed value
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(float speed);
    }
}