using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Add this class to a trigger boxCollider and it'll kill all playable characters that collide with it.
    /// If the enemy is stompable, use Stompable_PW instead
    /// </summary>
    public class KillOnTouch_PW : MonoBehaviour
    {
        /// Target layers
        [field: Tooltip("Target layers")]
        [field: SerializeField] public LayerMask TargetLayers { get; protected set; } = PaywallLayerManager.PlayerLayerMask;

        protected const string _playerHurtboxTag = "PlayerHurtbox";
        protected const string _playerTag = "Player";

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            KillCharacter(collision.gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            KillCharacter(collision.gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            KillCharacter(collider.gameObject);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            KillCharacter(collider.gameObject);
        }

        protected virtual void KillCharacter(GameObject collidingObject)
        {
            // If colliding object is not on target layer, do nothing
            if (((1 << collidingObject.layer) & TargetLayers) == 0)
            {
                return;
            }

            // If colliding object is not player, do nothing
            if (!collidingObject.TryGetComponent(out PlayableCharacter character))
            {
                return;
            }

            // If player is invincible, do nothing
            if (character.Invincible)
            {
                return;
            }

            // we ask the LevelManager to kill the character
            LevelManagerIRE_PW.Instance.KillCharacter(character);
        }

    }
}
