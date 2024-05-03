using UnityEngine;

namespace Paywall {

    public class Portal : MonoBehaviour {
        /// The entry portal
        [field: Tooltip("The entry portal")]
        [field: SerializeField] public GameObject Entry { get; protected set; }
        /// The exit portal
        [field: Tooltip("The exit portal")]
        [field: SerializeField] public GameObject Exit { get; protected set; }


    }
}
