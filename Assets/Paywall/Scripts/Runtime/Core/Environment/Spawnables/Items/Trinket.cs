using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Trinket pickable object
    /// </summary>
    public class Trinket : PickableObject_PW {
        /// The value of this trinket
        [field: Tooltip("The value of this trinket")]
        [field: SerializeField] public int Value { get; protected set; } = 1;

        protected override void ObjectPicked() {
            PaywallCreditsEvent.Trigger(MoneyTypes.Trinket, MoneyMethods.Add, Value);
        }
    }
}
