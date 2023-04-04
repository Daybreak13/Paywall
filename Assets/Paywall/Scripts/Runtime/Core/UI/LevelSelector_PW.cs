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
    public class LevelSelector_PW : LevelSelector {
        /// If true, save the game
        [field: Tooltip("If true, save the game")]
        [field: SerializeField] public bool Save { get; protected set; }
        /// If true, convert points to credits
        [field: Tooltip("If true, convert points to credits")]
        [field:SerializeField] public bool ConvertCredits { get; protected set; }

        public override void GoToLevel() {
            if (LevelManagerIRE_PW.HasInstance) {
                (LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).GotoLevel(LevelName, Save, ConvertCredits);
            }
            else {
                base.GoToLevel();
            }
        }

        public override void RestartLevel() {
            if (LevelManagerIRE_PW.HasInstance) {
                (LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).GotoLevel(SceneManager.GetActiveScene().name, Save, ConvertCredits);
            }
            else {
                base.RestartLevel();
            }
        }
    }
}
