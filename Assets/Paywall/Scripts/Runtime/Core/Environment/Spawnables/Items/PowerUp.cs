using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Types of power ups/fragments
    /// </summary>
    public enum PowerUpTypes { EX, Health, Ammo }

    /// <summary>
    /// Power up pickable object
    /// </summary>
    public class PowerUp : PickableObject_PW
    {
        [field: Header("Power Up")]

        /// Power up types
        [field: Tooltip("Power up types")]
        [field: SerializeField] public PowerUpTypes PowerUpType { get; protected set; }
        /// Amount of the powerup to pick up
        [field: Tooltip("Amount of the powerup to pick up")]
        [field: SerializeField] public int Amount { get; protected set; } = 1;

        protected override void ObjectPicked()
        {
            base.ObjectPicked();
            RunnerItemPickEvent.Trigger(PowerUpType, Amount);
        }
    }
}
