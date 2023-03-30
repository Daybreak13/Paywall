using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Sets jump ability permission
    /// </summary>
    [CreateAssetMenu(fileName = "BaseJumpUpgrade", menuName = "Paywall/Upgrades/RunnerUpgrades/BaseJumpUpgrade")]
    public class BaseJumpUpgrade : RunnerUpgrade {

        public override void UpgradeCharacterAction(Character character, UpgradeMethods upgradeMethod) {
            if (character.TryGetComponent<CharacterJump>(out CharacterJump characterJump)) {
                if (upgradeMethod == UpgradeMethods.Unlock) {
                    characterJump.PermitAbility(true);
                } else {
                    characterJump.PermitAbility(false);
                }
            }
        }

    }

}
