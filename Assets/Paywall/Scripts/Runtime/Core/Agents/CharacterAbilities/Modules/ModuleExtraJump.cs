using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Module that gives an extra jump
    /// </summary>
    public class ModuleExtraJump : CharacterAbilityIRE {

        /// The character jump ability component
        [field: Tooltip("The character jump ability component")]
        [field: SerializeField] public CharacterJumpIRE CharacterJump { get; protected set; }
        protected int _originalJumpCount;

        protected override void Initialization() {
            base.Initialization();
            _originalJumpCount = CharacterJump.NumberOfJumpsAllowed;
        }

        public virtual void InitializeModule() {

        }

        public override void PermitAbility(bool permit) {
            base.PermitAbility(permit);
            if (CharacterJump == null) {
                return;
            }
            if (permit) {
                _originalJumpCount = CharacterJump.NumberOfJumpsAllowed;
                CharacterJump.SetNumberJumpsAllowed(CharacterJump.NumberOfJumpsAllowed + 1);
            }
            else {
                CharacterJump.SetNumberJumpsAllowed(_originalJumpCount);
            }
        }
    }
}
