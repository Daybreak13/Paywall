using Paywall.Tools;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Add this to an enemy that is stompable, and may damage the player
    /// If the enemy is stompable, use this in place of KillOnTouch_PW
    /// </summary>
    public class Stompable_PW : LaunchPad
    {
        [field: Header("Stompable")]

        /// If true, this object kills the player on touch
        [field: Tooltip("If true, this object kills the player on touch")]
        [field: SerializeField] public bool KillsPlayer { get; protected set; }
        /// If true, this object kills the player on touch even if stomped on
        [field: Tooltip("If true, this object kills the player on touch even if stomped on")]
        [field: FieldCondition("KillsPlayer", true)]
        [field: SerializeField] public bool KillsPlayerRegardless { get; protected set; }
        /// Health component to use for death feedbacks
        [field: Tooltip("Health component to use for death feedbacks")]
        [field: SerializeField] public Health_PW HealthComponent { get; protected set; }

        protected bool _killedPlayer;
        protected bool _dead;

        protected override void Awake()
        {
            base.Awake();
            if (HealthComponent == null)
            {
                //HealthComponent = GetComponent<Health_PW>();
            }
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);
            _killedPlayer = false;
            if (collision.collider.CompareTag(_playerTag) && (_character != null))
            {
                if (KillsPlayer)
                {
                    // The player made contact with something that hits no matter what, or they made contact without stomping
                    if (KillsPlayerRegardless || !_collidingAbove)
                    {
                        KillPlayer(collision.gameObject);
                    }
                }
                // If the player stomped on this, kill it
                if (_collidingAbove)
                {
                    DestroySelf();
                    _dead = true;
                    if (!_killedPlayer)
                    {
                        PaywallKillEvent.Trigger(true, gameObject);
                    }
                }
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (_dead)
            {
                return;
            }
            _killedPlayer = false;
            if (collision.collider.CompareTag(_playerTag) && (_character != null))
            {
                if (KillsPlayer)
                {
                    KillPlayer(collision.gameObject);
                }
            }
        }

        /// <summary>
        /// Kills this object
        /// </summary>
        protected virtual void DestroySelf()
        {
            if (HealthComponent == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                HealthComponent.Kill();
            }
        }

        /// <summary>
        /// Triggered when we collide with either a 2D or 3D collider
        /// </summary>
        /// <param name="collidingObject">Colliding object.</param>
        protected virtual void KillPlayer(GameObject collidingObject)
        {
            // we verify that the colliding object is a PlayableCharacter with the Player tag. If not, we do nothing.

            if (_character == null)
            {
                return;
            }

            if (_character.Invincible)
            {
                return;
            }

            if (_character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Parrying)
            {

                return;
            }
            _killedPlayer = true;

            // we ask the LevelManager to kill the character
            LevelManagerIRE_PW.Instance.KillCharacter(_character);
        }

        protected virtual void OnEnable()
        {
            _dead = false;
        }
    }
}
