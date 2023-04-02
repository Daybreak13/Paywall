using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using UnityEngine.SceneManagement;

namespace Paywall {

    /// <summary>
    /// Level selector with added functionality
    /// Tells LevelManager_PW whether or not to convert the GameManager's points to credits
    /// </summary>
    public class LevelSelector_PW : LevelSelector {
        /// If true, convert points to credits
        [field: Tooltip("If true, convert points to credits")]
        [field:SerializeField] public bool ConvertCredits { get; protected set; }

        public override void GoToLevel() {
            if (LevelManager_PW.HasInstance) {
                (LevelManager_PW.Instance as LevelManager_PW).GotoLevel(LevelName, Fade, Save, ConvertCredits);
            }
            else {
                base.GoToLevel();
            }
        }

        public override void RestartLevel() {
            if (LevelManager_PW.HasInstance) {
                (LevelManager_PW.Instance as LevelManager_PW).GotoLevel(SceneManager.GetActiveScene().name, Fade, Save, ConvertCredits);
            }
            else {
                base.RestartLevel();
            }
        }
    }
}
