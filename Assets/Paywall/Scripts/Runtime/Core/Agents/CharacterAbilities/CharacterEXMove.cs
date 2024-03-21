using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Paywall.Tools;

namespace Paywall {

    /// <summary>
    /// Override this to create an EX move
    /// </summary>
    public class CharacterEXMove : CharacterAbilityIRE {
        /// Does this ability cost EX to use
        [field: Tooltip("Does this ability cost EX to use")]
        [field: SerializeField] public int EXCost { get; protected set; } = 1;
        /// Block use of this ability while super is active
        [field: Tooltip("Block use of this ability while super is active")]
        [field: SerializeField] public bool BlockDuringSuper { get; protected set; }

        protected virtual void HandleInputCallback(InputAction.CallbackContext ctx) {
            PerformAbility();
        }

        protected virtual void PerformAbility() {

        }

        protected override void OnEnable() {
            base.OnEnable();
            InputSystemManager_PW.InputActions.PlayerControls.Dodge.performed += HandleInputCallback;
        }

        protected override void OnDisable() {
            base.OnDisable();
            InputSystemManager_PW.InputActions.PlayerControls.Dodge.performed -= HandleInputCallback;
        }
    }
}
