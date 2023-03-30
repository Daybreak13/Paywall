using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine.SceneManagement;

namespace Paywall {

	public struct PaywallLevelEndEvent {
		public bool convertCredits;
		public PaywallLevelEndEvent(bool convert) {
			convertCredits = convert;
        }
		static PaywallLevelEndEvent e;
		public static void Trigger(bool convert) {
			e.convertCredits = convert;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// Corgi Engine LevelManager, adjusted for Paywall GameOver events
	/// </summary>
	public class LevelManager_PW : LevelManager {

		[field: Header("Game Over")]

		/// Canvas group containing the Game Over screen
		[Tooltip("Canvas group containing the Game Over screen")]
		[field: SerializeField] public GameObject GameOverCanvas { get; protected set; }
		/// If true, load to Game Over scene on a game over
		[Tooltip("If true, load to Game Over scene on a game over")]
		[field: SerializeField] public bool UseGameOverScene { get; protected set; }
		/// If not using Game Over scene, this is the delay to wait to pause the game
		[Tooltip("If not using Game Over scene, this is the delay to wait to pause the game")]
		[MMCondition("UseGameOverScene", true, true)]
		[field:SerializeField] public float PauseDelay { get; protected set; }
		/// If true, trigger CorgiEnginePointsEvent to set points to 0
		[Tooltip("If true, trigger CorgiEnginePointsEvent to set points to 0")]
		[field: SerializeField] public bool ClearPoints { get; protected set; } = true;

		/// <summary>
		/// Grab our GameOverCanvas if it was not set in inspector
		/// Clear points if necessary
		/// </summary>
		public override void Start() {
			base.Start();
			if (GameOverCanvas == null) {
				if (UICameraCanvasManager.HasInstance) {
					GameOverCanvas = UICameraCanvasManager.Instance.GameOverCanvas;
				}
			}
			if (ClearPoints) {
				CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
            }
		}

        /// <summary>
        /// Catch GameOver events
        /// </summary>
        /// <param name="engineEvent"></param>
        public override void OnMMEvent(CorgiEngineEvent engineEvent) {
			base.OnMMEvent(engineEvent);
			if (engineEvent.EventType == CorgiEngineEventTypes.GameOver) {
				GameOver();
			}
		}

		/// <summary>
		/// Load Game Over scene or bring up Game Over display in current scene
		/// </summary>
		protected virtual void GameOver() {
			// Load Game Over scene
			if (UseGameOverScene) {
				if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != "")) {
					MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
				}
			}
			// Bring up Game Over display 
			else {
				if (GameOverCanvas != null) {
					GameOverCanvas.SetActive(true);
				}
				StartCoroutine(GameOverPauseDelay());
			}
		}

		/// <summary>
		/// How long to wait after the GameOver event is caught for the game to pause
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator GameOverPauseDelay() {
			yield return new WaitForSeconds(PauseDelay);
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			if (GameManager.HasInstance) {
				GameManager.Instance.Paused = true;
			}
		}

		/// <summary>
		/// Kills the player
		/// Fires GameOver event if appropriate, which will be handled by the GameOverEventManager
		/// </summary>
		/// <param name="player"></param>
		public override void PlayerDead(Character player) {
			Health characterHealth = player.GetComponent<Health>();
			if (characterHealth == null) {
				return;
			}
			else {
				// if we've setup our game manager to use lives (meaning our max lives is more than zero)
				if (GameManager.Instance.MaximumLives > 0) {
					// we lose a life
					GameManager.Instance.LoseLife();
					// if we're out of lives, we check if we have an exit scene, and move there
					if (GameManager.Instance.CurrentLives <= 0) {
						Cleanup();

						CorgiEngineEvent.Trigger(CorgiEngineEventTypes.GameOver);
						
					}
				}
				
			}
		}

		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName, bool fadeOut = true, bool save = true, bool convert = false) {
			PaywallLevelEndEvent.Trigger(convert);
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelEnd);
			if (save) {
				MMGameEvent.Trigger("Save");
			}

			if (fadeOut) {
				if ((Players != null) && (Players.Count > 0)) {
					MMFadeInEvent.Trigger(OutroFadeDuration, FadeTween, FaderID, true, Players[0].transform.position);
				}
				else {
					MMFadeInEvent.Trigger(OutroFadeDuration, FadeTween, FaderID, true, Vector3.zero);
				}
			}

			StartCoroutine(GotoLevelCo(levelName, fadeOut));
		}

		/// <summary>
		/// If CE updates, update this
		/// </summary>
		/// <param name="levelName"></param>
		/// <param name="fadeOut"></param>
		/// <returns></returns>
		protected override IEnumerator GotoLevelCo(string levelName, bool fadeOut = true) {
			// 
			if (Players != null && Players.Count > 0) {
				foreach (Character player in Players) {
					if (player != null) player.Disable();
				}
			}

			if (fadeOut) {
				if (Time.timeScale > 0.0f) {
					yield return new WaitForSeconds(OutroFadeDuration);
				}
				else {
					yield return new WaitForSecondsRealtime(OutroFadeDuration);
				}
			}

			// we trigger an unPause event for the GameManager (and potentially other classes)
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LoadNextScene);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;
			switch (LoadingSceneMode) {
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}


	}
}
