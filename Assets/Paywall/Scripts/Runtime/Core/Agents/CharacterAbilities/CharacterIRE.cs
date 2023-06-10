using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using Paywall.Tools;

namespace Paywall {

	public enum CharacterTypes { Player, AI }
	public enum InvincibilityTypes { Damage, PowerUp }

	/// <summary>
	/// Playable character controller
	/// </summary>
    public class CharacterIRE : MonoBehaviour_PW, MMEventListener<PaywallEXChargeEvent> {
		/// Type of character this is
		[field: Tooltip("Type of character this is")]
		[field: SerializeField] public CharacterTypes CharacterType { get; protected set; } = CharacterTypes.AI;

		[field: Header("Sprite")]

		/// The character's sprite renderer. If left blank, component will use GetComponent to find it
		[field: Tooltip("The character's sprite renderer. If left blank, component will use GetComponent to find it")]
		[field: SerializeField] public SpriteRenderer Model { get; protected set; }
		/// Flicker the sprite when hit
		[field: Tooltip("Flicker the sprite when hit")]
		[field: SerializeField] public bool FlickerSpriteOnHit { get; protected set; }
		/// Flicker duration. Sprite flickers between flicker color and normal color, this is the duration it will rest on either one before flickering to the other color.
		[field: Tooltip("Flicker duration. Sprite flickers between flicker color and normal color, this is the duration it will rest on either one before flickering to the other color.")]
		[field: FieldCondition("FlickerSpriteOnHit", true)]
		[field: SerializeField] public float FlickerDuration { get; protected set; } = 0.1f;

		[field: Header("Animator")]

		/// should we use the default mecanim ?
		[field: Tooltip("should we use the default mecanim ?")]
		[field: SerializeField] public bool UseDefaultMecanim { get; protected set; } = true;

		[field: Header("Position")]

		/// returns true if the character is currently grounded
		// if true, the object will try to go back to its starting position
		[field: Tooltip("if true, the object will try to go back to its starting position")]
		[field: SerializeField] public bool ShouldResetPosition { get; protected set; } = true;
		// the speed at which the object should try to go back to its starting position
		[field: Tooltip("the speed at which the object should try to go back to its starting position")]
		[field: SerializeField] public float ResetPositionSpeed { get; protected set; } = 0.5f;

		[field: Header("Grounded")]

		/// the distance between the character and the ground
		[field: Tooltip("the distance between the character and the ground")]
		[field: MMReadOnly]
		[field: SerializeField] public float DistanceToTheGround { get; protected set; }
		/// the distance tolerance at which a character is considered grounded
		[field: Tooltip("the distance tolerance at which a character is considered grounded")]
		[field: SerializeField] public float GroundDistanceTolerance { get; protected set; } = 0.05f;

		[field: Header("Invincibility")]

		/// the duration (in seconds) of invincibility on spawn
		[field: Tooltip("the duration (in seconds) of invincibility on spawn")]
		[field: SerializeField] public float InitialInvincibilityDuration { get; protected set; } = 3f;

		/// The duration of the invincibility which activates upon taking damage/spawning
		[field: Tooltip("The duration of the invincibility which activates upon taking damage/spawning")]
		[field: SerializeField] public float TempInvincibilityDuration { get; protected set; } = 1f;
		/// Is the character currently invincible
		[field: Tooltip("Is the character currently invincible")]
		[field: SerializeField] public bool Invincible { get; protected set; }

		[field: Header("EX")]

		/// Current EX charge
		[field: Tooltip("Current EX charge")]
		[field: SerializeField] public float CurrentEX { get; protected set; }
        /// Minimum possible EX charge
        [field: Tooltip("Minimum possible EX charge")]
        [field: SerializeField] public float MinEX { get; protected set; } = 0f;
        /// Maximum possible EX charge
        [field: Tooltip("Maximum possible EX charge")]
		[field: SerializeField] public float MaxEX { get; protected set; } = 50f;
		/// Value of a single EX bar
		[field: Tooltip("Value of a single EX bar")]
		[field: SerializeField] public float OneEXBarValue { get; protected set; } = 10f;
        /// EX drain per second acceleration rate. The longer EX drains for, the faster it drains.
        [field: Tooltip("EX drain per second acceleration rate. The longer EX drains for, the faster it drains.")]
        [field: SerializeField] public float EXDrainRateAcceleration { get; protected set; } = 2f;
        /// Block EX gain while EX bar is draining
        [field: Tooltip("Block EX gain while EX bar is draining")]
        [field: SerializeField] public bool BlockEXGainWhileDraining { get; protected set; }
        /// Gain EX passively over time?
        [field: Tooltip("Gain EX passively over time?")]
        [field: SerializeField] public bool GainEXOverTime { get; protected set; }
		/// EX gain rate per second
		[field: Tooltip("EX gain rate per second")]
		[field: FieldCondition("GainEXOverTime", true)]
		[field: SerializeField] public float EXGainPerSecond { get; protected set; } = 2.5f;

        // State Machines
        /// the movement state machine 
        public MMStateMachine<CharacterStates_PW.MovementStates> MovementState { get; protected set; }
		/// the condition state machine
		public MMStateMachine<CharacterStates_PW.ConditionStates> ConditionState { get; protected set; }

		public InputSystemManager_PW LinkedInputManager { get; protected set; }
		public bool Grounded { get; protected set; }
		public GameObject Ground { get { return _ground; } }
		public float DistanceToRight { get; protected set; }
		[field: MMReadOnly]
		[field: SerializeField] public bool CollidingRight { get; protected set; }
		public Animator CharacterAnimator { get; protected set; }
		public Vector3 InitialPosition { get; protected set; }

		protected float _distanceToTheGroundRaycastLength = 10f;
		protected GameObject _ground;
		protected LayerMask _collisionMaskSave;
		protected float _awakeAt;
		protected GameObject _rightObject;

		protected Vector3 _raycastLeftOrigin;
		protected Vector3 _raycastRightOrigin;

		protected bool _initialInvincibilityActive = true;

		protected bool _initialized;
		protected float _remainingInvincibility;
		protected float _initialGravity;
		protected Color _initialColor;
		protected Color _flickerColor;
		protected Coroutine _flickerCoroutine;

		public Rigidbody2D CharacterRigidBody { get; protected set; }
		public BoxCollider2D CharacterBoxCollider { get; protected set; }

		protected Vector2 _boundsTopLeftCorner;
		protected Vector2 _boundsTopRightCorner;
		protected Vector2 _boundsBottomLeftCorner;
		protected Vector2 _boundsBottomRightCorner;
		protected Vector2 _boundsCenter;

		protected float _exDrainRate;	// EX drain rate per second
		protected bool _EXDraining;

		protected const string _playerIgnoreTag = "PlayerIgnore";

		CharacterAbilityIRE[] _characterAbilities;

		protected virtual void Awake() {
			if (!_initialized) {
				Initialization();
			}
		}

		protected virtual void Initialization() {
			if (Model == null) {
				Model = GetComponentInChildren<SpriteRenderer>();
            }
			_initialColor = _flickerColor = Model.color;
			_flickerColor.a = 0.1f;
			MovementState = new MMStateMachine<CharacterStates_PW.MovementStates>(gameObject, true);
			ConditionState = new MMStateMachine<CharacterStates_PW.ConditionStates>(gameObject, true);
			LinkedInputManager = (InputSystemManager_PW)FindAnyObjectByType(typeof(InputSystemManager_PW), FindObjectsInactive.Include);
			CharacterRigidBody = gameObject.MMGetComponentNoAlloc<Rigidbody2D>();
			_initialGravity = CharacterRigidBody.gravityScale;
			CharacterBoxCollider = gameObject.MMGetComponentNoAlloc<BoxCollider2D>();
			CacheAbilities();
			if (GUIManagerIRE_PW.HasInstance) {
				GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX, MinEX, MaxEX);
			}
			_initialized = true;
		}

		/// <summary>
		/// Caches list of CharacterAbilities attached to this character
		/// </summary>
		protected virtual void CacheAbilities() {
			_characterAbilities = GetComponentsInChildren<CharacterAbilityIRE>();
			foreach (CharacterAbilityIRE ability in _characterAbilities) {
				ability.SetCharacter(this);
            }
        }

		protected virtual void Update() {

			// we determine the distance between the ground and the character
			ComputeDistanceToTheGround();
			if (Grounded) {
                MovementState.ChangeState(CharacterStates_PW.MovementStates.Running);
            }

			// Handle EX gain/drain over time
			HandleEX();

            EarlyProcessAbilities();
			ProcessAbilities();
			LateProcessAbilities();

			HandleInvincibility();

			// we send our various states to the animator.      
			UpdateAnimator();

			// if we're supposed to reset the player's position, we lerp its position to its initial position
			//ResetPosition();

			// we check if the player is out of the death bounds or not
			CheckDeathConditions();
			
		}

		protected virtual void EarlyProcessAbilities() {
			foreach (CharacterAbilityIRE ability in _characterAbilities) {
				ability.EarlyProcessAbility();
			}
		}

		protected virtual void ProcessAbilities() {
			foreach (CharacterAbilityIRE ability in _characterAbilities) {
				ability.ProcessAbility();
			}
		}

		protected virtual void LateProcessAbilities() {
			foreach (CharacterAbilityIRE ability in _characterAbilities) {
				ability.LateProcessAbility();
			}
		}

		protected virtual void FixedProcessAbilities() {

        }

		public virtual void ResetCharacter() {
			CharacterRigidBody.gravityScale = _initialGravity;
			CharacterRigidBody.velocity = Vector3.zero;
			ResetAbilities();
        }

		/// <summary>
		/// Reset all character abilities
		/// </summary>
		protected virtual void ResetAbilities() {
			foreach (CharacterAbilityIRE ability in _characterAbilities) {
				ability.ResetAbility();
			}
		}

		/// <summary>
		/// Update rigidbody
		/// </summary>
		protected virtual void FixedUpdate() {
			CheckCollisionRight();
			// if we're supposed to reset the player's position, we lerp its position to its initial position
			ResetPosition();
		}

		/// <summary>
		/// Use this to define the initial position of the agent. Used mainly for reset position purposes
		/// </summary>
		/// <param name="initialPosition">Initial position.</param>
		public virtual void SetInitialPosition(Vector3 initialPosition) {
			InitialPosition = initialPosition;
		}

		public virtual void Die() {
			Destroy(this);
        }

		protected virtual bool CheckCollisionRight() {
			Vector2 topOrigin = new(CharacterBoxCollider.bounds.max.x, CharacterBoxCollider.bounds.max.y);
			Vector2 bottomOrigin = new(CharacterBoxCollider.bounds.max.x, CharacterBoxCollider.bounds.min.y);
			int RaysToCast = 5;
			RaycastHit2D raycastHit2D;

			for (int i = 0; i < RaysToCast; i++) {
				Vector2 originPoint = Vector2.Lerp(topOrigin, bottomOrigin, i / (RaysToCast - 1));
				raycastHit2D = Physics2D.Raycast(originPoint, Vector2.right, 1f + CharacterBoxCollider.edgeRadius, 1 << LayerMask.NameToLayer("Ground"));
				if (raycastHit2D.collider != null) {
					if (raycastHit2D.distance < (GroundDistanceTolerance + CharacterBoxCollider.edgeRadius)) {
						CollidingRight = true;
						_rightObject = raycastHit2D.collider.gameObject;
						if (raycastHit2D.distance <= 0) {
							//float vx = raycastHit2D.collider.gameObject.GetComponent<>
							//CharacterRigidBody.velocity = new Vector2(CharacterRigidBody.velocity.y);
                        }
						return true;
					}
				}
            }
			CollidingRight = false;
			_rightObject = null;
			return false;
		}

		#region PlayableCharacter overrides

		protected virtual void ComputeDistanceToTheGround() {

			DistanceToTheGround = -1;

			_raycastLeftOrigin = CharacterBoxCollider.bounds.min;
			_raycastRightOrigin = CharacterBoxCollider.bounds.min;
			_raycastRightOrigin.x = CharacterBoxCollider.bounds.max.x;

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
			if ((CharacterType == CharacterTypes.Player) && (_ground != null) && (_ground.CompareTag(_playerIgnoreTag))) {
				_ground = null;
				Grounded = false;
				return;
            }
			Grounded = DetermineIfGroudedConditionsAreMet();
		}

		/// <summary>
		/// Determines if grouded conditions are met.
		/// </summary>
		/// <returns><c>true</c>, if if grouded conditions are met was determined, <c>false</c> otherwise.</returns>
		protected virtual bool DetermineIfGroudedConditionsAreMet() {
			// if the distance to the ground is equal to -1, this means the raycast never found the ground, thus there's no ground, thus the character isn't grounded anymore
			if (DistanceToTheGround == -1) {
				return (false);
			}
			// if the distance to the ground is within the tolerated bounds, the character is grounded, otherwise it's not.
			if (DistanceToTheGround < (GroundDistanceTolerance + CharacterBoxCollider.edgeRadius)) {
				return (true);
			}
			else {
				return (false);
			}
		}

		protected virtual void CheckDeathConditions() {
			if ((GameManagerIRE_PW.Instance.Status == GameManagerIRE_PW.GameStatus.GameInProgress) && (LevelManagerIRE_PW.Instance.CheckDeathCondition(CharacterBoxCollider.bounds))) {
				LevelManagerIRE_PW.Instance.KillCharacterOutOfBounds(this);
			}
		}

		/// <summary>
		/// Gets the playable character bounds.
		/// </summary>
		/// <returns>The playable character bounds.</returns>
		protected virtual Bounds GetPlayableCharacterBounds() {
			if (gameObject.MMGetComponentNoAlloc<Collider>() != null) {
				return gameObject.MMGetComponentNoAlloc<Collider>().bounds;
			}

			if (gameObject.MMGetComponentNoAlloc<Collider2D>() != null) {
				return gameObject.MMGetComponentNoAlloc<Collider2D>().bounds;
			}

			return gameObject.MMGetComponentNoAlloc<Renderer>().bounds;
		}

		/// <summary>
		/// This is called at Update() and sets each of the animators parameters to their corresponding State values
		/// </summary>
		protected virtual void UpdateAnimator() {
			if (CharacterAnimator == null) { return; }

			// we send our various states to the animator.		
			if (UseDefaultMecanim) {
				UpdateAllMecanimAnimators();
			}
		}

		/// <summary>
		/// Updates all mecanim animators.
		/// </summary>
		protected virtual void UpdateAllMecanimAnimators() {
			MMAnimatorExtensions.UpdateAnimatorBoolIfExists(CharacterAnimator, "Grounded", Grounded);
			MMAnimatorExtensions.UpdateAnimatorFloatIfExists(CharacterAnimator, "VerticalSpeed", CharacterRigidBody.velocity.y);
		}

		/// <summary>
		/// Called on update, tries to return the object to its initial position
		/// </summary>
		protected virtual void ResetPosition() {
			if (ShouldResetPosition) {
				if (!CollidingRight && transform.position.x != InitialPosition.x && (ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Normal)) {
					CharacterRigidBody.velocity = new Vector3((InitialPosition.x - transform.position.x) * (ResetPositionSpeed), CharacterRigidBody.velocity.y);
					//CharacterRigidBody.velocity = new Vector3(0, CharacterRigidBody.velocity.y);
				}
			}
		}

		#endregion

		/// <summary>
		/// External functions can call this to apply force to the character's rigidbody
		/// Use the CharacterJumpIRE component 
		/// </summary>
		/// <param name="force"></param>
		public virtual void ApplyForce(Vector2 force) {
			CharacterRigidBody.AddForce(force * CharacterRigidBody.mass);
		}

		/// <summary>
		/// External functions can call this to set the rigidbody gravity scale
		/// </summary>
		/// <param name="gravityScale"></param>
		public virtual void SetGravityScale(float gravityScale) {
			CharacterRigidBody.gravityScale = gravityScale;
		}

		public virtual void ResetGravityScale() {
			CharacterRigidBody.gravityScale = _initialGravity;
		}

		public virtual void SetInvincibility(float duration) {
			Invincible = true;
			_remainingInvincibility = duration;
		}

		public virtual void ActivateTempInvincibility() {
			Invincible = true;
			_remainingInvincibility = TempInvincibilityDuration;
			if (_flickerCoroutine != null) {
				StopCoroutine(_flickerCoroutine);
            }
			_flickerCoroutine = StartCoroutine(MMImage.Flicker(Model, _initialColor, _flickerColor, FlickerDuration, _remainingInvincibility));
		}

		protected virtual void HandleInvincibility() {
			if (_remainingInvincibility > 0) {
				_remainingInvincibility -= Time.deltaTime;
			}
			else {
				Invincible = false;
			}
		}

		#region Raycasts

		protected virtual void CastRaysBelow() {
			SetRaysParameters();
			Vector3 raycastLeftOrigin = CharacterBoxCollider.bounds.min;
			Vector3 raycastRightOrigin = CharacterBoxCollider.bounds.min;
			raycastRightOrigin.x = CharacterBoxCollider.bounds.max.x;
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
			float top = CharacterBoxCollider.offset.y + (CharacterBoxCollider.size.y / 2f);
			float bottom = CharacterBoxCollider.offset.y - (CharacterBoxCollider.size.y / 2f);
			float left = CharacterBoxCollider.offset.x - (CharacterBoxCollider.size.x / 2f);
			float right = CharacterBoxCollider.offset.x + (CharacterBoxCollider.size.x / 2f);

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
			_boundsCenter = CharacterBoxCollider.bounds.center;
		}

		#endregion

		/// <summary>
		/// Set max EX
		/// </summary>
		/// <param name="newMax"></param>
		public virtual void SetMaxEX(float newMax) {
			MaxEX = newMax;
			GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX, MinEX, newMax);
		}

		/// <summary>
		/// Spends EX bar
		/// </summary>
		/// <returns></returns>
		public virtual bool SpendEXBars(int bars) {
			float EXCost = OneEXBarValue * bars;
			if (CurrentEX >= EXCost) {
				CurrentEX -= EXCost;
				GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX);
				return true;
			}
			return false;
		}

        /// <summary>
        /// Spend EX over time (like for supers)
        /// </summary>
        /// <param name="rate">Drain rate per second</param>
        /// <param name="bars">Minimum bars required to activate</param>
        public virtual void SpendEXOverTime(float rate, int bars) {
			if (Mathf.FloorToInt(CurrentEX / OneEXBarValue) < bars) {
				return;
			}
			_exDrainRate = rate;
			_EXDraining = true;
		}

		/// <summary>
		/// Handles EX draining and/or increasing every frame
		/// </summary>
		protected virtual void HandleEX() {
			if (_EXDraining) {
				CurrentEX -= _exDrainRate * Time.deltaTime;		// Drain EX by drain rate
                _exDrainRate += EXDrainRateAcceleration * Time.deltaTime;	// Increase drain rate
                if (CurrentEX <= MinEX) {
					CurrentEX = MinEX;
					_EXDraining = false;
					_exDrainRate = 0f;
				}
			}
			if (GainEXOverTime) {
				CurrentEX += EXGainPerSecond * Time.deltaTime;
			}
			GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX);
		}

		/// <summary>
		/// Catch charge events, add or set EX charge
		/// EX gain is blocked while EX is draining
		/// </summary>
		/// <param name="chargeEvent"></param>
		public virtual void OnMMEvent(PaywallEXChargeEvent chargeEvent) {
			// Add EX amount
			if (chargeEvent.ChangeAmountMethod == ChangeAmountMethods.Add) {
				if (BlockEXGainWhileDraining) {
					if ((chargeEvent.ChargeAmount > 0) && _EXDraining) {
						return;
					}
				}
				CurrentEX += chargeEvent.ChargeAmount;
				// If EX overflows, cap it to MaxEX
				if (CurrentEX > MaxEX) {
					CurrentEX = MaxEX;
				}
            }
			// Set EX amount
            else {
				if (BlockEXGainWhileDraining) {
					if ((chargeEvent.ChargeAmount > CurrentEX) && _EXDraining) {
						return;
					}
				}
                CurrentEX = chargeEvent.ChargeAmount;
            }
        }

		protected virtual void OnEnable() {
			if (!_initialized) {
				Initialization();
			}
            this.MMEventStartListening<PaywallEXChargeEvent>();
        }

        protected virtual void OnDisable() {
			StopAllCoroutines();
			this.MMEventStopListening<PaywallEXChargeEvent>();
		}

	}
}
