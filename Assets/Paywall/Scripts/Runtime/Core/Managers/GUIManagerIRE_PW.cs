using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using TMPro;
using Paywall.Tools;

namespace Paywall {

    public class GUIManagerIRE_PW : GUIManager {
		[field: Header("HUD")]

		/// the game object that contains the heads up display (avatar, health, points...)
		[field: Tooltip("the game object that contains the heads up display (avatar, health, points...)")]
		[field: SerializeField] public GameObject HUD { get; protected set; }
		/// the game object that contains the ammo display
		[field: Tooltip("the game object that contains the ammo display")]
		[field: SerializeField] public GameObject AmmoBarContainer { get; protected set; }
		/// the game object that contains the ammo display
		[field: Tooltip("the game object that contains the ammo display")]
		[field: SerializeField] public SegmentedBar AmmoBar { get; protected set; }
		/// the jetpack bar
		[field: Tooltip("the jetpack bar")]
		[field: SerializeField] public MMProgressBar[] HealthBars { get; protected set; }

        protected virtual void Start() {
			Initialization();
        }

		protected virtual void Initialization() {
			if ((AmmoBar == null) && (AmmoBarContainer != null)) {
				AmmoBar = AmmoBarContainer.GetComponentInChildren<SegmentedBar>();
			}
        }

        /// <summary>
        /// Sets the HUD active or inactive
        /// </summary>
        /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
        public virtual void SetHUDActive(bool state) {
			if (HUD != null) {
				HUD.SetActive(state);
			}
			if (PointsText != null) {
				PointsText.enabled = state;
			}
			if (LevelText != null) {
				LevelText.enabled = state;
			}
		}

        public override void SetGameOverScreen(bool state) {
			GameOverScreen.SetActive(state);
			//TextMeshProUGUI gameOverScreenTextObject = GameOverScreen.transform.Find("GameOverScreenText").GetComponent<TextMeshProUGUI>();
			//if (gameOverScreenTextObject != null) {
			//	gameOverScreenTextObject.text = "GAME OVER\nYOUR SCORE : " + Mathf.Round(GameManager.Instance.Points);
			//}
		}

        /// <summary>
        /// Updates the health bar.
        /// </summary>
        /// <param name="currentHealth">Current health.</param>
        /// <param name="minHealth">Minimum health.</param>
        /// <param name="maxHealth">Max health.</param>
        /// <param name="playerID">Player I.</param>
        public virtual void UpdateHealthBar(float currentHealth, float minHealth, float maxHealth, string playerID) {
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0) { return; }

			foreach (MMProgressBar healthBar in HealthBars) {
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID) {
					healthBar.UpdateBar(currentHealth, minHealth, maxHealth);
				}
			}
		}

		public virtual void UpdateAmmoBar(int currentAmmo, int minAmmo, int maxAmmo) {
			if ((AmmoBarContainer == null) || (AmmoBar == null)) { return; }
			if (minAmmo >= 0) {
				AmmoBar.SetMinimumValue(minAmmo);
            }
			if (maxAmmo >= 0) {
				AmmoBar.SetMaximumValue(maxAmmo);
            }
			AmmoBar.SetCurrentValue(currentAmmo);
		}

		public virtual void UpdateAmmoBar(int currentAmmo) {
			UpdateAmmoBar(currentAmmo, -1, -1);
        }

	}
}
