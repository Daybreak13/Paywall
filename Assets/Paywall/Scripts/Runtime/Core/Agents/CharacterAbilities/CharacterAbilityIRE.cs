using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.InputSystem;
using MoreMountains.InfiniteRunnerEngine;

namespace Paywall {

    public class CharacterAbilityIRE : MonoBehaviour {
        /// Is the ability enabled
        [field: Tooltip("Is the ability enabled")]
        [field: SerializeField] public bool AbilityPermitted { get; protected set; } = true;

        protected IREInputActions _inputActions;
        protected bool _initialized;
        protected PaywallPlayableCharacter _character;

        /// <summary>
        /// Is the ability allowed to be used
        /// </summary>
        public bool AbilityAuthorized {
            get {
                if (_character != null) {

                }
                if (GameManager.HasInstance) {
                    if (GameManager.Instance.Status != GameManager.GameStatus.GameInProgress) {
                        return false;
                    }
                }
                return AbilityPermitted;
            }

        }

        protected virtual void Start() {
            if (!_initialized) {
                Initialization();
            }
        }

        protected virtual void Initialization() {
            _inputActions = new();
            TryGetComponent(out _character);
        }

        protected virtual void Update() {
            HandleInput();
            ProcessAbility();
        }

        protected virtual void HandleInput() {

        }

        protected virtual void ProcessAbility() {

        }

        protected virtual void OnEnable() {
            if (!_initialized) {
                Initialization();
            }
            _inputActions.Enable();
        }

        protected virtual void OnDisable() {
            _inputActions.Disable();
        }

    }
}
