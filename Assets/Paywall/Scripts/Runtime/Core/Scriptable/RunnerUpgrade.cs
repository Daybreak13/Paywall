using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

namespace Paywall {

    /// <summary>
    /// Upgrade for the Runner game
    /// </summary>
    //[CreateAssetMenu(fileName = "RunnerUpgrade", menuName = "Paywall/Upgrades/RunnerUpgrades/RunnerUpgrade", order = 2)]
    public class RunnerUpgrade : ScriptableUpgrade {

        public override void UpgradeAction(object obj = null, UpgradeMethods upgradeMethod = UpgradeMethods.Unlock) {
            base.UpgradeAction();
            if ((obj != null) && (obj.GetType() == typeof(Character))) {
                UpgradeCharacterAction((Character)obj, upgradeMethod);
            }
        }

        public virtual void UpgradeCharacterAction(Character character, UpgradeMethods upgradeMethod = UpgradeMethods.Unlock) {

        }

    }
}
