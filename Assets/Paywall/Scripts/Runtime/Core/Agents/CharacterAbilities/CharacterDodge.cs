using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Paywall {

    /// <summary>
    /// Dash forward, canceling vertical momentum, and phasing through enemies, bricks, damaging effects
    /// </summary>
    public class CharacterDodge : CharacterAbilityIRE {
        /// Dodge invincibility duration
        [field: Tooltip("Dodge invincibility duration")]
        [field: SerializeField] public float DodgeDuration { get; protected set; } = 0.2f;
        /// Dodge force applied
        [field: Tooltip("Dodge force applied")]
        [field: SerializeField] public Vector2 DodgeForce { get; protected set; }
        /// Dodge cooldown duration. Starts counting once dodge has completed ended.
        [field: Tooltip("Dodge cooldown duration. Starts counting once dodge has completed ended.")]
        [field: SerializeField] public float DodgeCooldown { get; protected set; } = 0.1f;
        /// The multiplier to apply to game's timescale during the dodge
        [field: Tooltip("The multiplier to apply to game's timescale during the dodge")]
        [field: Range(0f, 1f)]
        [field: SerializeField] public float DodgeTimeScaleMultiplier { get; protected set; } = 0.75f;
        /// The multiplier to apply to level's speed during the dodge
        [field: Tooltip("The multiplier to apply to level's speed during the dodge")]
        [field: SerializeField] public float DodgeLevelSpeedMultiplier { get; protected set; } = 2f;
        /// Does this ability cost EX to use
        [field: Tooltip("Does this ability cost EX to use")]
        [field: SerializeField] public bool CostsEX { get; protected set; }

        protected float _currentDodgeTime;
        protected float _timeLastDodgeEnded;
        protected Coroutine _dodgeCoroutine;
        protected Vector2 _initialVelocity;
        protected Color _initialColor;
        protected int _initialLayer;
        protected Vector2 _returnPositionSpeed;

        protected virtual bool EvaluateDodgeConditions() {
            // If we aren't cooldown, return true
            if ((Time.time - _timeLastDodgeEnded) >= DodgeCooldown) {
                return true;
            }
            return false;
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (_character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Dodging) {
                _currentDodgeTime += Time.deltaTime;
            }
        }

        protected virtual void HandleDodgeInput(InputAction.CallbackContext ctx) {
            PerformDodge();
        }

        /// <summary>
        /// Perform dodge
        /// </summary>
        protected virtual void PerformDodge() {
            if (!AbilityAuthorized || !EvaluateDodgeConditions()) {
                return;
            }

            // If ability costs EX, spend the EX or do nothing if there is insufficient EX
            if (CostsEX) {
                if (!_character.SpendEXBars(1)) {
                    return;
                }
            }
            _dodgeCoroutine = StartCoroutine(DodgeCo());
        }

        /// <summary>
        /// Perform dodge coroutine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DodgeCo() {
            // Slow time if applicable, increase level speed, animate dodge, change character state
            GameManagerIRE_PW.Instance.SetTimeScale(GameManagerIRE_PW.Instance.TimeScale * DodgeTimeScaleMultiplier);
            LevelManagerIRE_PW.Instance.TemporarilyMultiplySpeed(DodgeLevelSpeedMultiplier, DodgeDuration);
            _initialColor = _character.Model.color;
            _initialVelocity = _character.CharacterRigidBody.velocity;
            _character.ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Dodging);

            Color newColor = _initialColor;
            newColor.a = 0.2f;
            _character.Model.color = newColor;
            _initialLayer = _character.gameObject.layer;
            _character.gameObject.layer = LayerMask.NameToLayer("Dodging");     // Change collision layer temporarily
            //_character.CharacterRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            _character.SetInvincibilityDuration(DodgeDuration);
            _character.ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Normal);
            _character.ApplyForce(DodgeForce);
            _currentDodgeTime = 0f;

            yield return new WaitForSeconds(DodgeDuration);

            GameManagerIRE_PW.Instance.ResetTimeScale();
            _character.gameObject.layer = _initialLayer;
            _character.Model.color = _initialColor;
            //_character.CharacterRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _currentDodgeTime = 0f;
            _timeLastDodgeEnded = Time.time;
        }

        protected override void OnEnable() {
            base.OnEnable();
            _inputManager.InputActions.PlayerControls.Dodge.performed += HandleDodgeInput;
        }

        protected override void OnDisable() {
            base.OnDisable();
            _inputManager.InputActions.PlayerControls.Dodge.performed -= HandleDodgeInput;
        }
    }
}
