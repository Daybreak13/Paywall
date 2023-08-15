using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Paywall {

    public class SupplyDepotDisplay : MonoBehaviour {
        [field: Header("Buttons")]

        /// 
        [field: Tooltip("")]
        [field: SerializeField] public Button EXButton { get; protected set; }
        /// 
        [field: Tooltip("")]
        [field: SerializeField] public Button HealthButton { get; protected set; }
        /// 
        [field: Tooltip("")]
        [field: SerializeField] public Button AmmoButton { get; protected set; }


    }
}
