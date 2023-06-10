using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using TMPro;
using Paywall.Tools;
using UnityEngine.UI;
using System;

namespace Paywall {

    public class GUIManagerIRE_PW : Singleton_PW<GUIManagerIRE_PW>, MMEventListener<MMGameEvent> {
        #region Property Fields

        [field: Header("Bindings")]

		/// the pause screen game object
		[Tooltip("the pause screen game object")]
		[field: SerializeField] public GameObject PauseScreen { get; protected set; }
		/// the game over screen game object
		[Tooltip("the game over screen game object")]
		[field: SerializeField] public GameObject GameOverScreen { get; protected set; }
		/// the object that will contain lives hearts
		[Tooltip("the object that will contain lives hearts")]
		[field: SerializeField] public GameObject HeartsContainer { get; protected set; }
		/// the points counter
		[Tooltip("the points counter")]
		[field: SerializeField] public TextMeshProUGUI PointsText { get; protected set; }
        /// the distance counter
        [Tooltip("the distance counter")]
        [field: SerializeField] public TextMeshProUGUI DistanceText { get; protected set; }
        /// the trinkets counter
        [Tooltip("the trinkets counter")]
		[field: SerializeField] public TextMeshProUGUI TrinketsText { get; protected set; }
		/// the level display
		[Tooltip("the level display")]
		[field: SerializeField] public TextMeshProUGUI LevelText { get; protected set; }
		/// the countdown at the start of a level
		[Tooltip("the countdown at the start of a level")]
		[field: SerializeField] public TextMeshProUGUI CountdownText { get; protected set; }
		/// the screen used for all fades
		[Tooltip("the screen used for all fades")]
		[field: SerializeField] public Image Fader { get; protected set; }

		[field: Header("Resources")]

		/// the gameobject to use to represent lost lives
		[Tooltip("the gameobject to use to represent lost lives")]
		[field: SerializeField] public GameObject GUIHeartEmpty { get; protected set; }
		/// the gameobject to use to represent lost lives
		[Tooltip("the gameobject to use to represent lost lives")]
		[field: SerializeField] public GameObject GUIHeartFull { get; protected set; }

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
        /// the game object that contains the ammo display
        [field: Tooltip("the game object that contains the ammo display")]
        [field: SerializeField] public GameObject EXBarContainer { get; protected set; }
        /// the game object that contains the EX meter display
        [field: Tooltip("the game object that contains the EX meter display")]
        [field: SerializeField] public SegmentedBar EXBar { get; protected set; }
        /// the health bar
        [field: Tooltip("the health bar")]
		[field: SerializeField] public TextMeshProUGUI HealthPoints { get; protected set; }
		/// the health bar
		[field: Tooltip("the health bar")]
		[field: SerializeField] public MMProgressBar[] HealthBars { get; protected set; }

        #endregion

        protected virtual void Start() {
			Initialization();
		}

		protected virtual void Initialization() {
			if ((AmmoBar == null) && (AmmoBarContainer != null)) {
				AmmoBar = AmmoBarContainer.GetComponentInChildren<SegmentedBar>();
			}
			if ((EXBar == null) && (EXBarContainer! != null)) {
				EXBar = EXBarContainer.GetComponentInChildren<SegmentedBar>();
			}
			if (DistanceText != null) {
				DistanceText.text = "0";
			}
			if (TrinketsText != null) {
				TrinketsText.text = "0";
			}
		}

		/// <summary>
		/// Initialization from external source
		/// </summary>
		public virtual void Initialize() {
			RefreshPoints();
			InitializeLives();

			if (CountdownText != null && CountdownText.text == null) {
				CountdownText.enabled = false;
			}
		}

		/// <summary>
		/// Initializes the lives display.
		/// </summary>
		public virtual void InitializeLives() {
			if (HealthPoints != null) {
				HealthPoints.text = GameManagerIRE_PW.Instance.CurrentLives.ToString();
			}

			if (HeartsContainer == null)
				return;

			// we remove everything inside the HeartsContainer
			foreach (Transform child in HeartsContainer.transform) {
				Destroy(child.gameObject);
			}

			int deadLives = (GameManagerIRE_PW.Instance as GameManagerIRE_PW).TotalLives - (GameManagerIRE_PW.Instance as GameManagerIRE_PW).CurrentLives;
			// for each life in the total number of possible lives you can have
			for (int i = 0; i < (GameManagerIRE_PW.Instance as GameManagerIRE_PW).TotalLives; i++) {
				// if the life is already lost, we display an empty heart
				GameObject heart;
				if (deadLives > 0) {
					heart = (GameObject)Instantiate(GUIHeartEmpty);
				}
				else {
					// if the life is still 'alive', we display a full heart
					heart = (GameObject)Instantiate(GUIHeartFull);
				}
				// we instantiate the heart gameobject and position it

				heart.transform.SetParent(HeartsContainer.transform, false);
				heart.GetComponent<RectTransform>().localPosition = new Vector3(HeartsContainer.GetComponent<RectTransform>().sizeDelta.x / 2 - i * (heart.GetComponent<RectTransform>().sizeDelta.x * 75f), 0, 0);
				deadLives--;
			}
		}

		/// <summary>
		/// Override this to have code executed on the GameStart event
		/// </summary>
		public virtual void OnGameStart() {

		}

		/// <summary>
		/// Sets the pause.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetPause(bool state) {
			if (PauseScreen == null) { return; }
			PauseScreen.SetActive(state);
		}

		/// <summary>
		/// Sets the countdown active.
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		public virtual void SetCountdownActive(bool state) {
			if (CountdownText == null) { return; }
			CountdownText.enabled = state;
		}

		/// <summary>
		/// Sets the countdown text.
		/// </summary>
		/// <param name="value">the new countdown text.</param>
		public virtual void SetCountdownText(string newText) {
			if (CountdownText == null) { return; }
			CountdownText.text = newText;
		}

		/// <summary>
		/// Sets the game over screen on or off.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the game over screen on.</param>
		public virtual void SetGameOverScreen(bool state) {
			GameOverScreen.SetActive(state);
			//TextMeshProUGUI gameOverScreenTextObject = GameOverScreen.transform.Find("GameOverScreenText").GetComponent<TextMeshProUGUI>();
			//if (gameOverScreenTextObject != null) {
			//	gameOverScreenTextObject.text = "GAME OVER\nYOUR SCORE : " + Mathf.Round((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Points);
			//}
		}

		/// <summary>
		/// Sets the distance text to the distance traveled in LevelManager
		/// </summary>
		public virtual void RefreshDistance() {
			if (DistanceText == null) {
				return;
			}

			int distance = (int)Math.Round(LevelManagerIRE_PW.Instance.DistanceTraveled, 0);
			DistanceText.text = distance.ToString();
		}

		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public virtual void RefreshPoints() {
			if (PointsText == null)
				return;

			PointsText.text = (GameManagerIRE_PW.Instance as GameManagerIRE_PW).Points.ToString("000 000 000");
		}

		/// <summary>
		/// Sets the level name in the HUD
		/// </summary>
		public virtual void SetLevelName(string name) {
			if (LevelText == null)
				return;

			LevelText.text = name;
		}

		/// <summary>
		/// Fades the fader in or out depending on the state
		/// </summary>
		/// <param name="state">If set to <c>true</c> fades the fader in, otherwise out if <c>false</c>.</param>
		public virtual void FaderOn(bool state, float duration) {
			if (Fader == null) {
				return;
			}
			Fader.gameObject.SetActive(true);
			if (state)
				StartCoroutine(MMFade.FadeImage(Fader, duration, new Color(0, 0, 0, 1f)));
			else
				StartCoroutine(MMFade.FadeImage(Fader, duration, new Color(0, 0, 0, 0f)));
		}

		/// <summary>
		/// Fades the fader to the alpha set as parameter
		/// </summary>
		/// <param name="newColor">The color to fade to.</param>
		/// <param name="duration">Duration.</param>
		public virtual void FaderTo(Color newColor, float duration) {
			if (Fader == null) {
				return;
			}
			Fader.gameObject.SetActive(true);
			StartCoroutine(MMFade.FadeImage(Fader, duration, newColor));
		}

		/*
		 * 
		 */

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

		/// <summary>
		/// Updates the ammo bar
		/// </summary>
		/// <param name="currentAmmo"></param>
		/// <param name="minAmmo"></param>
		/// <param name="maxAmmo"></param>
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

		/// <summary>
		/// Update EX bar
		/// </summary>
		/// <param name="currentEX"></param>
		/// <param name="minEX"></param>
		/// <param name="maxEX"></param>
		public virtual void UpdateEXBar(float currentEX, float minEX, float maxEX) {
			if ((EXBarContainer == null) || (EXBar == null)) { return; }
			if (minEX >= 0) {
				EXBar.SetMinimumValue(minEX);
			}
			if (maxEX >= 0) {
				EXBar.SetMaximumValue(maxEX);
			}
			EXBar.SetCurrentValue(currentEX);
		}

		public virtual void UpdateEXBar(float currentEX) {
			UpdateEXBar(currentEX, -1, -1);
		}

		public virtual void UpdateTrinketsText(int trinkets) {
			TrinketsText.text = trinkets.ToString();
        }

		public virtual void OnMMEvent(MMGameEvent gameEvent) {
			switch (gameEvent.EventName) {
				case "PauseOn":
					SetPause(true);
					break;
				case "PauseOff":
					SetPause(false);
					break;
				case "GameStart":
					OnGameStart();
					break;
			}
		}

		protected virtual void OnEnable() {
			this.MMEventStartListening<MMGameEvent>();
		}

		protected virtual void OnDisable() {
			this.MMEventStopListening<MMGameEvent>();
		}

	}
}
