using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    /// <summary>
    /// Primary input system manager for runner gameplay
    /// </summary>
    public class InputSystemManager_PW : MonoBehaviour {
        public IREInputActions InputActions;

        protected bool _initialized;

        protected virtual void Start() {
            if (!_initialized) {
                Initialization();
            }
        }

        protected virtual void Initialization() {
            InputActions = new IREInputActions();
            _initialized = true;
        }

        protected virtual void Update() {
            HandleKeyboard();
        }

        protected virtual void HandleKeyboard() {
            if (InputActions.PlayerControls.Jump.WasPressedThisFrame()) {
                MainActionButtonDown();
            }
        }

        public virtual void MainActionButtonDown() {
            if ((LevelManagerIRE_PW.Instance.ControlScheme == LevelManagerIRE_PW.Controls.SingleButton)
                || (LevelManagerIRE_PW.Instance.ControlScheme == LevelManagerIRE_PW.Controls.Swipe)) {
                if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.GameOver) {
                    return;
                }
                if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.LifeLost) {
                    LevelManagerIRE_PW.Instance.LifeLostAction();
                    return;
                }
            }
        }

        /// <summary>
        /// On enable we enable our input actions
        /// </summary>
        protected virtual void OnEnable() {
            if (!_initialized) {
                Initialization();
            }
            InputActions.Enable();
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable() {
            InputActions.Disable();
        }
    }
}
