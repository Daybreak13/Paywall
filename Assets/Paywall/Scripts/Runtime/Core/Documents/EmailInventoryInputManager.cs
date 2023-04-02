using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine.InputSystem;
using Paywall.Documents;

namespace Paywall {

    public class EmailInventoryInputManager : MonoBehaviour {

        [SerializeField] private EmailButtonDisplay emailButtonDisplay;

        public CorgiEngineInputActions InputActions;

        protected virtual void Awake() {
            InputActions = new CorgiEngineInputActions();
        }

        protected virtual void Update() {
            HandleInput();
        }

        protected virtual void HandleInput() {
            if (InputActions.UI.Cancel.triggered) {
                Back();
            }
        }

        protected virtual void Back() {
            if (emailButtonDisplay.EmailButtonsContainer.activeSelf) {
                emailButtonDisplay.EmailButtonsContainer.SetActive(false);
                emailButtonDisplay.ToggleButtonsContainer.SetActive(true);
            }
        }

        protected virtual void OnEnable() {
            InputActions.Enable();
        }

        protected virtual void OnDisable() {
            InputActions.Disable();

        }
    }
}
