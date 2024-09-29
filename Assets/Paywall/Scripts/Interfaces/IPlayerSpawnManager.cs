using UnityEngine;

namespace Paywall.Interfaces
{
    /// <summary>
    /// Interface for player spawn manager
    /// Fields for player character prefab, starting position
    /// </summary>
    public interface IPlayerSpawnManager
    {
        /// <summary>
        /// Player character starting position
        /// </summary> 
        public GameObject StartingPosition { get; }
        /// <summary>
        /// Current player character game object
        /// </summary> 
        public GameObject CurrentPlayerObject { get; }

        void InstantiateCharacters();

    }
}
