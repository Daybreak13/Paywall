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

        public bool AbilityInitialized { get { return _initialized; } }

        protected InputSystemManager_PW _inputManager;
        protected bool _initialized;
        protected CharacterIRE _character;
        protected Animator _animator;
        protected Rigidbody2D _rigidbody2D;
        protected BoxCollider2D _boxCollider;
        protected float _initialGravityScale;

        /// <summary>
        /// Is the ability allowed to be used
        /// </summary>
        public bool AbilityAuthorized {
            get {
                if (_character != null) {
                    if (_character.ConditionState.CurrentState != CharacterStates_PW.ConditionStates.Normal) {
                        return false;
                    }
                }
                if (GameManagerIRE_PW.HasInstance) {
                    if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
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
            if (_character == null) {
                _character = GetComponentInParent<CharacterIRE>();
            }
            if (_character != null) {
                _inputManager = _character.LinkedInputManager;
                _animator = _character.CharacterAnimator;
                _rigidbody2D = _character.CharacterRigidBody;
                _boxCollider = _character.CharacterBoxCollider;
                _initialGravityScale = _rigidbody2D.gravityScale;
                _initialized = true;
            }
        }

        public virtual void SetCharacter(CharacterIRE character) {
            _character = character;
        }

        protected virtual void Update() {
            //EarlyProcessAbility();
            //ProcessAbility();
        }

        protected virtual void InternalHandleInput() {
            if (_inputManager == null) {
                return;
            }
            HandleInput();
        }

        /// <summary>
        /// Handles user input. To be overridden.
        /// </summary>
        protected virtual void HandleInput() {

        }

        /// <summary>
        /// Early update. To be overridden
        /// </summary>
        public virtual void EarlyProcessAbility() {
            InternalHandleInput();
        }

        /// <summary>
        /// Update. To be overridden.
        /// </summary>
        public virtual void ProcessAbility() {

        }

        /// <summary>
        /// Late update. To be overridden
        /// </summary>
        public virtual void LateProcessAbility() {

        }

        /// <summary>
        /// Resets abilities. Usually called on player out of bounds death.
        /// </summary>
        public virtual void ResetAbility() {

        }

        protected virtual void OnEnable() {
            if (!_initialized) {
                Initialization();
            }

        }

        protected virtual void OnDisable() {

        }

    }
}
