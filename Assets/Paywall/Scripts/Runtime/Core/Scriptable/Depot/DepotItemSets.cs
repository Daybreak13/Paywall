using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// List of all depot item sets to choose for the shop selection
    /// </summary>
    [CreateAssetMenu(fileName = "DepotItemSets", menuName = "Paywall/Depot/DepotItemSets")]
    public class DepotItemSets : ScriptableList<DepotItemList>
    {

    }
}
