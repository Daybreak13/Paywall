using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using UnityEngine.SceneManagement;

namespace Paywall {

    /// <summary>
    /// Level selector with added functionality
    /// Tells LevelManager_PW whether or not to convert the GameManager's points to credits
    /// </summary>
    public class LevelSelector_PW : MonoBehaviour {
        /// The name of the level to go to
        [field: Tooltip("The name of the level to go to")]
        public string LevelName;
        /// If true, save the game
        [field: Tooltip("If true, save the game")]
        [field: SerializeField] public bool Save { get; protected set; }
        /// If true, convert points to credits
        [field: Tooltip("If true, convert points to credits")]
        [field:SerializeField] public bool ConvertCredits { get; protected set; }

        public virtual void GoToLevel() {
            if (LevelManagerIRE_PW.HasInstance) {
                LevelManagerIRE_PW.Instance.GotoLevel(LevelName, Save, ConvertCredits);
            }
        }

        public virtual void RestartLevel() {
            if (LevelManagerIRE_PW.HasInstance) {
                LevelManagerIRE_PW.Instance.GotoLevel(SceneManager.GetActiveScene().name, Save, ConvertCredits);
            }
        }

        /// <summary>
	    /// Resumes the game
	    /// </summary>
	    public virtual void Resume() {
            (GameManagerIRE_PW.Instance as GameManagerIRE_PW).UnPause();
        }

        /// <summary>
	    /// Resets the score.
	    /// </summary>
	    public virtual void ResetScore() {
            SingleHighScoreManager.ResetHighScore();
        }

    }
}
