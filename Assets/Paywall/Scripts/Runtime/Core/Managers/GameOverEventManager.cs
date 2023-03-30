using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace Paywall {

	/// <summary>
	/// Catches GameOver events, usually sent by LevelManager
	/// Loads the Game Over scene, or brings up the Game Over display in the current scene, depending on settings
	/// </summary>
    public class GameOverEventManager : MMSingleton<GameOverEventManager>, MMEventListener<CorgiEngineEvent> {
		/// Canvas group containing the Game Over screen
		[Tooltip("Canvas group containing the Game Over screen")]
		public GameObject GameOverCanvas;
		/// If true, load to Game Over scene on a game over
		[Tooltip("If true, load to Game Over scene on a game over")]
		public bool UseGameOverScene;
		/// If not using Game Over scene, this is the delay to wait to pause the game
		[Tooltip("If not using Game Over scene, this is the delay to wait to pause the game")]
		[MMCondition("UseGameOverScene", true, true)]
		public float PauseDelay;

		protected virtual void Start() {
			if (GameOverCanvas == null) {
				if (UICameraCanvasManager.HasInstance) {
					GameOverCanvas = UICameraCanvasManager.Instance.GameOverCanvas;
                }
            }
        }

		/// <summary>
		/// Catch GameOver events
		/// </summary>
		/// <param name="engineEvent"></param>
		public virtual void OnMMEvent(CorgiEngineEvent engineEvent) {
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

		protected virtual IEnumerator GameOverPauseDelay() {
			yield return new WaitForSeconds(PauseDelay);
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			if (GameManager.HasInstance) {
				GameManager.Instance.Paused = true;
            }
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable() {
			this.MMEventStartListening<CorgiEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable() {
			this.MMEventStopListening<CorgiEngineEvent>();
		}
	}

}
