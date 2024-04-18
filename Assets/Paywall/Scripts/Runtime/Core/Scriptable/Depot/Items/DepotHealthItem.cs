using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "HealthItem", menuName = "Paywall/Depot/Items/HealthItem")]
    public class DepotHealthItem : BaseScriptableDepotItem {
        /// How much health is given
        [field: Tooltip("How much health is given")]
        [field: SerializeField] public int HealthGain { get; protected set; } = 1;

        public override void BuyAction() {
            base.BuyAction();
            SupplyDepotItemManager.Instance.HealthItem(HealthGain);
        }
    }
}
