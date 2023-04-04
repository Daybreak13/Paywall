using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    /// <summary>
    /// Input system manager for Infinite Runner Engine
    /// </summary>
    public class IFInputSystemManager : InputManager {
        /// the minimum horizontal and vertical value you need to reach to trigger movement on an analog controller (joystick for example)
		[Tooltip("the minimum horizontal and vertical value you need to reach to trigger movement on an analog controller (joystick for example)")]
        public Vector2 Threshold = new Vector2(0.1f, 0.4f);

        public IFInputActions InputActions;

        protected bool _initialized = false;

        protected virtual void Start() {
            if (!_initialized) {
                Initialization();
            }
        }

        protected virtual void Initialization() {
            InputActions = new IFInputActions();
            _initialized = true;
        }

        protected override void HandleKeyboard() {
            if (InputActions.PlayerControls.Pause.WasPressedThisFrame()) {
                PauseButtonDown();
            }
            if (InputActions.PlayerControls.Pause.IsPressed()) {
                PauseButtonPressed();
            }
            if (InputActions.PlayerControls.Pause.WasReleasedThisFrame()) {
                PauseButtonUp();
            }

            if (InputActions.PlayerControls.Jump.WasPressedThisFrame()) {
                MainActionButtonDown();
            }
            if (InputActions.PlayerControls.Jump.IsPressed()) {
                MainActionButtonPressed();
            }
            if (InputActions.PlayerControls.Jump.WasReleasedThisFrame()) {
                MainActionButtonUp();
            }

            if (InputActions.PlayerControls.Up.WasPressedThisFrame()) {
                UpButtonDown();
            }
            if (InputActions.PlayerControls.Up.IsPressed()) {
                UpButtonPressed();
            }
            if (InputActions.PlayerControls.Up.WasReleasedThisFrame()) {
                UpButtonUp();
            }

            if (InputActions.PlayerControls.Down.WasPressedThisFrame()) {
                DownButtonDown();
            }
            if (InputActions.PlayerControls.Down.IsPressed()) {
                DownButtonPressed();
            }
            if (InputActions.PlayerControls.Down.WasReleasedThisFrame()) {
                DownButtonUp();
            }

            if (InputActions.PlayerControls.Left.WasPressedThisFrame()) {
                LeftButtonDown();
            }
            if (InputActions.PlayerControls.Left.IsPressed()) {
                LeftButtonPressed();
            }
            if (InputActions.PlayerControls.Left.WasReleasedThisFrame()) {
                LeftButtonUp();
            }

            if (InputActions.PlayerControls.Right.WasPressedThisFrame()) {
                RightButtonDown();
            }
            if (InputActions.PlayerControls.Right.IsPressed()) {
                RightButtonPressed();
            }
            if (InputActions.PlayerControls.Right.WasReleasedThisFrame()) {
                RightButtonUp();
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
