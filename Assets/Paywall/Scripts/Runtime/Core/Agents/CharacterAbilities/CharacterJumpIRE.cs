using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Paywall.Tools;
using MoreMountains.Tools;
using MoreMountains.InfiniteRunnerEngine;
using static Paywall.LaunchPad;

namespace Paywall {

	public enum JumpTypes { Normal, Low }

    public class CharacterJumpIRE : CharacterAbilityIRE, MMEventListener<PaywallModuleEvent>, MMEventListener<MMGameEvent> {
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
		[field: SerializeField] public float CooldownBetweenJumps { get; protected set; } = 0.05f;
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
		[field: SerializeField] public bool UseDoubleJumpHeight { get; protected set; } = true;
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
		[field: SerializeField] public bool UseJumpHeight { get; protected set; } = true;
		/// Jump height
		[field: Tooltip("Jump height")]
		[field: FieldCondition("UseJumpHeight", true)]
		[field: SerializeField] public float JumpHeight { get; protected set; } = 3f;
		/// The duration after the first jump is performed to check if we need to do a low or high jump
		[field: Tooltip("The duration after the first jump is performed to check if we need to do a low or high jump")]
		[field: SerializeField] public int LowJumpBufferFrames { get; protected set; } = 6;

		[field: MMReadOnly]
		[field: SerializeField] public int NumberOfJumpsLeft { get; protected set; }

		[field: Header("Launchers")]

		/// If true, override the provided launch height from LaunchPad
		[Tooltip("If true, override the provided launch height from LaunchPad")]
		[field: SerializeField] public bool OverrideLaunchHeight { get; protected set; }
		[field: FieldCondition("OverrideLaunchHeight", true)]
		[field: SerializeField] public float LaunchHeight { get; protected set; } = 2f;

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

		[field: Header("Buffers")]

		/// How long of a buffer the jump input has
		[field: Tooltip("How long of a buffer the jump input has")]
		[field: SerializeField] public float JumpBuffer { get; protected set; }

		[field: Header("Jump Time")]

		/// How long of a buffer the jump input has
		[field: Tooltip("How long of a buffer the jump input has")]
		[field: SerializeField] public bool OverrideJumpTimes { get; protected set; }
		/// How long of a buffer the jump input has
		[field: Tooltip("How long of a buffer the jump input has")]
		[field: FieldCondition("OverrideJumpTimes", true)]
		[field: SerializeField] public float NormalJumpTime { get; protected set; }
		/// How long of a buffer the jump input has
		[field: Tooltip("How long of a buffer the jump input has")]
		[field: FieldCondition("OverrideJumpTimes", true)]
		[field: SerializeField] public float LowJumpTime { get; protected set; }

		[field: Header("Upgrades and Modules")]

        /// The extra jump module SO
        [field: Tooltip("The extra jump module SO")]
        [field: SerializeField] public ScriptableModule ExtraJumpModule { get; protected set; }

        [field: Header("Debug")]

		/// How long of a buffer the jump input has
		[field: Tooltip("How long of a buffer the jump input has")]
		[field: SerializeField] public bool GetJumpTime { get; protected set; }

        //public int FinalNumJumpsAllowed => ExtraJumpModule.IsActive ? NumberOfJumpsAllowed : NumberOfJumpsAllowed + 1;
		public int FinalNumJumpsAllowed { get; protected set; }

        protected bool _jumping = false;
		protected bool _doubleJumping = false;
		protected float _lastJumpTime;

		protected bool _noJumpFalling;	// are we airborne without having jumped?

		protected bool _jumpCancelled;
		protected Coroutine _jumpCoroutine;
		protected Coroutine _bufferCoroutine;
		protected bool _bufferingJump;
		protected float _jumpForce;		// Jump force applied to normal jump
		protected bool _inJumpStartup;
		protected int _jumpFrames;

		protected const string _playerIgnoreTag = "PlayerIgnore";
		protected LaunchPad _launchPad;
		protected const string _jumpUpgradeName = "Jump";

		protected float _origin;

        protected override void Initialization() {
            base.Initialization();
            if (UseJumpHeight) {
                _jumpForce = CalculateJumpForce(JumpHeight);
            }
            else {
                _jumpForce = JumpForce;
            }
			FinalNumJumpsAllowed = NumberOfJumpsAllowed;
        }

        #region Every Frame

        public override void ProcessAbility() {
			// we reset our jump variables if needed
			if (_character.Grounded) {
				if ((Time.time - _lastJumpTime > MinimalDelayBetweenJumps)
					&& (Time.time - _lastJumpTime > UngroundedDurationAfterJump)) {

					// Debug
					if (_jumping && GetJumpTime) {
                        Debug.Log("Jump time: " + (Time.time - _lastJumpTime));
                        Debug.Log("Calculated normal jump time: " + CalculateJumpTime(JumpTypes.Normal));
                        Debug.Log("Calculated low jump time: " + CalculateJumpTime(JumpTypes.Low));
                    }

					_jumping = false;
					NumberOfJumpsLeft = FinalNumJumpsAllowed;
				}

				_noJumpFalling = false;
				_doubleJumping = false;
			} 
			else {
				if (!_jumping) {
					_noJumpFalling = true;
					_character.MovementState.ChangeState(CharacterStates_PW.MovementStates.Falling);
                }
            }
			// If airborne without having jumped, decrement jump count if we are still at max jumps
			if (_noJumpFalling && (NumberOfJumpsLeft == FinalNumJumpsAllowed)) {
				NumberOfJumpsLeft--;
            }
		}

		// Debug
		bool falling;

		/// <summary>
		/// Slows jump if we released the button
		/// </summary>
        protected virtual void FixedUpdate() {
			if (_character.Grounded) {
				_jumpFrames = 0;
				_jumpCancelled = false;

				// Debug
				if (_rigidbody2D.velocity.y == 0) {
					_origin = _rigidbody2D.position.y;
					falling = false;
				}
				
			}
			if (_jumping) {
				_jumpFrames++;
			}

			// Slow the jump if we released the jump button
			if (!UseJumpStartUp && _jumpCancelled && !_doubleJumping) {
				if (_jumpFrames > LowJumpBufferFrames) {
					// As soon as buffer frames end, begin slow
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

			// Debug
			if (!falling) {
				if (_rigidbody2D.velocity.y < 0) {
					//Debug.Log("Time: " + (Time.time - _lastJumpTime) + " ... " + "Height: " + (_rigidbody2D.position.y - _origin));
					falling = true;
                }
            }
		}

		/// <summary>
		/// Handle jump button pressed and released
		/// </summary>
        protected override void HandleInput() {
			if (!AbilityAuthorized) {
				return;
			}

            if (InputSystemManager_PW.InputActions.PlayerControls.Jump.WasPressedThisFrame()) {
                if (!UseJumpStartUp) {
                    JumpNoStartup();
                }
                else {
                    JumpWithStartup();
                }
            }

			if (JumpProportionalToPress) {
				// we cancel the jump if the jump button was released before the low jump buffer ended
				if (InputSystemManager_PW.InputActions.PlayerControls.Jump.WasReleasedThisFrame()) {
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

        }

		/// <summary>
		/// Called every frame. Launches the character if it came into contact with a launch pad.
		/// </summary>
		protected virtual void HandleLaunchPad() {
			if ((Time.time - _lastJumpTime > MinimalDelayBetweenJumps)
					&& (Time.time - _lastJumpTime > UngroundedDurationAfterJump)) {
				if (_character.Ground != null && _character.Ground.CompareTag("LaunchPad")) {
					_launchPad = _character.Ground.GetComponent<LaunchPad>();
					switch (_launchPad.LaunchType) {
                        case LaunchTypes.Height:
                            ApplyExternalJumpForce(LaunchTypes.Height, _launchPad.LaunchHeight);
                            break;
                        case LaunchTypes.Force:
                            ApplyExternalJumpForce(LaunchTypes.Force, _launchPad.LaunchForce);
                            break;
                        case LaunchTypes.Jump:
                            break;
                    }
				}
			}
		}

        #endregion

        #region Callbacks

        /// <summary>
        /// Jump.performed callback
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void JumpPerformed(InputAction.CallbackContext ctx) {
			if (!UseJumpStartUp) {
				JumpNoStartup();
			}
			else {
				JumpWithStartup();
			}
		}

		/// <summary>
		/// Jump.canceled callback
		/// </summary>
		/// <param name="ctx"></param>
		protected virtual void JumpCanceled(InputAction.CallbackContext ctx) {
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

        #endregion

        /// <summary>
        /// Checks if we are allowed to jump, return false if not
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateJumpConditions() {
			if (GameManagerIRE_PW.HasInstance) {
				if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
					return false;
				}
			}

			// if the character is not grounded and is only allowed to jump when grounded, we do not jump
			if (JumpsAllowedWhenGroundedOnly && !_character.Grounded) {
				return false;
			}

			// if the character doesn't have any jump left, we do not jump
			if (NumberOfJumpsLeft <= 0) {
				return false;
			}

			// if we're still in cooldown from the last jump AND this is not the first jump, we do not jump
			if ((Time.time - _lastJumpTime < CooldownBetweenJumps) && (NumberOfJumpsLeft != FinalNumJumpsAllowed)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Buffer a jump, then jump if conditions allow
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator BufferJump() {
			_bufferingJump = true;
			yield return new WaitForSeconds(JumpBuffer);
			_bufferingJump = false;
			if (EvaluateJumpConditions()) {
				JumpNoStartup();
            }
        }

		/// <summary>
		/// Evaluates jump conditions and initiates jump if allowed
		/// No jump startup, use InitiateJump() if using jump startup
		/// </summary>
		public virtual void JumpNoStartup() {
			if (_noJumpFalling && (NumberOfJumpsLeft == FinalNumJumpsAllowed)) {
				NumberOfJumpsLeft--;
			}

			if (!EvaluateJumpConditions()) {
				if (_bufferCoroutine != null) {
					StopCoroutine(_bufferCoroutine);
				}
				if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.GameInProgress) {
					_bufferCoroutine = StartCoroutine(BufferJump());
				}
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

        #region Jump With Startup

        /// <summary>
        /// Evaluates jump conditions and initiates jump if allowed
        /// If using jump startup, use this instead of Jump()
        /// </summary>
        public virtual void JumpWithStartup() {
			if (!EvaluateJumpConditions()) {
				if (_bufferCoroutine != null) {
					StopCoroutine(_bufferCoroutine);
				}
				if ((GameManagerIRE_PW.Instance as GameManagerIRE_PW).Status == GameManagerIRE_PW.GameStatus.GameInProgress) {
					_bufferCoroutine = StartCoroutine(BufferJump());
				}
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
		/// Only used with jump startup
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

        #endregion

        /// <summary>
        /// Performs a jump or double jump and updates animator and state
        /// </summary>
        protected virtual void PerformJump() {
			_character.MovementState.ChangeState(CharacterStates_PW.MovementStates.Jumping);
			_jumpCancelled = false;
			if (_bufferCoroutine != null) {
				StopCoroutine(_bufferCoroutine);
			}

			_lastJumpTime = Time.time;
			// we jump and decrease the number of jumps left
			NumberOfJumpsLeft--;

			// if the character isn't grounded, we reset its velocity and gravity
			if (!_character.Grounded) {
				_rigidbody2D.velocity = Vector3.zero;
				_rigidbody2D.gravityScale = _initialGravityScale;
			}
			_rigidbody2D.velocity = Vector3.zero;
			_rigidbody2D.gravityScale = _initialGravityScale;

			// we make our character jump
			// if the character is already airborne, use the double jump
			if (!_character.Grounded) {
				if (UseDoubleJumpHeight) {
					ApplyJumpForce(CalculateJumpForce(DoubleJumpHeight));
				}
				else {
					ApplyJumpForce(DoubleJumpForce);
				}
			}
			else {
				ApplyJumpForce(_jumpForce);
			}
			if (!_character.Grounded) {
				_doubleJumping = true;
			}
			MMEventManager.TriggerEvent(new MMGameEvent("Jump"));

			_lastJumpTime = Time.time;
			_jumping = true;
			if (_animator != null) {
				MMAnimatorExtensions.UpdateAnimatorTriggerIfExists(_animator, "JustJumped");
			}
		}

		/// <summary>
		/// Applies upward impulse force to character
		/// </summary>
		/// <param name="force"></param>
		protected virtual void ApplyJumpForce(float force) {
			_rigidbody2D.AddForce(Vector3.up * force, ForceMode2D.Impulse);
		}

		/// <summary>
		/// Called by external components to apply force to this character (like LaunchPad)
		/// If useForce is false, the force param is instead a height, and we need to calculate the force required to attain that height
		/// </summary>
		/// <param name="force"></param>
		/// <param name="useForce"></param>
		public virtual void ApplyExternalJumpForce(LaunchTypes launchType, float force, bool resetJumps = true) {
			_rigidbody2D.velocity = Vector3.zero;
			_rigidbody2D.gravityScale = _initialGravityScale;
			_lastJumpTime = Time.time;
			_noJumpFalling = true;
			if (resetJumps) {
				NumberOfJumpsLeft = FinalNumJumpsAllowed - 1;
			}
			else {
				NumberOfJumpsLeft--;
			}

			if (OverrideLaunchHeight) {
				float jumpForce = CalculateJumpForce(LaunchHeight);
				_rigidbody2D.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
			}
			else {
                float jumpForce;
                switch (launchType) {
                    case LaunchTypes.Height:
                        jumpForce = CalculateJumpForce(force);
                        _rigidbody2D.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
                        break;
                    case LaunchTypes.Force:
                        _rigidbody2D.AddForce(Vector3.up * force, ForceMode2D.Impulse);
                        break;
                    case LaunchTypes.Jump:
						_rigidbody2D.AddForce(Vector3.up * _jumpForce, ForceMode2D.Impulse);
                        break;
                }
			}
            MMEventManager.TriggerEvent(new MMGameEvent("Jump"));
        }

        /// <summary>
        /// Set number of remaining jumps
        /// </summary>
        /// <param name="jumpsLeft"></param>
        public virtual void SetJumpsLeft(int jumpsLeft) {
			NumberOfJumpsLeft = jumpsLeft;
		}

		/// <summary>
		/// Set number of jumps allowed
		/// </summary>
		/// <param name="jumps"></param>
		public virtual void SetNumberJumpsAllowed(int jumps) {
			NumberOfJumpsAllowed = jumps;
		}

		/// <summary>
		/// Calculates the duration of a normal full jump
		/// </summary>
		/// <returns></returns>
		public virtual float CalculateJumpTime(JumpTypes jumpType) {
			float jumpTime = 0;
			float Trelease, Vinitial, Vrelease, Tpeak, Vfinal, Tfall;
			switch (jumpType) {
				case JumpTypes.Normal:
					Vinitial = CalculateJumpForce(JumpHeight) / _rigidbody2D.mass;
					Tpeak = (-Vinitial) / (Physics2D.gravity.y * _initialGravityScale);
					Vfinal = Mathf.Sqrt(Mathf.Abs(2f * (Physics2D.gravity.y * _initialGravityScale) * JumpHeight));
					Tfall = (Vfinal) / Mathf.Abs((Physics2D.gravity.y * _initialGravityScale));
					jumpTime = Tpeak + Tfall;
					break;
				case JumpTypes.Low:
					Trelease = LowJumpBufferFrames * Time.fixedDeltaTime;
					Vinitial = CalculateJumpForce(JumpHeight) / _rigidbody2D.mass;
					Vrelease = Vinitial + (Physics2D.gravity.y * _initialGravityScale) * Trelease;
					Tpeak = (-Vrelease / (-JumpReleaseSpeed + (Physics2D.gravity.y * _initialGravityScale)));
					Vfinal = Mathf.Sqrt(Mathf.Abs(2f * (Physics2D.gravity.y * _initialGravityScale) * LowJumpHeight));
					Tfall = (Vfinal) / Mathf.Abs((Physics2D.gravity.y * _initialGravityScale));
					jumpTime = Trelease + Tpeak + Tfall;
					break;
            }
			
			return jumpTime;
        }

		/// <summary>
		/// Calculates the jump force required to attain the given height
		/// </summary>
		/// <param name="height"></param>
		/// <returns></returns>
		protected virtual float CalculateJumpForce(float height) {
			return Mathf.Sqrt(height * -2 * (Physics2D.gravity.y * _initialGravityScale)) * _rigidbody2D.mass;
		}

		/// <summary>
		/// Reset ability. Resets number of jumps left
		/// </summary>
        public override void ResetAbility() {
			NumberOfJumpsLeft = FinalNumJumpsAllowed;
        }

		/// <summary>
		/// Catches changes to the extra jump module and adjusts this component accordingly
		/// </summary>
		/// <param name="moduleEvent"></param>
        public void OnMMEvent(PaywallModuleEvent moduleEvent) {
            if (moduleEvent.Module.Name.Equals(ExtraJumpModule.Name)) {
				if (PaywallProgressManager.Instance.ModulesDict[moduleEvent.Module.Name].IsActive) {
					FinalNumJumpsAllowed++;
				}
				else {
                    FinalNumJumpsAllowed--;
				}
			}
        }

		public void OnMMEvent(MMGameEvent gameEvent) {
			if (gameEvent.EventName.Equals("Rescue")) {
				NumberOfJumpsLeft = FinalNumJumpsAllowed - 1;
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			this.MMEventStartListening<PaywallModuleEvent>();
			this.MMEventStartListening<MMGameEvent>();
			//InputSystemManager_PW.InputActions.PlayerControls.Jump.performed += JumpPerformed;
			//InputSystemManager_PW.InputActions.PlayerControls.Jump.canceled += JumpCanceled;
		}

		protected override void OnDisable() {
			base.OnDisable();
			this.MMEventStopListening<PaywallModuleEvent>();
			this.MMEventStopListening<MMGameEvent>();
			//InputSystemManager_PW.InputActions.PlayerControls.Jump.performed -= JumpPerformed;
			//InputSystemManager_PW.InputActions.PlayerControls.Jump.canceled -= JumpCanceled;
		}

	}
}
