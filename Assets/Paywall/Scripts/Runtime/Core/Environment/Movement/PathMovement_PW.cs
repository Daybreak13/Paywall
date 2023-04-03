using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Paywall {

    public class PathMovement_PW : MMPathMovement {
        /// If true, set gameobject to inactive once reaching goal
        [Tooltip("If true, set gameobject to inactive once reaching goal")]
        public bool SleepOnGoal = true;
        public bool ResetTransformPosition;

        protected bool _init;

        protected override void Initialization() {
            base.Initialization();
            _init = true;
        }

        protected override void EndReached() {
            if (SleepOnGoal && (CycleOption == CycleOptions.OnlyOnce || CycleOption == CycleOptions.StopAtBounds)) {
                ResetPath();
                gameObject.SetActive(false);
            }
        }

        public override void ResetPath() {
            Initialization();
            CanMove = false;
            if (ResetTransformPosition) {
                transform.position = _originalTransformPosition;
            }
        }

        protected virtual void OnEnable() {
            if (_init) {
                CanMove = true;
            }
        }
    }
}
