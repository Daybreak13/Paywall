using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paywall {

    /// <summary>
    /// Serializable class used by PaywallProgressManager to save upgrade progress
    /// </summary>
    [Serializable]
    public class Upgrade {
        [field: Header("Basic Info")]

        /// The upgrade's name
        [Tooltip("The upgrade's name")]
        [field: SerializeField] public string UpgradeName { get; protected set; }
        /// The upgrade's description
        [Tooltip("The upgrade's description")]
        [field: TextArea]
        [field: SerializeField] public string UpgradeDescription { get; protected set; }
        /// Has the upgrade been unlocked
        [Tooltip("Has the upgrade been unlocked")]
        [field: SerializeField] public bool Unlocked { get; protected set; }
        /// How much the upgrade costs to unlock
        [Tooltip("How much the upgrade costs to unlock")]
        [field: SerializeField] public int Cost { get; protected set; }
        /// What currency does the upgrade take
        [Tooltip("What currency does the upgrade take")]
        [field: SerializeField] public MoneyTypes MoneyType { get; protected set; }
        /// What type of upgrade is it
        [Tooltip("What type of upgrade is it")]
        [field: SerializeField] public UpgradeTypes UpgradeType { get; protected set; }

        public Upgrade() {

        }

        public Upgrade(ScriptableUpgrade scriptableUpgrade) {
            ConvertToClass(scriptableUpgrade);
        }

        public virtual void UnlockUpgrade() {
            Unlocked = true;
        }

        public virtual void LockUpgrade() {
            Unlocked = false;
        }

        public virtual Upgrade ConvertToClass(ScriptableUpgrade s) {
            UpgradeName = s.UpgradeName;
            UpgradeDescription = s.UpgradeDescription;
            Unlocked = s.Unlocked;
            Cost = s.Cost;
            MoneyType = s.MoneyType;
            UpgradeType = s.UpgradeType;
            return this;
        }

        /// <summary>
        /// What action to perform on upgrading. To be overridden.
        /// </summary>
        public virtual void UpgradeAction(object obj = null, UpgradeMethods upgradeMethod = UpgradeMethods.Unlock) {
            if (upgradeMethod == UpgradeMethods.Unlock) {
                UnlockUpgrade();
            }
            else {
                LockUpgrade();
            }
        }

    }
}
