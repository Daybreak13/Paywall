using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    public enum SuperTypes { None, Invincible, Reload, Ghost }

    public class CharacterSuper : CharacterAbilityIRE, MMEventListener<MMGameEvent> {

        #region Property Fields

        /// Currently equipped super
        [field: Tooltip("Currently equipped super")]
        [field: SerializeField] public SuperTypes CurrentSuperType { get; protected set; }
        /// Minimum EX required to activate super
        [field: Tooltip("Minimum EX required to activate super")]
        [field: SerializeField] public float MinEXForActivation { get; protected set; } = 50f;

        [field: Header("Drain Rate")]

        /// Initial drain rate
        [field: Tooltip("Initial drain rate")]
        [field: SerializeField] public float InitialDrainRate { get; protected set; }
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
            CurrentDrainRate = InitialDrainRate;
        }

        /// <summary>
        /// Check if we are allowed to activate super this frame
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateSuperConditions() {
            if ((_character as PlayerCharacterIRE).CurrentEX < MinEXForActivation) {
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

            HandleEX();
        }

        /// <summary>
        /// Handles EX drain from the super
        /// End when EX hits 0
        /// </summary>
        protected virtual void HandleEX() {
            // Increase drain rate every frame
            CurrentDrainRate += DrainRateAcceleration * Time.deltaTime;

            (_character as PlayerCharacterIRE).AddEX(-CurrentDrainRate * Time.deltaTime);
            if ((_character as PlayerCharacterIRE).CurrentEX <= 0) {
                EndSuper();
            }
        }

        /// <summary>
        /// Checks if we are allowed to activate super, then select the proper super function to execute
        /// </summary>
        protected virtual void PerformSuper() {
            if (!AbilityAuthorized || !EvaluateSuperConditions()) {
                return;
            }
            (_character as PlayerCharacterIRE).SetEXDraining(true);
            CurrentDrainRate = InitialDrainRate;
        }

        protected virtual void EndSuper() {
            (_character as PlayerCharacterIRE).SetEXDraining(false);
        }

        public void OnMMEvent(MMGameEvent eventType) {
            if (eventType.EventName.Equals("LifeLost")) {
                EndSuper();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            InputSystemManager_PW.InputActions.PlayerControls.Super.performed += InputSuper;
            this.MMEventStartListening<MMGameEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            InputSystemManager_PW.InputActions.PlayerControls.Super.performed -= InputSuper;
            this.MMEventStopListening<MMGameEvent>();
        }
    }
}
