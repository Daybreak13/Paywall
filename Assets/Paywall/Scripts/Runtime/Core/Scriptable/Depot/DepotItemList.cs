using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// List of immutable depot items
    /// Read the data into lists/dictionaries to be modified at runtime and serialized
    /// </summary>
    [CreateAssetMenu(fileName = "DepotItemList", menuName = "Paywall/Depot/DepotItemList")]
    public class DepotItemList : ScriptableList<BaseScriptableDepotItem> {
        
    }
}
