using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class LevelManagerIRE_PW : LevelManager {

        /// <summary>
        /// Save game and convert points to credits if applicable
        /// </summary>
        /// <param name="levelName"></param>
        /// <param name="save"></param>
        /// <param name="convert"></param>
        public virtual void GotoLevel(string levelName, bool save, bool convert) {
            PaywallLevelEndEvent.Trigger(convert);
            if (save) {
                MMGameEvent.Trigger("Save");
            }
            GotoLevel(levelName);
        }

    }
}
