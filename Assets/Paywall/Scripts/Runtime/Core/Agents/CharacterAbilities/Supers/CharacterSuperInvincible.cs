using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public class CharacterSuperInvincible : CharacterSuper {
        /// Speed factor to apply to level speed during invincible super
        [field: Tooltip("Speed factor to apply to level speed during invincible super")]
        [field: SerializeField] public float InvincibleSpeedFactor { get; protected set; } = 2f;
        /// EX gain for enemy killed by invincible super
        [field: Tooltip("EX gain for enemy killed by invincible super")]
        [field: SerializeField] public float InvincibleEXOnKill { get; protected set; } = 5f;

        public override void ProcessAbility() {
            base.ProcessAbility();
        }

        protected override void PerformSuper() {
            base.PerformSuper();
            PerformSuperInvincible();
        }

        /// <summary>
        /// Perform invincible super
        /// </summary>
        protected virtual void PerformSuperInvincible() {
            SuperActive = true;
            _character.ToggleInvincibility(true);
            LevelManagerIRE_PW.Instance.TemporarilyAddSpeedSwitch(InvincibleSpeedFactor, true);
            _character.Model.color = Color.blue;
        }

        /// <summary>
        /// End invincible super
        /// </summary>
        protected override void EndSuper() {
            base.EndSuper();
            SuperActive = false;
            _character.ToggleInvincibility(false);
            LevelManagerIRE_PW.Instance.TemporarilyAddSpeedSwitch(0f, false);
            _character.Model.color = _initialColor;
        }

        /// <summary>
        /// Handles collisions for invincible super
        /// If colliding with an enemy, kill it
        /// </summary>
        /// <param name="collision"></param>
        protected virtual void OnCollisionEnter2D(Collision2D collision) {
            if ((CurrentSuperType == SuperTypes.Invincible) && SuperActive
                && (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))) {

                //
                if (collision.gameObject.TryGetComponent(out Health_PW health)) {
                    // If we IKed, add EX if applicable
                    if (health.InstantKill(gameObject)) {
                        (_character as PlayerCharacterIRE).AddEX(InvincibleEXOnKill);
                    }
                }
            }
        }
    }
}
