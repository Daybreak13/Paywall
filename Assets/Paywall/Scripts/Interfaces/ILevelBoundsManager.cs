using UnityEngine;

namespace Paywall.Interfaces
{

    /// <summary>
    /// Level bounds manager
    /// </summary>
    public interface ILevelBoundsManager
    {
        /// <summary>
        /// Outside of these bounds, a game object will be recycled
        /// </summary>
        public Bounds RecycleBounds { get; }
        /// <summary>
        /// Outside of these bounds, a the player will be killed
        /// </summary>
        public Bounds DeathBounds { get; }
        /// <summary>
        /// Spawnables and rotators won't move until they've passed this point
        /// </summary>
        public Transform MoveBarrier { get; }
    }
}
