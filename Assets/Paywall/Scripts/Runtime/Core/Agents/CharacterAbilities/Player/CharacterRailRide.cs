using MoreMountains.Tools;
using Paywall.Tools;
using System;
using UnityEngine;

namespace Paywall {

    public class CharacterRailRide : CharacterAbilityIRE {
        #region Property Fields

        /// Level speed increase while railriding
        [field: Tooltip("Level speed increase while railriding")]
        [field: SerializeField] public float RailSpeedBonus { get; protected set; } = 20f;
        /// Can we press Down+Jump to drop down from the rail?
        [field: Tooltip("Can we press Down+Jump to drop down from the rail?")]
        [field: SerializeField] public bool CanDropDown { get; protected set; }
        /// After dropping down from the rail, how long to block RailRiding, so that we aren't snapped back into it immediately
        [field: Tooltip("After dropping down from the rail, how long to block RailRiding, so that we aren't snapped back into it immediately")]
        [field: FieldCondition("CanDropDown", true)]
        [field: SerializeField] public float BlockDuration { get; protected set; } = 0.2f;

        #endregion

        protected Guid _guid;
        protected Rail _rail;
        protected bool _railRiding;
        protected float _bottomColliderBuffer = 0f;
        protected float _dropTime;

        protected override void Initialization() {
            base.Initialization();
            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Can we start rail riding?
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateRailRideConditions() {
            if (Character.CharacterRigidBody.velocity.y <= 0
                    && Character.MovementState.CurrentState != CharacterStates_PW.MovementStates.RailRiding
                    && Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Normal) {
                // If we just dropped and are blocking RailRiding
                if (CanDropDown && Time.time - _dropTime < BlockDuration) {
                    return false;
                }
                // If the bottom of the character is too far below the rail, do not railride
                Vector2 closest = Physics2D.ClosestPoint(Character.CharacterBoxCollider.bounds.min, _rail.EdgeCollider);
                if (closest.y > Character.CharacterBoxCollider.bounds.min.y - _bottomColliderBuffer) {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// If true, stop rail riding
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldStopRailRiding() {
            if (Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding) {
                if (Character.Grounded) {
                    return false;
                }
                if (Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Dodging 
                        || Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Teleporting) {
                    return true;
                }
            }
            return false;
        }

        protected override void HandleInput() {
            base.HandleInput();
            if (!CanDropDown || !AbilityAuthorized || !_railRiding) {
                return;
            }

            // If we are allowed to drop down, stop railriding
            if (InputActions.PlayerControls.Down.IsPressed()
                    && InputActions.PlayerControls.Jump.WasPerformedThisFrame()) {
                _dropTime = Time.time;
                StopRailRide();
            }
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (ShouldStopRailRiding()) {
                StopRailRide();
            }
        }

        protected virtual void FixedUpdate() {
            RailRide();
        }

        /// <summary>
        /// Ride the rail by setting character Y position to the Y position of the closest point on the rail EdgeCollider2D
        /// Position is offset by size of character collider, so that the bottom of the character is the part that is contacting the rail
        /// </summary>
        protected virtual void RailRide() {
            if (Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding && Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Normal
                && !Character.Grounded) {
                Vector2 closest = Physics2D.ClosestPoint(Character.CharacterBoxCollider.bounds.min, _rail.EdgeCollider);
                Character.CharacterRigidBody.velocity = new Vector2(Character.CharacterRigidBody.velocity.x, 0);
                Character.CharacterRigidBody.position = 
                    new Vector2(Character.CharacterRigidBody.position.x, closest.y + Character.CharacterBoxCollider.bounds.extents.y - Character.CharacterBoxCollider.offset.y + Character.CharacterBoxCollider.edgeRadius);
                Vector2 pos = transform.position;
                if (pos != Character.CharacterRigidBody.position) {
                    transform.position = new(Character.CharacterRigidBody.position.x, Character.CharacterRigidBody.position.y, transform.position.z);
                }
            }
        }

        /// <summary>
        /// Once valid contact is made with the rail, start rail riding
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void StartRailRide(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.RailTag) && AbilityAuthorized && !Character.Grounded) {

                _rail = collider.gameObject.GetComponent<Rail>();

                if (!EvaluateRailRideConditions()) {
                    return;
                }

                Character.MovementState.ChangeState(CharacterStates_PW.MovementStates.RailRiding);
                Character.CharacterRigidBody.gravityScale = 0;
                LevelManagerIRE_PW.Instance.TemporarilyAddSpeedSwitch(RailSpeedBonus, _guid);
                _railRiding = true;
                RailRide();
            }
        }

        /// <summary>
        /// Stop rail ride. End speed boost and set character state.
        /// </summary>
        protected virtual void StopRailRide() {
            if (!_railRiding) {
                return;
            }
            if (Character.MovementState.CurrentState != CharacterStates_PW.MovementStates.Jumping) {
                Character.MovementState.ChangeState(CharacterStates_PW.MovementStates.Falling);
            }
            if (Character.ConditionState.CurrentState != CharacterStates_PW.ConditionStates.Dodging) {
                Character.CharacterRigidBody.gravityScale = _initialGravityScale;
            }
            _railRiding = false;
            LevelManagerIRE_PW.Instance.TemporarilyAddSpeedSwitch(RailSpeedBonus, _guid);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider) {
            StartRailRide(collider);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider) {
            StartRailRide(collider);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.RailTag)) {
                StopRailRide();
            }
        }

        public override void UpdateAnimator() {
            base.UpdateAnimator();
            MMAnimatorExtensions.UpdateAnimatorBoolIfExists(_animator, "RailRiding", Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding);
        }
    }
}
