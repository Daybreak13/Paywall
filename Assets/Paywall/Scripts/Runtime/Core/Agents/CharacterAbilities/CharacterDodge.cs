using Paywall.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Paywall {

    /// <summary>
    /// Dash forward, canceling vertical momentum, and phasing through enemies, bricks, damaging effects
    /// </summary>
    public class CharacterDodge : CharacterEXMove {
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
        /// The boost to apply to level's speed during the dodge
        [field: Tooltip("The boost to apply to level's speed during the dodge")]
        [field: SerializeField] public float DodgeLevelSpeedBoost { get; protected set; } = 100f;

        protected float _currentDodgeTime;
        protected float _timeLastDodgeEnded;
        protected Coroutine _dodgeCoroutine;
        protected Vector2 _initialVelocity;
        protected Color _initialColor;
        protected int _initialLayer;
        protected Vector2 _returnPositionSpeed;

        protected virtual bool EvaluateDodgeConditions() {
            // If we aren't in cooldown, return true
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

        /// <summary>
        /// Perform dodge
        /// </summary>
        protected override void PerformAbility() {
            base.PerformAbility();
            if (!AbilityAuthorized || !EvaluateDodgeConditions()) {
                return;
            }

            // Spend the EX or do nothing if there is insufficient EX
            if (!(_character as PlayerCharacterIRE).SpendEXBars(EXCost)) {
                return;
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
            LevelManagerIRE_PW.Instance.TemporarilyAddSpeed(DodgeLevelSpeedBoost, DodgeDuration);
            _initialColor = _character.Model.color;
            _initialVelocity = _character.CharacterRigidBody.velocity;
            _character.ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Dodging);

            Color newColor = _initialColor;
            newColor.a = 0.2f;
            _character.Model.color = newColor;
            _initialLayer = _character.gameObject.layer;
            _character.gameObject.layer = LayerMask.NameToLayer("Dodging");     // Change collision layer temporarily
            //_character.CharacterRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            //_character.SetInvincibilityDuration(DodgeDuration);
            _character.ApplyForce(DodgeForce);
            _currentDodgeTime = 0f;

            yield return new WaitForSeconds(DodgeDuration);

            GameManagerIRE_PW.Instance.ResetTimeScale();
            _character.ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Normal);
            transform.SafeSetTransformPosition(transform.position, LayerMask.GetMask("Enemy"), PaywallExtensions.SetTransformModes.PlusX);
            _character.gameObject.layer = _initialLayer;
            _character.Model.color = _initialColor;
            //_character.CharacterRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _currentDodgeTime = 0f;
            _timeLastDodgeEnded = Time.time;
        }
    }
}
