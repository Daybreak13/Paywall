using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// List of immutable depot items
    /// Read the data into lists/dictionaries to be modified at runtime and serialized
    /// </summary>
    [CreateAssetMenu(fileName = "DepotItemList", menuName = "Paywall/Depot/DepotItemList")]
    public class DepotItemList : ScriptableList<BaseScriptableDepotItem>
    {
        /// ID of this list
        [field: Tooltip("ID of this list")]
        [field: SerializeField] public string ID { get; protected set; }
        /// Is this list able to appear in the shop
        [field: Tooltip("Is this list able to appear in the shop")]
        [field: SerializeField] public bool Active { get; protected set; } = true;
    }
}
