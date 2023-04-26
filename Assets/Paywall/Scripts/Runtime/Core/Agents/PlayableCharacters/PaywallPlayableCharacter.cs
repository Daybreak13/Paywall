using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using UnityEngine.InputSystem;
using Paywall.Tools;

namespace Paywall {

    /// <summary>
    /// Playable character for Paywall
    /// </summary>
    public class PaywallPlayableCharacter : PlayableCharacter {
        #region Property Fields

        [field: Header("Jumper")]

		/// the vertical force applied to the character when jumping
		[field: Tooltip("the vertical force applied to the character when jumping")]
		[field: FieldCondition("UseJumpHeight", true, true)]
		[field: SerializeField] public float JumpForce { get; protected set; } = 15f;
		/// the number of jumps allowed
		[field: Tooltip("the number of jumps allowed")]
		[field: SerializeField] public int NumberOfJumpsAllowed { get; protected set; } = 2;
		/// the minimum time (in seconds) allowed between two consecutive jumps
		[field: Tooltip("the minimum time (in seconds) allowed between two consecutive jumps")]
		[field: SerializeField] public float CooldownBetweenJumps { get; protected set; } = 0f;
		/// can the character jump only when grounded ?
		[field: Tooltip("can the character jump only when grounded ?")]
		[field: SerializeField] public bool JumpsAllowedWhenGroundedOnly { get; protected set; }
		/// the speed at which the character falls back down again when the jump button is released
		[field: Tooltip("the speed at which the character falls back down again when the jump button is released")]
		[field: SerializeField] public float JumpReleaseSpeed { get; protected set; } = 50f;
		/// if this is set to false, the jump height won't depend on the jump button release speed
		[field: Tooltip("if this is set to false, the jump height won't depend on the jump button release speed")]
		[field: SerializeField] public bool JumpProportionalToPress { get; protected set; }
		/// the minimal time, in seconds, that needs to have passed for a new jump to be authorized
		[field: Tooltip("the minimal time, in seconds, that needs to have passed for a new jump to be authorized")]
		[field: SerializeField] public float MinimalDelayBetweenJumps { get; protected set; } = 0.02f;
		/// a duration, after a jump, during which the character can't be considered grounded (to avoid jumps left to be reset too soon depending on context)
		[field: Tooltip("a duration, after a jump, during which the character can't be considered grounded (to avoid jumps left to be reset too soon depending on context)")]
		[field: SerializeField] public float UngroundedDurationAfterJump { get; protected set; } = 0.2f;

		[field: Header("Secondary Jumps")]

		/// If true, the jumps after the first will have a different height than the first
		[field: Tooltip("If true, the jumps after the first will have a different height than the first")]
		[field: SerializeField] public bool UseDoubleJumpHeight { get; protected set; }
		/// If true, the jumps after the first will have a different height than the first
		[field: Tooltip("If true, the jumps after the first will have a different height than the first")]
		[field: FieldCondition("UseDoubleJumpHeight", true, true)]
		[field: SerializeField] public float DoubleJumpForce { get; protected set; } = 10f;
		/// If true, the jumps after the first will have a different height than the first
		[field: Tooltip("If true, the jumps after the first will have a different height than the first")]
		[field: FieldCondition("UseDoubleJumpHeight", true)]
		[field: SerializeField] public float DoubleJumpHeight { get; protected set; } = 2f;

		[field: Header("Other Jump Settings")]

		/// If true, use jump height instead of jump force
		[field: Tooltip("If true, use jump height instead of jump force")]
		[field: SerializeField] public bool UseJumpHeight { get; protected set; } 
		/// Jump height
		[field: Tooltip("Jump height")]
		[field: FieldCondition("UseJumpHeight", true)]
		[field: SerializeField] public float JumpHeight { get; protected set; } = 3f;
		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: SerializeField] public float LowJumpBuffer { get; protected set; } = 0.2f;
		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: SerializeField] public int LowJumpBufferFrames { get; protected set; } = 12;

		[field: MMReadOnly]
		[field: SerializeField] public int NumberOfJumpsLeft { get; protected set; }

		[field: Header("Jump Startup")]

		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: FieldCondition("UseJumpHeight", true, true)]
		[field: SerializeField] public float LowJumpForce { get; protected set; } = 10f;
		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: FieldCondition("UseJumpHeight", true)]
		[field: SerializeField] public float LowJumpHeight { get; protected set; } = 2f;
		[field: SerializeField] public bool UseJumpStartUp { get; protected set; }
		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: FieldCondition("UseJumpStartUp", true)]
		[field: SerializeField] public float JumpStartup { get; protected set; } = 0.1f;

		[field: Header("Other Settings")]

		/// The duration of the invincibility which activates upon taking damage/spawning
		[field: Tooltip("The duration of the invincibility which activates upon taking damage/spawning")]
		[field: SerializeField] public float TempInvincibilityDuration { get; protected set; }

		#endregion

		protected bool _jumping = false;
		protected bool _doubleJumping = false;
		protected float _lastJumpTime;

		protected IREInputActions _inputActions;
		protected bool _initialized;
		protected bool _noJumpFalling;
		protected float _initialGravity;
		protected Rigidbody2D _rigidbody2D;
		protected float _remainingInvincibility;
		protected BoxCollider2D _boxCollider;

		protected Vector2 _boundsTopLeftCorner;
		protected Vector2 _boundsTopRightCorner;
		protected Vector2 _boundsBottomLeftCorner;
		protected Vector2 _boundsBottomRightCorner;
		protected Vector2 _boundsCenter;

		protected bool _jumpCancelled;
		protected Coroutine _jumpCoroutine;
		protected float _jumpForce;
		protected bool _inJumpStartup;
		protected int _jumpFrames;

		protected float _origin;

		protected override void Awake() {
			base.Awake();
        }

		protected override void Start() {
            base.Start();
			_lastJumpTime = Time.time;
			NumberOfJumpsLeft = NumberOfJumpsAllowed;
			if (!_initialized) {
				Initialization();
			}
        }

		protected virtual void Initialization() {
			_inputActions = new();
			_rigidbody2D = GetComponent<Rigidbody2D>();
			_boxCollider = GetComponent<BoxCollider2D>();
			_initialGravity = _rigidbody2D.gravityScale;
			if (UseJumpHeight) {
				JumpForce = CalculateJumpForce(JumpHeight);
				DoubleJumpForce = CalculateJumpForce(DoubleJumpHeight);
            }
        }

        protected override void Update() {
			base.Update();

			// we reset our jump variables if needed
			if (_grounded) {
				if ((Time.time - _lastJumpTime > MinimalDelayBetweenJumps)
					&& (Time.time - _lastJumpTime > UngroundedDurationAfterJump)) {
					_jumping = false;
					NumberOfJumpsLeft = NumberOfJumpsAllowed;
				}

				_noJumpFalling = false;
				_doubleJumping = false;
			}
		}

		/// <summary>
		/// Update rigidbody
		/// </summary>
		protected virtual void FixedUpdate() {
			if (_grounded) {
				_jumpFrames = 0;
				_jumpCancelled = false;
				_origin = _rigidbody2D.position.y;
            }
			if (_jumping) {
				_jumpFrames++;
            }

			if (!UseJumpStartUp && _jumpCancelled && !_doubleJumping) {
				if (_jumpFrames > LowJumpBufferFrames) {
					if (_rigidbody2D.velocity.y > 0) {
						Vector3 newGravity = Vector3.up * (_rigidbody2D.velocity.y - JumpReleaseSpeed * Time.fixedDeltaTime);
						if (newGravity.y <= 0) {
							newGravity.y = 0;
							//Debug.Log(_rigidbody2D.position.y - _origin);
						}
						_rigidbody2D.velocity = new Vector3(_rigidbody2D.velocity.x, newGravity.y, 0f);
					}
					else {
						_jumpCancelled = false;
					}
				}
			}
		}
		
		/// <summary>
		/// Updates all mecanim animators.
		/// </summary>
		protected override void UpdateAllMecanimAnimators() {
			MMAnimatorExtensions.UpdateAnimatorBoolIfExists(_animator, "Grounded", _grounded);
			MMAnimatorExtensions.UpdateAnimatorBoolIfExists(_animator, "Jumping", _jumping);
			MMAnimatorExtensions.UpdateAnimatorFloatIfExists(_animator, "VerticalSpeed", _rigidbody2D.velocity.y);
		}

		#region Jumper

		/// <summary>
		/// What happens when the main action button button is pressed
		/// </summary>
		public override void MainActionStart() {
			if (!UseJumpStartUp) {
				Jump();
			} else {
				InitiateJump();
            }
		}

		/// <summary>
		/// Evaluates jump conditions and initiates jump if allowed
		/// No jump startup, use InitiateJump() if using jump startup
		/// </summary>
		public virtual void Jump() {
			if (!EvaluateJumpConditions()) {
				return;
			}

			if (UseJumpHeight) {
				_jumpForce = CalculateJumpForce(JumpHeight);
			}
			else {
				_jumpForce = JumpForce;
			}
			_jumpCancelled = false;
			PerformJump();
		}

		/// <summary>
		/// Evaluates jump conditions and initiates jump if allowed
		/// If using jump startup, use this instead of Jump()
		/// </summary>
		public virtual void InitiateJump() {
			if (!EvaluateJumpConditions()) {
				return;
			}

			if (!_jumping) {
				_jumpCoroutine = StartCoroutine(StartJump());
			}
			else {
				PerformJump();
			}
		}

		/// <summary>
		/// If grounded (first jump), wait for jump startup, then jump
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator StartJump() {
			_inJumpStartup = true;
			yield return new WaitForSeconds(JumpStartup);

			if (_jumpCancelled) {
				if (UseJumpHeight) {
					_jumpForce = CalculateJumpForce(LowJumpHeight);
				}
				else {
					_jumpForce = LowJumpForce;
				}
			}
			else {
				if (UseJumpHeight) {
					_jumpForce = CalculateJumpForce(JumpHeight);
				}
				else {
					_jumpForce = JumpForce;
				}
			}
			_jumpCancelled = false;
			_inJumpStartup = false;
			PerformJump();
		}

		public virtual void StopJump() {
			StopCoroutine(_jumpCoroutine);
			_jumpCancelled = false;
			_inJumpStartup = false;
        }

		/// <summary>
		/// Performs a jump or double jump and updates animator and state
		/// </summary>
		protected virtual void PerformJump() {
			_jumpCancelled = false;

			_lastJumpTime = Time.time;
			// we jump and decrease the number of jumps left
			NumberOfJumpsLeft--;

			// if the character isn't grounded, we reset its velocity and gravity
			if (!_grounded) {
				_rigidbody2D.velocity = Vector3.zero;
				_rigidbody2D.gravityScale = _initialGravity;
			}
			_rigidbody2D.velocity = Vector3.zero;
			_rigidbody2D.gravityScale = _initialGravity;

			// we make our character jump
			// if the character is already airborne, use the double jump
			if (UseDoubleJumpHeight && !_grounded) {
				ApplyJumpForce(DoubleJumpForce);
			}
			else {
				ApplyJumpForce(_jumpForce);
			}
			if (_jumping) {
				_doubleJumping = true;
            }
			MMEventManager.TriggerEvent(new MMGameEvent("Jump"));

			_lastJumpTime = Time.time;
			_jumping = true;
			if (_animator != null) {
				MMAnimatorExtensions.UpdateAnimatorTriggerIfExists(_animator, "JustJumped");
			}
		}

		protected virtual void ApplyJumpForce(float force) {
			_rigidbody2D.AddForce(Vector3.up * force, ForceMode2D.Impulse);
		}

		/// <summary>
		/// Called by external components to apply force to this character
		/// If useForce is false, the force param is instead a height, and we need to calculate the force required to attain that height
		/// </summary>
		/// <param name="force"></param>
		/// <param name="useForce"></param>
		public virtual void ApplyExternalJumpForce(float force, bool useForce = true, bool resetJumps = true) {
			_rigidbody2D.velocity = Vector3.zero;
			_rigidbody2D.gravityScale = _initialGravity;
			_lastJumpTime = Time.time;
			_noJumpFalling = true;
			if (resetJumps) {
				NumberOfJumpsLeft = NumberOfJumpsAllowed - 1;
			}
			else {
				NumberOfJumpsLeft--;
			}
			if (useForce) {				
				_rigidbody2D.AddForce(Vector3.up * JumpForce, ForceMode2D.Impulse);
			} else {
				float jumpForce = CalculateJumpForce(force);
				_rigidbody2D.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            }
		}

		/// <summary>
		/// What happens when the main action button button is released
		/// </summary>
		public override void MainActionEnd() {
			// we cancel the jump if necessary
			if (JumpProportionalToPress) {
				if (!UseJumpStartUp) {
					if (_jumping && !_doubleJumping && (_jumpFrames < LowJumpBufferFrames)) {
						_jumpCancelled = true;
					}
				}
				else {
					if (_inJumpStartup) {
						_jumpCancelled = true;
					}
					else {
						_jumpCancelled = false;
					}
				}
			}
		}

		protected virtual bool EvaluateJumpConditions() {
			if (GameManager.HasInstance) {
				if (GameManager.Instance.Status != GameManager.GameStatus.GameInProgress) {
					return false;
				}
			}

			// if the character is not grounded and is only allowed to jump when grounded, we do not jump
			if (JumpsAllowedWhenGroundedOnly && !_grounded) {
				return false;
			}

			// if the character doesn't have any jump left, we do not jump
			if (NumberOfJumpsLeft <= 0) {
				return false;
			}

			// if we're still in cooldown from the last jump AND this is not the first jump, we do not jump
			if ((Time.time - _lastJumpTime < CooldownBetweenJumps) && (NumberOfJumpsLeft != NumberOfJumpsAllowed)) {
				return false;
			}

			return true;
		}

        #endregion

        #region PlayableCharacter overrides

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

        #endregion

		public virtual void SetJumpsLeft(int jumpsLeft) {
			NumberOfJumpsLeft = jumpsLeft;
        }

		public virtual void SetInvincibility(float duration) {
			Invincible = true;
			_remainingInvincibility = duration;
        }

		public virtual void ActivateTempInvincibility() {
			Invincible = true;
			_remainingInvincibility = TempInvincibilityDuration;
        }

        protected override void CheckInvincibility() {
            base.CheckInvincibility();
			if (_remainingInvincibility > 0) {
				_remainingInvincibility -= Time.deltaTime;
			} else {
				Invincible = false;
            }
		}

		protected virtual double CalculateDistanceTraveled() {
			float timeElapsed = Time.time - _lastJumpTime;
			double distance;
			distance = _jumpForce * timeElapsed + 0.5 * Physics2D.gravity.y * _rigidbody2D.gravityScale * Mathf.Pow(timeElapsed, 2);
			return distance;
		}

        /// <summary>
        /// Calculates the jump force required to attain the given height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        protected virtual float CalculateJumpForce(float height) {
			return Mathf.Sqrt(height * -2 * (Physics2D.gravity.y * _initialGravity));
		}

        #region Raycasts

        protected virtual void CastRaysBelow() {
			SetRaysParameters();
			Vector3 raycastLeftOrigin = _boxCollider.bounds.min;
			Vector3 raycastRightOrigin = _boxCollider.bounds.min;
			raycastRightOrigin.x = _boxCollider.bounds.max.x;
			GameObject hitObject = null;
			float distanceToObject;

			// we cast a ray to the bottom to check if we're above ground and determine the distance
			RaycastHit2D raycastLeft = MMDebug.RayCast(raycastLeftOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
			if (raycastLeft) {
				distanceToObject = raycastLeft.distance;
				hitObject = raycastLeft.collider.gameObject;
			}
			RaycastHit2D raycastRight = MMDebug.RayCast(raycastRightOrigin, Vector2.down, _distanceToTheGroundRaycastLength, 1 << LayerMask.NameToLayer("Ground"), Color.gray, true);
			if (raycastRight) {
				if (raycastLeft) {
					if (raycastRight.distance < DistanceToTheGround) {
						distanceToObject = raycastRight.distance;
						hitObject = raycastRight.collider.gameObject;
					}
				}
				else {
					distanceToObject = raycastRight.distance;
					hitObject = raycastRight.collider.gameObject;
				}
			}

			if (!raycastLeft && !raycastRight) {
				// if the raycast hasn't hit anything, we set the distance to -1
				distanceToObject = -1;
				hitObject = null;
			}
			
		}

		/// <summary>
		/// Creates a rectangle with the boxcollider's size for ease of use and draws debug lines along the different raycast origin axis
		/// </summary>
		public virtual void SetRaysParameters() {
			float top = _boxCollider.offset.y + (_boxCollider.size.y / 2f);
			float bottom = _boxCollider.offset.y - (_boxCollider.size.y / 2f);
			float left = _boxCollider.offset.x - (_boxCollider.size.x / 2f);
			float right = _boxCollider.offset.x + (_boxCollider.size.x / 2f);

			_boundsTopLeftCorner.x = left;
			_boundsTopLeftCorner.y = top;

			_boundsTopRightCorner.x = right;
			_boundsTopRightCorner.y = top;

			_boundsBottomLeftCorner.x = left;
			_boundsBottomLeftCorner.y = bottom;

			_boundsBottomRightCorner.x = right;
			_boundsBottomRightCorner.y = bottom;

			_boundsTopLeftCorner = transform.TransformPoint(_boundsTopLeftCorner);
			_boundsTopRightCorner = transform.TransformPoint(_boundsTopRightCorner);
			_boundsBottomLeftCorner = transform.TransformPoint(_boundsBottomLeftCorner);
			_boundsBottomRightCorner = transform.TransformPoint(_boundsBottomRightCorner);
			_boundsCenter = _boxCollider.bounds.center;
		}

        #endregion

        protected virtual void OnEnable() {
			if (!_initialized) {
				Initialization();
            }
			_inputActions.Enable();
        }

		protected virtual void OnDisable() {
			_inputActions.Disable();
			StopAllCoroutines();
        }

    }
}
