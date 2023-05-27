using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Attach this component to a character to allow them to pause the game
    /// </summary>
    public class CharacterPauseIRE : CharacterAbilityIRE {
        protected override void HandleInput() {
            base.HandleInput();
            if (_inputManager.InputActions.PlayerControls.Pause.WasPerformedThisFrame()) {
                if (GameManagerIRE_PW.HasInstance) {
                    (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Pause();
                }
            }
        }

    }
}
