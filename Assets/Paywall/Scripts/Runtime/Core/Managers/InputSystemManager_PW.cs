using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using UnityEngine.InputSystem;

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
        /// Handles pauses when the game is not in progress (that is handled by CharacterPauseIRE)
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void PauseButtonDown(InputAction.CallbackContext ctx) {
            if (GameManagerIRE_PW.HasInstance && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress)) {
                GameManagerIRE_PW.Instance.Pause();
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
            InputActions.PlayerControls.Pause.performed += PauseButtonDown;
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable() {
            InputActions.PlayerControls.Pause.performed -= PauseButtonDown;
            InputActions.Disable();
        }
    }
}
