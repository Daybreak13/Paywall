using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    /// <summary>
    /// Health component adjusted for IRE
    /// </summary>
    public class Health_PW : Health {
        protected PlayableCharacter _playableCharacter;

        protected override void Initialization() {
            base.Initialization();
            if (_playableCharacter == null) {
                TryGetComponent(out _playableCharacter);
            }
        }

        public override void Kill() {
            base.Kill();
            if (MoreMountains.InfiniteRunnerEngine.LevelManager.HasInstance && (_playableCharacter != null)) {
                MoreMountains.InfiniteRunnerEngine.LevelManager.Instance.KillCharacter(_playableCharacter);
            }
        }
    }
}
