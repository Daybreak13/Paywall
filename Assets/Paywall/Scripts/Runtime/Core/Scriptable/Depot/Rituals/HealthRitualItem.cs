using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "HealthRitualItem", menuName = "Paywall/Depot/Ritual/HealthRitualItem")]
    public class HealthRitualItem : ScriptableRitualItem {
        /// How much health is given
        [field: Tooltip("How much health is given")]
        [field: SerializeField] public int HealthGain { get; protected set; } = 1;
        /// Level speed increase from ritual
        [field: Tooltip("Level speed increase from ritual")]
        [field: SerializeField] public float SpeedIncrease { get; protected set; } = 10f;

        public override void BuyAction() {
            base.BuyAction();
            SupplyDepotItemManager.Instance.HealthRitual(HealthGain, SpeedIncrease);
        }
    }
}
