using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Module that rescues player when they go OOB
    /// </summary>
    public class ModuleRescue : CharacterAbilityIRE {

        /// <summary>
        /// When the player goes out of bounds, reset their position to the starting position
        /// </summary>
        protected virtual void Rescue() {
            _character.transform.position = (_character as PlayerCharacterIRE).InitialPosition;
        }

        public override void PermitAbility(bool permit) {
            base.PermitAbility(permit);
        }
    }
}
