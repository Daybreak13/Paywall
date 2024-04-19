using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Module that rescues player when they go OOB
    /// </summary>
    public class ModuleRescue : CharacterAbilityIRE, MMEventListener<PaywallModuleEvent> {

        /// <summary>
        /// When the player goes out of bounds, reset their position to the starting position
        /// </summary>
        protected virtual void Rescue() {
            Character.transform.position = (Character as PlayerCharacterIRE).InitialPosition;
        }

        public override void PermitAbility(bool permit) {
            base.PermitAbility(permit);
        }

        public virtual void OnMMEvent(PaywallModuleEvent moduleEvent) {

        }

        protected override void OnEnable() {
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
        }
    }
}
