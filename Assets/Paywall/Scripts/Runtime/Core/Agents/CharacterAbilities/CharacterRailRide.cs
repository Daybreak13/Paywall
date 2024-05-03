using UnityEngine;

namespace Paywall {

    public class CharacterRailRide : CharacterAbilityIRE {
        protected Rail _rail;

        protected virtual bool EvaluateRailRideConditions() {
            if (Character.CharacterRigidBody.velocity.y < 0
                && Character.MovementState.CurrentState != CharacterStates_PW.MovementStates.RailRiding) {

                return true;
            }
            return false;
        }

        protected virtual void FixedUpdate() {
            if (Character.MovementState.CurrentState == CharacterStates_PW.MovementStates.RailRiding) {
                Vector2 closet = Physics2D.ClosestPoint(Character.CharacterBoxCollider.bounds.min, _rail.EdgeCollider);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.CompareTag(PaywallTagManager.RailTag)
                && Character.CharacterRigidBody.velocity.y < 0 
                && Character.MovementState.CurrentState != CharacterStates_PW.MovementStates.RailRiding) {

                Character.MovementState.ChangeState(CharacterStates_PW.MovementStates.RailRiding);
                _rail = collision.gameObject.GetComponent<Rail>();
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision) {
            if (collision.gameObject.CompareTag(PaywallTagManager.RailTag)) {
                if (Character.CharacterRigidBody.velocity.y < 0 && Character.MovementState.CurrentState != CharacterStates_PW.MovementStates.RailRiding) {
                    Character.MovementState.ChangeState(CharacterStates_PW.MovementStates.RailRiding);
                    _rail = collision.gameObject.GetComponent<Rail>();
                }
            }
        }

        protected virtual void OnCollisionExit2D(Collision2D collision) {
            if (collision.gameObject.CompareTag(PaywallTagManager.RailTag)) {

            }
        }
    }
}
