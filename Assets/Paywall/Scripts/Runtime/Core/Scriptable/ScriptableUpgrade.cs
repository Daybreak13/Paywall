using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Paywall {

    public enum UpgradeTypes { Player, Game }

    /// <summary>
    /// Scriptable object representing an upgrade
    /// To be extended by other classes
    /// </summary>
    [Serializable]
    public class ScriptableUpgrade : ScriptableObject {
        [field: Header("Basic Info")]

        /// The upgrade's ID
        [field: Tooltip("The upgrade's ID")]
        [field: SerializeField] public string UpgradeID { get; protected set; }
        /// The upgrade's name
        [field: Tooltip("The upgrade's name")]
        [field:SerializeField] public string UpgradeName { get; protected set; }
        /// The upgrade's description
        [field: Tooltip("The upgrade's description")]
        [field:TextArea]
        [field: SerializeField] public string UpgradeDescription { get; protected set; }
        /// The upgrade's icon
        [field: Tooltip("The upgrade's icon")]
        [field: SerializeField] public Image UpgradeIcon { get; protected set; }
        /// Has the upgrade been unlocked
        [field: Tooltip("Has the upgrade been unlocked")]
        [field: SerializeField] public bool Unlocked { get; protected set; }
        /// How much the upgrade costs to unlock
        [field: Tooltip("How much the upgrade costs to unlock")]
        [field: SerializeField] public int Cost { get; protected set; }
        /// What currency does the upgrade take
        [field: Tooltip("What currency does the upgrade take")]
        [field: SerializeField] public MoneyTypes MoneyType { get; protected set; }
        /// What type of upgrade is it
        [field: Tooltip("What type of upgrade is it")]
        [field: SerializeField] public UpgradeTypes UpgradeType { get; protected set; }

        /// <summary>
        /// Unlocks the upgrade
        /// </summary>
        public virtual void UnlockUpgrade() {
            Unlocked = true;
        }

        /// <summary>
        /// Locks the upgrade
        /// </summary>
        public virtual void LockUpgrade() {
            Unlocked = false;
        }

        public virtual Upgrade ConvertToClass() {
            Upgrade upgrade = new Upgrade(this);
            return upgrade;
        }

        /// <summary>
        /// What action to perform on upgrading. To be overridden.
        /// </summary>
        public virtual void UpgradeAction() {

        }

    }
}
