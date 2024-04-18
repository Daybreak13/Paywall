using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "AmmoItem", menuName = "Paywall/Depot/Items/AmmoItem")]
    public class DepotAmmoItem : BaseScriptableDepotItem {
        /// How much ammo is given
        [field: Tooltip("How much ammo is given")]
        [field: SerializeField] public int AmmoGain { get; protected set; } = 1;

        public override void BuyAction() {
            base.BuyAction();
            SupplyDepotItemManager.Instance.AmmoItem(AmmoGain);
        }
    }
}
