using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System;

namespace Paywall {

    public class ProjectileWeapon_PW : ProjectileWeapon {
		[field: MMInspectorGroup("Paywall Settings", true)]

		/// How long after a shot before the weapon begins to reload
		[field: Tooltip("How long after a shot before the weapon begins to reload")]
		[field: SerializeField] public float ReloadDelay { get; protected set; } = 2f;
		/// How long does this weapon cause the player to stall in the air when fired
		[field: Tooltip("How long does this weapon cause the player to stall in the air when fired")]
		[field: SerializeField] public float AirStallTime { get; protected set; } = 0.1f;

		public CharacterIRE CharacterOwner { get; protected set; }
		public CharacterHandleWeaponIRE HandleWeaponIRE { get; protected set; }

		protected float _lastReload;
		protected float _lastShot;
		protected float _lastFullAmmo;

        protected override void LateUpdate() {
            base.LateUpdate();
			AutoReloadWeapon();
        }

		public virtual void SetOwner(CharacterIRE character, CharacterHandleWeaponIRE handleWeapon) {
			CharacterOwner = character;
			HandleWeaponIRE = handleWeapon;
        }

        protected override void ShootRequest() {
			// if we have a weapon ammo component, we determine if we have enough ammunition to shoot
			if (MagazineBased) {
				if (WeaponAmmo != null) {
					if (WeaponAmmo.EnoughAmmoToFire()) {
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						_lastShot = Time.time;
					}
					else {
						if (AutoReload && MagazineBased) {
							InitiateReloadWeapon();
						}
						else {
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
				else {
					if (CurrentAmmoLoaded > 0) {
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						CurrentAmmoLoaded -= AmmoConsumedPerShot;
						_lastShot = Time.time;
					}
					else {
						WeaponState.ChangeState(WeaponStates.WeaponIdle);
					}
				}
			}
			else {
				if (WeaponAmmo != null) {
					if (WeaponAmmo.EnoughAmmoToFire()) {
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						_lastShot = Time.time;
					}
					else {
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else {
					WeaponState.ChangeState(WeaponStates.WeaponUse);
					_lastShot = Time.time;
				}
			}
		}

        protected override void WeaponUse() {
            base.WeaponUse();
			HandleWeaponIRE.AirStall(AirStallTime);
        }

		/// <summary>
		/// Autoreload weapon
		/// Should use coroutine for autoreload??
		/// </summary>
		protected virtual void AutoReloadWeapon() {
			if (((Time.time - _lastShot) < ReloadDelay)
				|| (CurrentAmmoLoaded == MagazineSize)
				|| ((Time.time - _lastReload) < ReloadTime)) {
				return;
            }
			CurrentAmmoLoaded++;
			_lastReload = Time.time;
        }


    }
}
