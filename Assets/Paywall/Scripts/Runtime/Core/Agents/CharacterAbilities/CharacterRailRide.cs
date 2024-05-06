using MoreMountains.Tools;
using System;
using UnityEngine;

namespace Paywall {

    public class CharacterRailRide : CharacterAbilityIRE {
        #region Property Fields

        /// Level speed increase while railriding
        [field: Tooltip("Level speed increase while railriding")]
        [field: SerializeField] public float RailSpeedBonus { get; protected set; } = 20f;

        #endregion

        protected Guid _guid;
        protected Rail _rail;
        protected bool _railRiding;

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
                return true;
            }
            return false;
        }

        /// <summary>
        /// If true, stop rail riding
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldStopRailRiding() {
            if (Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Dodging || Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Teleporting) {
                return true;
            }
            return false;
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding && ShouldStopRailRiding()) {
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
            if (Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding && Character.ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Normal) {
                Vector2 closest = Physics2D.ClosestPoint(Character.CharacterBoxCollider.bounds.min, _rail.EdgeCollider);
                Character.CharacterRigidBody.velocity = new Vector2(Character.CharacterRigidBody.velocity.x, 0);
                Character.CharacterRigidBody.position = new Vector2(transform.position.x, closest.y + Character.CharacterBoxCollider.bounds.extents.y - Character.CharacterBoxCollider.offset.y + Character.CharacterBoxCollider.edgeRadius);
            }
        }

        /// <summary>
        /// Once valid contact is made with the rail, start rail riding
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void StartRailRide(Collider2D collider) {
            if (collider.gameObject.CompareTag(PaywallTagManager.RailTag)
                    && EvaluateRailRideConditions()) {

                Character.MovementState.ChangeState(CharacterStates_PW.MovementStates.RailRiding);
                _rail = collider.gameObject.GetComponent<Rail>();
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
