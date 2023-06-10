using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using Paywall.Tools;
using MoreMountains.Tools;

namespace Paywall {

    /// <summary>
    /// Health component adjusted for IRE
    /// </summary>
    public class Health_PW : Health {
        [field: MMInspectorGroup("Paywall", true, 20)]

        /// Give trinkets on kill?
        [field: Tooltip("Give trinkets on kill?")]
        [field: SerializeField] public bool GivesTrinketsOnKill { get; protected set; }
        /// Trinkets to give on kill
        [field: Tooltip("Trinkets to give on kill")]
        [field: FieldCondition("GivesTrinketsOnKill", true)]
        [field: SerializeField] public int Trinkets { get; protected set; } = 1;

		/// Give streak on kill?
		[field: Tooltip("Give streak on kill?")]
		[field: SerializeField] public bool GivesStreakOnKill { get; protected set; }
		/// Multiplier to apply to EX charge gain when this object is damaged
		[field: Tooltip("Multiplier to apply to EX charge gain when this object is damaged")]
		[field: SerializeField] public float EXChargeOnDamageMultiplier { get; protected set; }

		/// Override the post damage invincibility duration given by DamageOnTouch
		[field: Tooltip("Override the post damage invincibility duration given by DamageOnTouch")]
        [field: SerializeField] public bool OverridePostDamageInvincibilityDuration { get; protected set; }
        /// How long to be invincible after taking damage
        [field: Tooltip("How long to be invincible after taking damage")]
        [field: FieldCondition("OverridePostDamageInvincibilityDuration", true)]
        [field: SerializeField] public float PostDamageInvincibilityDuration { get; protected set; }

		/// <summary>
		/// Apply damage to this Health component
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="instigator"></param>
		/// <param name="flickerDuration"></param>
		/// <param name="invincibilityDuration"></param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
        public override void Damage(float damage, GameObject instigator, float flickerDuration, 
            float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null) {

			if (!gameObject.activeInHierarchy) {
				return;
			}

			// if the object is invulnerable, we do nothing and exit
			if (TemporarilyInvulnerable || Invulnerable || ImmuneToDamage || PostDamageInvulnerable) {
				OnHitZero?.Invoke();
				return;
			}

			if (!CanTakeDamageThisFrame()) {
				return;
			}

			damage = ComputeDamageOutput(damage, typedDamages, true);

			if (damage <= 0) {
				OnHitZero?.Invoke();
				return;
			}

			// we decrease the character's health by the damage
			float previousHealth = CurrentHealth;
			if (MasterHealth != null) {
				previousHealth = MasterHealth.CurrentHealth;
				MasterHealth.Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);

				if (!OnlyDamageMaster) {
					previousHealth = CurrentHealth;
					SetHealth(CurrentHealth - damage, instigator);
				}
			}
			else {
				SetHealth(CurrentHealth - damage, instigator);
			}

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			OnHit?.Invoke();

			if (CurrentHealth < 0) {
				CurrentHealth = 0;
			}

			// we prevent the character from colliding with Projectiles, Player and Enemies
			if (OverridePostDamageInvincibilityDuration) {
				EnablePostDamageInvulnerability();
				StartCoroutine(DisablePostDamageInvulnerability(PostDamageInvincibilityDuration));
            } 
			else if ((invincibilityDuration > 0) && gameObject.activeInHierarchy) {
				EnablePostDamageInvulnerability();
				StartCoroutine(DisablePostDamageInvulnerability(invincibilityDuration));
			}

			// we trigger a damage taken event
			MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

			// Trigger EXCharge event if applicable
			if ((EXChargeOnDamageMultiplier > 0) && instigator.CompareTag(PaywallTagManager.PlayerDamageTag) && instigator.TryGetComponent(out DamageOnTouch_PW damageOnTouch)) {
				if (damageOnTouch.ChargeAmountGained > 0) {
					PaywallEXChargeEvent.Trigger(damageOnTouch.ChargeAmountGained * (previousHealth - CurrentHealth) * EXChargeOnDamageMultiplier, ChangeAmountMethods.Add);
                }
            }

			if (_animator != null) {
				_animator.SetTrigger("Damage");
			}

			// we play the damage feedback
			if (FeedbackIsProportionalToDamage) {
				DamageFeedbacks?.PlayFeedbacks(this.transform.position, damage);
			}
			else {
				DamageFeedbacks?.PlayFeedbacks(this.transform.position);
			}

			if (FlickerSpriteOnHit) {
				// We make the character's sprite flicker
				if (_renderer != null) {
					StartCoroutine(MMImage.Flicker(_renderer, _initialColor, FlickerColor, 0.05f, flickerDuration));
				}
			}

			// we update the health bar
			UpdateHealthBar(true);


			// we process any condition state change
			ComputeCharacterConditionStateChanges(typedDamages);
			ComputeCharacterMovementMultipliers(typedDamages);

			// if health has reached zero we set its health to zero (useful for the healthbar)
			if (MasterHealth != null) {
				if (MasterHealth.CurrentHealth <= 0) {
					MasterHealth.CurrentHealth = 0;
					Kill();
				}
				if (!OnlyDamageMaster) {
					if (CurrentHealth <= 0) {
						CurrentHealth = 0;
						Kill();
					}
				}
			}
			else {
				if (CurrentHealth <= 0) {
					CurrentHealth = 0;
					// On kill, give trinkets if applicable, and trigger death event
					if (instigator.CompareTag(PaywallTagManager.PlayerDamageTag)) {
						if (GivesTrinketsOnKill) {
							PaywallCreditsEvent.Trigger(MoneyTypes.Trinket, MoneyMethods.Add, Trinkets);
							PaywallDeathEvent.Trigger(gameObject);
						}
                        PaywallDeathEvent.Trigger(gameObject, GivesStreakOnKill);
                    }
                    Kill();
				}
			}

        }

    }
}
