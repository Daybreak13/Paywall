using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paywall {

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
