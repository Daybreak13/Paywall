using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    [CreateAssetMenu(fileName = "DepotOption", menuName = "Paywall/Depot/DepotOption")]
    public class ScriptableDepotOption : ScriptableObject {
        /// Name to display on the button for this option
        [field: Tooltip("Name to display on the button for this option")]
        [field: SerializeField] public string Name;
        /// Description to display when this option is selected/highlighted
        [field: Tooltip("Description to display when this option is selected/highlighted")]
        [field: TextArea]
        [field: SerializeField] public string Description;
    }
}
