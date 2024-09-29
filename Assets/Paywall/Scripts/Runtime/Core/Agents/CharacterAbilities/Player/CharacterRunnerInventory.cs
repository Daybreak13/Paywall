using MoreMountains.Tools;
using UnityEngine;

namespace Paywall
{

    /// <summary>
    /// Manages runner inventory for resources picked that only apply during a run (eg powerups)
    /// </summary>
    public class CharacterRunnerInventory : CharacterAbilityIRE, MMEventListener<RunnerItemPickEvent>
    {
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
        [field: SerializeField] public CharacterHandleWeaponStandalone HandleWeaponComponent { get; protected set; }

        protected override void Initialization()
        {
            base.Initialization();
            if (HandleWeaponComponent == null)
            {
                HandleWeaponComponent = GetComponent<CharacterHandleWeaponStandalone>();
            }
            GUIManagerIRE_PW.Instance.SetHealthFragments(HealthFragments, MaxHealthFragments);
            GUIManagerIRE_PW.Instance.SetAmmoFragments(AmmoFragments, MaxAmmoFragments);
        }

        /// <summary>
        /// Handle item pick events
        /// </summary>
        /// <param name="itemPickEvent"></param>
        public virtual void OnMMEvent(RunnerItemPickEvent itemPickEvent)
        {
            switch (itemPickEvent.PickedPowerUpType)
            {
                case PowerUpTypes.Health:
                    HealthFragments += itemPickEvent.Amount;
                    // If we have a full health unit, increase health
                    if (HealthFragments >= MaxHealthFragments)
                    {
                        HealthFragments -= MaxHealthFragments;
                        GameManagerIRE_PW.Instance.SetLives(GameManagerIRE_PW.Instance.CurrentLives + 1);
                    }
                    GUIManagerIRE_PW.Instance.SetHealthFragments(HealthFragments, MaxHealthFragments);
                    break;
                case PowerUpTypes.Ammo:
                    AmmoFragments += itemPickEvent.Amount;
                    // If we have a full ammo unit, increase mag size
                    if (AmmoFragments >= MaxAmmoFragments)
                    {
                        AmmoFragments -= MaxAmmoFragments;
                        HandleWeaponComponent.SetCurrentWeaponMagSize(HandleWeaponComponent.CurrentWeapon.MagazineSize + 1);
                    }
                    GUIManagerIRE_PW.Instance.SetAmmoFragments(AmmoFragments, MaxAmmoFragments);
                    break;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<RunnerItemPickEvent>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<RunnerItemPickEvent>();
        }
    }
}
