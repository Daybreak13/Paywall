using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

    /// <summary>
    /// Manages runner inventory for resources picked that only apply during a run (eg powerups)
    /// </summary>
    public class CharacterRunnerInventory : CharacterAbilityIRE, MMEventListener<RunnerItemPickEvent> {
        /// Armor fragments (HP)
        [field: Tooltip("Health fragments (increase HP)")]
        [field: SerializeField] public int HealthFragments { get; protected set; }
        /// How many fragments does it take to make a whole health unit
        [field: Tooltip("How many fragments does it take to make a whole health unit")]
        [field: SerializeField] public int MaxHealthFragments { get; protected set; } = 2;
        /// Magazine fragments (ammo)
        [field: Tooltip("Ammo fragments (increase ammo capacity)")]
        [field: SerializeField] public int AmmoFragments { get; protected set; }
        /// How many fragments does it take to make a whole ammo unit
        [field: Tooltip("How many fragments does it take to make a whole ammo unit")]
        [field: SerializeField] public int MaxAmmoFragments { get; protected set; } = 2;

        [field: Header("Components")]

        /// CharacterHandleWeaponIRE component
        [field: Tooltip("CharacterHandleWeaponIRE component")]
        [field: SerializeField] public CharacterHandleWeaponIRE HandleWeaponComponent { get; protected set; }

        protected override void Initialization() {
            base.Initialization();
            if (HandleWeaponComponent == null) {
                HandleWeaponComponent = GetComponent<CharacterHandleWeaponIRE>();
            }
        }

        /// <summary>
        /// Handle item pick events
        /// </summary>
        /// <param name="itemPickEvent"></param>
        public virtual void OnMMEvent(RunnerItemPickEvent itemPickEvent) {
            switch (itemPickEvent.PickedPowerUpType) {
                case PowerUpTypes.Health:
                    HealthFragments++;
                    // If we have a full health unit, increase health
                    if (HealthFragments == MaxHealthFragments) {
                        HealthFragments = 0;
                        GameManagerIRE_PW.Instance.SetLives(GameManagerIRE_PW.Instance.CurrentLives + 1);
                    }
                    break;
                case PowerUpTypes.Ammo:
                    AmmoFragments++;
                    // If we have a full ammo unit, increase mag size
                    if (AmmoFragments == MaxAmmoFragments) {
                        AmmoFragments = 0;
                        HandleWeaponComponent.SetCurrentWeaponMagSize(HandleWeaponComponent.CurrentWeapon.MagazineSize);
                    }
                    break;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.MMEventStartListening<RunnerItemPickEvent>();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.MMEventStopListening<RunnerItemPickEvent>();
        }
    }
}
