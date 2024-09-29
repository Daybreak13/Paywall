using MoreMountains.Tools;
using UnityEngine;
using Zenject;

namespace Paywall
{
    /// <summary>
    /// Manages player character spawn: position and prefab
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        /// <summary>
        /// The player character will spawn at this transform position
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public GameObject StartingPosition { get; private set; }

        /// <inheritdoc />
        public GameObject CurrentPlayerObject { get; }
        /// <summary>
        /// 
        /// </summary>
        public PlayableCharacter CurrentPlayerCharacter { get; private set; }

        private PlayableCharacter _currentPlayerCharacter;
        private PlayableCharacter.Factory _characterFactory;

        [Inject]
        public void Construct(PlayableCharacter.Factory characterFactory)
        {
            _characterFactory = characterFactory;
        }

        /// <summary>
        /// Instantiates all the playable characters and feeds them to the gameManager
        /// </summary>
        private void InstantiateCharacters()
        {
            if (_characterFactory == null)
            {
                return;
            }




            //if (PlayableCharacter == null)
            //{
            //    return;
            //}

            //// we instantiate the corresponding prefab
            //PlayableCharacter instance = Instantiate(PlayableCharacter);
            //// we position it based on the StartingPosition point
            //instance.transform.position = new Vector3(StartingPosition.transform.position.x, StartingPosition.transform.position.y, StartingPosition.transform.position.z);
            //// we set manually its initial position
            //instance.SetInitialPosition(instance.transform.position);
            //// we feed it to the game manager
            //CurrentPlayerCharacter = instance;

            MMEventManager.TriggerEvent(new MMGameEvent("PlayableCharactersInstantiated"));
        }

    }
}
