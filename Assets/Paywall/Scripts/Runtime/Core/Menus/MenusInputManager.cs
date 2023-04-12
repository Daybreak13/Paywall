using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Paywall {

    public class MenusInputManager : MonoBehaviour {
        public IREInputActions InputActions;

        protected bool _initialized = false;

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
            HandleInput();
        }

        protected virtual void HandleInput() {

        }

        protected virtual void HandleBackButton(InputAction.CallbackContext context) {
            PWInputEvent.Trigger(PWInputEventTypes.Back);
        }

        /// <summary>
        /// On enable we enable our input actions
        /// </summary>
        protected virtual void OnEnable() {
            if (!_initialized) {
                Initialization();
            }
            InputActions.Enable();
            InputActions.UI.Cancel.performed += HandleBackButton;
        }

        /// <summary>
        /// On disable we disable our input actions
        /// </summary>
        protected virtual void OnDisable() {
            InputActions.Disable();
            InputActions.UI.Cancel.performed -= HandleBackButton;
        }
    }
}
