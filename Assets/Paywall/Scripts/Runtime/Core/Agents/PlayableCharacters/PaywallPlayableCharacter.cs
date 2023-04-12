using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using UnityEngine.InputSystem;

namespace Paywall {

    /// <summary>
    /// Playable character for Paywall
    /// </summary>
    public class PaywallPlayableCharacter : Jumper {
		protected IREInputActions _inputActions;
		protected bool _initialized;

        protected override void Start() {
            base.Start();
			if (!_initialized) {
				Initialization();
			}
        }

		protected virtual void Initialization() {
			_inputActions = new();
        }

        protected override bool EvaluateJumpConditions() {
			if (GameManager.HasInstance) {
				if (GameManager.Instance.Status != GameManager.GameStatus.GameInProgress) {
					return false;
				}
			}
			return base.EvaluateJumpConditions();
        }

        protected override void ComputeDistanceToTheGround() {
			if (_rigidbodyInterface == null) {
				return;
			}

			DistanceToTheGround = -1;

			if (_rigidbodyInterface.Is2D) {
				_raycastLeftOrigin = _rigidbodyInterface.ColliderBounds.min;
				_raycastRightOrigin = _rigidbodyInterface.ColliderBounds.min;
				_raycastRightOrigin.x = _rigidbodyInterface.ColliderBounds.max.x;

				// we cast a ray to the bottom to check if we're above ground and determine the distance
				RaycastHit2D raycastLeft = MMDebug.RayCast(_raycastLeftOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
				if (raycastLeft) {
					DistanceToTheGround = raycastLeft.distance;
					_ground = raycastLeft.collider.gameObject;
				}
				RaycastHit2D raycastRight = MMDebug.RayCast(_raycastRightOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
				if (raycastRight) {
					if (raycastLeft) {
						if (raycastRight.distance < DistanceToTheGround) {
							DistanceToTheGround = raycastRight.distance;
							_ground = raycastRight.collider.gameObject;
						}
					}
					else {
						DistanceToTheGround = raycastRight.distance;
						_ground = raycastRight.collider.gameObject;
					}
				}

				if (!raycastLeft && !raycastRight) {
					// if the raycast hasn't hit the ground, we set the distance to -1
					DistanceToTheGround = -1;
					_ground = null;
				}
				_grounded = DetermineIfGroudedConditionsAreMet();
			}
		}

        protected override void CheckDeathConditions() {
			if (LevelManagerIRE_PW.Instance.CheckDeathCondition(GetPlayableCharacterBounds())) {
				(LevelManagerIRE_PW.Instance as LevelManagerIRE_PW).KillCharacterOutOfBounds(this);
			}
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
