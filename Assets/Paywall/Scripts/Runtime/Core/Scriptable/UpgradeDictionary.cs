using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [System.Serializable]
    public class UpgradeSubGroup {
        public string Label;
        public List<ScriptableUpgrade> Upgrades = new();
    }

    [System.Serializable]
    public class UpgradeGroup {
        public string Label;
        public List<UpgradeSubGroup> SubGroups = new();
    }

    [CreateAssetMenu(fileName = "UpgradeDictionary", menuName = "Paywall/Upgrades/Utilities/UpgradeDictionary")]
    public class UpgradeDictionary : ScriptableObject {
        /// The upgrade dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public List<UpgradeSubGroup> GameStoreUpgrades { get; protected set; } = new();
        /// The upgrade dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public List<UpgradeSubGroup> RunnerStoreUpgrades { get; protected set; } = new();
        /// The upgrade dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public List<UpgradeSubGroup> OtherUpgrades { get; protected set; } = new();
        /// The upgrade dictionary
        [field: Tooltip("The email dictionary")]
        [field: SerializeField] public List<UpgradeGroup> Upgrades { get; protected set; } = new();

        public virtual Dictionary<string, ScriptableUpgrade> GetDictionary() {
            Dictionary<string, ScriptableUpgrade> upgrades = new();
            foreach(UpgradeSubGroup group in GameStoreUpgrades) {
                foreach (ScriptableUpgrade upgrade in group.Upgrades) {
                    upgrades.Add(upgrade.UpgradeID, upgrade);
                }
            }
            foreach (UpgradeSubGroup group in RunnerStoreUpgrades) {
                foreach (ScriptableUpgrade upgrade in group.Upgrades) {
                    upgrades.Add(upgrade.UpgradeID, upgrade);
                }
            }
            foreach (UpgradeSubGroup group in OtherUpgrades) {
                foreach (ScriptableUpgrade upgrade in group.Upgrades) {
                    upgrades.Add(upgrade.UpgradeID, upgrade);
                }
            }

            foreach (UpgradeGroup group in Upgrades) {
                foreach (UpgradeSubGroup subGroup in group.SubGroups) {
                    foreach (ScriptableUpgrade upgrade in subGroup.Upgrades) {
                        upgrades.Add(upgrade.UpgradeID, upgrade);
                    }
                }
            }

            return upgrades;
        }

    }
}
