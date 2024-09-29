using UnityEngine;

namespace Paywall
{

    [CreateAssetMenu(fileName = "EXItem", menuName = "Paywall/Depot/Items/EXItem")]
    public class DepotEXItem : BaseScriptableDepotItem
    {
        /// How much EX is given
        [field: Tooltip("How much EX is given")]
        [field: SerializeField] public int EXGain { get; protected set; } = 30;

        public override void BuyAction()
        {
            base.BuyAction();
            SupplyDepotItemManager.Instance.EXItem(EXGain);
        }
    }
}
