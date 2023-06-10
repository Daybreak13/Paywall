using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    public enum PowerUpTypes { EX, Health, Ammo }

    /// <summary>
    /// Power up pickable object
    /// </summary>
    public class PowerUp : PickableObject_PW {
        [field: Header("Power Up")]

        /// Power up types
        [field: Tooltip("")]
        [field: SerializeField] public PowerUpTypes PowerUpType { get; protected set; }

        protected override void ObjectPicked() {
            base.ObjectPicked();
            RunnerItemPickEvent.Trigger(PowerUpType);
        }
    }
}
