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

        public virtual void UpgradeCharacterAction(Character character, UpgradeMethods upgradeMethod = UpgradeMethods.Unlock) {

        }

    }
}
