using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System;

namespace Paywall {

    public class ProjectileWeapon_PW : ProjectileWeapon {
		[field: MMInspectorGroup("Paywall Settings", true)]

		[field: SerializeField] public float ReloadDelay { get; protected set; } = 2f;

		protected float _lastReload;
		protected float _lastShot;

        protected override void LateUpdate() {
			Debug.Log(WeaponState);
            base.LateUpdate();
			AutoReloadWeapon();
        }

        protected override void ShootRequest() {
			// if we have a weapon ammo component, we determine if we have enough ammunition to shoot
			if (MagazineBased) {
				if (WeaponAmmo != null) {
					if (WeaponAmmo.EnoughAmmoToFire()) {
						WeaponState.ChangeState(WeaponStates.WeaponUse);
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
					}
					else {
						
					}
				}
			}
			else {
				if (WeaponAmmo != null) {
					if (WeaponAmmo.EnoughAmmoToFire()) {
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else {
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else {
					WeaponState.ChangeState(WeaponStates.WeaponUse);
				}
			}
		}

        protected override void WeaponUse() {
            base.WeaponUse();
			_lastShot = Time.time;
        }

        public override void InitiateReloadWeapon() {
			if (!MagazineBased) {
				return;
			}
		}

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
