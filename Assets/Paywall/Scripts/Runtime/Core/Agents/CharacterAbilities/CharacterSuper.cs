using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    public enum SuperTypes { None, Invincible, Reload, Ghost }

    public class CharacterSuper : CharacterAbilityIRE {

        #region Property Fields

        /// Currently equipped super
        [field: Tooltip("Currently equipped super")]
        [field: SerializeField] public SuperTypes CurrentSuperType { get; protected set; }
        /// Minimum EX required to activate super
        [field: Tooltip("Minimum EX required to activate super")]
        [field: SerializeField] public float MinEXForActivation { get; protected set; } = 50f;

        [field: Header("Invincible Super")]

        /// Speed factor to apply to level speed during invincible super
        [field: Tooltip("Speed factor to apply to level speed during invincible super")]
        [field: SerializeField] public float InvincibleSpeedFactor { get; protected set; } = 2f;
        /// EX meter drain rate for invincible super. EX per second.
        [field: Tooltip("EX meter drain rate for invincible super. EX per second.")]
        [field: SerializeField] public float InvincibleDrainRate { get; protected set; } = 5f;
        /// EX gain for enemy killed by invincible super
        [field: Tooltip("EX gain for enemy killed by invincible super")]
        [field: SerializeField] public float InvincibleEXOnKill { get; protected set; } = 5f;

        [field: Header("Reload Super")]

        /// EX meter drain rate for reload super. EX per second.
        [field: Tooltip("EX meter drain rate for reload super. EX per second.")]
        [field: SerializeField] public float ReloadDrainRate { get; protected set; } = 5f;

        [field: Header("Ghost Super")]

        /// EX meter drain rate for ghost super. EX per second.
        [field: Tooltip("EX meter drain rate for ghost super. EX per second.")]
        [field: SerializeField] public float GhostDrainRate { get; protected set; } = 5f;

        [field: Header("Drain Rate")]

        /// Current drain rate
        [field: Tooltip("Current drain rate")]
        [field: MMReadOnly]
        [field: SerializeField] public float CurrentDrainRate { get; protected set; }
        /// The longer the super is active for, the faster the EX drains
        [field: Tooltip("The longer the super is active for, the faster the EX drains")]
        [field: SerializeField] public float DrainRateAcceleration { get; protected set; } = 0.5f;

        [field: Header("Status")]

        /// Is super currently active
        [field: Tooltip("Is super currently active")]
        [field: MMReadOnly]
        [field: SerializeField] public bool SuperActive { get; protected set; }

        #endregion

        protected Color _initialColor;

        protected override void Initialization() {
            base.Initialization();
            _initialColor = _character.Model.color;
        }

        /// <summary>
        /// Check if we are allowed to activate super this frame
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateSuperConditions() {
            if (_character.CurrentEX < MinEXForActivation) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handles input callback when the super button is pressed
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void InputSuper(InputAction.CallbackContext ctx) {
            PerformSuper();
        }

        /// <summary>
        /// Handle active supers every frame
        /// Do nothing if super is inactive or game is not in progress
        /// </summary>
        public override void ProcessAbility() {
            if (!SuperActive || (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress)) {
                return;
            }

            // Increase drain rate every frame
            CurrentDrainRate += DrainRateAcceleration * Time.deltaTime;

            switch (CurrentSuperType) {
                case SuperTypes.None:
                    return;
                case SuperTypes.Invincible:
                    HandleInvincible();
                    break;
                case SuperTypes.Reload:
                    break;
                case SuperTypes.Ghost:
                    break;
            }
        }

        /// <summary>
        /// Checks if we are allowed to activate super, then select the proper super function to execute
        /// </summary>
        protected virtual void PerformSuper() {
            if (!AbilityAuthorized || !EvaluateSuperConditions()) {
                return;
            }

            switch (CurrentSuperType) {
                case SuperTypes.None:
                    return;
                case SuperTypes.Invincible:
                    PerformSuperInvincible();
                    break;
                case SuperTypes.Reload:
                    PerformSuperReload();
                    break;
                case SuperTypes.Ghost:
                    PerformSuperGhost();
                    break;
            }
        }

        /// <summary>
        /// Handles invincible super every frame it is active
        /// End super when EX hits 0
        /// </summary>
        protected virtual void HandleInvincible() {
            _character.AddEX(-CurrentDrainRate * Time.deltaTime);
            if (_character.CurrentEX == 0) {
                EndSuperInvincible();
            }
        }

        /// <summary>
        /// Perform invincible super
        /// </summary>
        protected virtual void PerformSuperInvincible() {
            SuperActive = true;
            CurrentDrainRate = InvincibleDrainRate;
            _character.ToggleInvincibility(true);
            LevelManagerIRE_PW.Instance.TemporarilyMultiplySpeedSwitch(InvincibleSpeedFactor);
            _character.Model.color = Color.blue;
        }

        /// <summary>
        /// End invincible super
        /// </summary>
        protected virtual void EndSuperInvincible() {
            SuperActive = false;
            _character.ToggleInvincibility(false);
            LevelManagerIRE_PW.Instance.TempSpeedSwitchOff();
            _character.Model.color = _initialColor;
        }

        protected virtual void PerformSuperReload() {

        }

        protected virtual void PerformSuperGhost() {

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
                        _character.AddEX(InvincibleEXOnKill);
                    }
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            _character.LinkedInputManager.InputActions.PlayerControls.Super.performed += InputSuper;
        }

        protected override void OnDisable() {
            base.OnDisable();
            _character.LinkedInputManager.InputActions.PlayerControls.Super.performed -= InputSuper;
        }
    }
}
