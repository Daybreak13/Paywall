using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "AmmoRitualItem", menuName = "Paywall/Depot/Ritual/AmmoRitualItem")]
    public class AmmoRitualItem : ScriptableRitualItem {
        /// How much ammo is given
        [field: Tooltip("How much ammo is given")]
        [field: SerializeField] public int AmmoGain { get; protected set; } = 1;

        public override void BuyAction() {
            base.BuyAction();
            SupplyDepotItemManager.Instance.AmmoRitual(AmmoGain);
        }
    }
}
