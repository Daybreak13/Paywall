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
    public class PlayerCharacterIRE : CharacterIRE, MMEventListener<PaywallEXChargeEvent>, MMEventListener<RunnerItemPickEvent>, MMEventListener<PaywallKillEvent>, MMEventListener<MMGameEvent> {

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
        /// Is EX currently draining?
        [field: Tooltip("Is EX currently draining?")]
		[field: MMReadOnly]
        [field: SerializeField] public bool EXDraining { get; protected set; }
        /// EX drain per second acceleration rate. The longer EX drains for, the faster it drains.
        [field: Tooltip("EX drain per second acceleration rate. The longer EX drains for, the faster it drains.")]
        [field: SerializeField] public float EXDrainRateAcceleration { get; protected set; } = 2f;
        /// Block EX gain while EX bar is draining
        [field: Tooltip("Block EX gain while EX bar is draining")]
        [field: SerializeField] public bool BlockEXGainWhileDraining { get; protected set; }
		/// EX gained on kill
		[field: Tooltip("EX gained on kill")]
		[field: SerializeField] public float KillEXGain { get; protected set; } = 10f;
        /// EX lost when a life is lost
        [field: Tooltip("EX lost when a life is lost")]
        [field: SerializeField] public float EXLifeLost { get; protected set; } = 10f;
        /// Gain EX passively over time?
        [field: Tooltip("Gain EX passively over time?")]
        [field: SerializeField] public bool GainEXOverTime { get; protected set; }
		/// EX gain rate per second
		[field: Tooltip("EX gain rate per second")]
		[field: FieldCondition("GainEXOverTime", true)]
		[field: SerializeField] public float EXGainPerSecond { get; protected set; } = 2.5f;

		[field: Header("Upgrades and Modules")]

        /// The fall rescue module SO
        [field: Tooltip("The fall rescue module SO")]
        [field: SerializeField] public ScriptableModule RescueModule { get; protected set; }

        public Vector3 InitialPosition { get; protected set; }

		protected float _exDrainRate;	// EX drain rate per second
		protected bool _exDraining;
		protected float _exLastFrame;
		protected Vector2 _savedVelocity;

        protected override void Initialization() {
            if (GUIManagerIRE_PW.HasInstance) {
                GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX, MinEX, MaxEX);
            }
            base.Initialization();
        }

        protected override void Update() {
            // Handle EX gain/drain over time
            HandleEX();
            base.Update();
            // we check if the player is out of the death bounds or not
            CheckDeathConditions();
		}

        protected override void FixedUpdate() {
            base.FixedUpdate();
			ResetPosition();
        }

        /// <summary>
        /// Use this to define the initial position of the agent. Used mainly for reset position purposes
        /// </summary>
        /// <param name="initialPosition">Initial position.</param>
        public virtual void SetInitialPosition(Vector3 initialPosition) {
			InitialPosition = initialPosition;
		}

		#region PlayableCharacter overrides

		/// <summary>
		/// Checks if the character is out of bounds, and kill it if so
		/// </summary>
		protected virtual void CheckDeathConditions() {
			if ((GameManagerIRE_PW.Instance.Status == GameManagerIRE_PW.GameStatus.GameInProgress) && (LevelManagerIRE_PW.Instance.CheckDeathCondition(CharacterBoxCollider.bounds))) {
				if (PaywallProgressManager.Instance.ModulesDict[RescueModule.Name].IsActive) {
					Rescue();
                }
				else {
                    LevelManagerIRE_PW.Instance.KillCharacterOutOfBounds(this);
                }
            }
		}

		/// <summary>
		/// Rescues player when going OOB
		/// </summary>
		protected virtual void Rescue() {
			ActivateDamageInvincibility();
			transform.SafeSetTransformPosition(InitialPosition, LayerMask.GetMask("Ground"), PaywallExtensions.SetTransformModes.PlusX);
			CharacterRigidBody.velocity = Vector3.zero;
            MMGameEvent.Trigger("Rescue");
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
		/// Called on fixed update, tries to return the object to its initial position
		/// </summary>
		protected virtual void ResetPosition() {
			if (ShouldResetPosition && !Teleporting) {
				if (transform.position.x != InitialPosition.x && (ConditionState.CurrentState == CharacterStates_PW.ConditionStates.Normal)) {
					float deltaX = InitialPosition.x - transform.position.x;
					if (Mathf.Abs(deltaX) > 0.1f) {
						CharacterRigidBody.velocity = new Vector2((InitialPosition.x - transform.position.x) * ResetPositionSpeed, CharacterRigidBody.velocity.y);
					}
					else {
						CharacterRigidBody.velocity = new(0, CharacterRigidBody.velocity.y);
						CharacterRigidBody.MovePosition(new Vector2(InitialPosition.x, transform.position.y));
					}
				}
			}
			// When we teleport, the character's x position gets pushed forward, so we need to reset it
			else if (Teleporting) {
                float deltaX = transform.position.x - InitialPosition.x;
				float d = CharacterRigidBody.velocity.x * Time.fixedDeltaTime;
				if (transform.position.x - Mathf.Abs(d) > InitialPosition.x
					) {
                    CharacterRigidBody.velocity = new Vector2(-LevelManagerIRE_PW.Instance.TeleportSpeed * LevelManagerIRE_PW.Instance.SpeedMultiplier + TeleportResetBuffer, CharacterRigidBody.velocity.y);
                }
				// If moving d distance would overshoot, we use regular reset position speed to slowly get the rest of the way
                else {
                    //CharacterRigidBody.velocity = new(0, CharacterRigidBody.velocity.y);
                    //CharacterRigidBody.MovePosition(new Vector2(InitialPosition.x, transform.position.y));
                    CharacterRigidBody.velocity = new Vector2((InitialPosition.x - transform.position.x) * ResetPositionSpeed, CharacterRigidBody.velocity.y);
                    Teleporting = false;
                }
            }
		}

		#endregion

		/// <summary>
		/// Sets character state if it is teleporting
		/// </summary>
		/// <param name="on"></param>
		public virtual void SetTeleportState(bool on) {
			if (on) {
				ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Teleporting);
                gameObject.layer = PaywallLayerManager.TeleportingLayer;
				Model.enabled = false;
				_savedVelocity = CharacterRigidBody.velocity;
			}
			else {
				ConditionState.ChangeState(CharacterStates_PW.ConditionStates.Normal);
                gameObject.layer = PaywallLayerManager.PlayerLayer;	
                Model.enabled = true;
				CharacterRigidBody.velocity = _savedVelocity;
			}
		}

		public virtual void TeleportResetPosition() {

		}

        /// <summary>
        /// Checks the EX draining flag
        /// </summary>
        /// <param name="draining"></param>
        public virtual void SetEXDraining(bool draining) {
			EXDraining = draining;
		}

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
		/// Adds EX to current EX
		/// </summary>
		/// <param name="amount"></param>
		public virtual void AddEX(float amount) {
			if ((BlockEXGainWhileDraining && _exDraining && (amount > 0))
					|| (CurrentEX == MaxEX && amount > 0)
					|| (CurrentEX == MinEX && amount < 0)) {
				return;
			}
			CurrentEX += amount;
			if (CurrentEX > MaxEX) {
				CurrentEX = MaxEX;
			}
			if (CurrentEX < MinEX) {
				CurrentEX = MinEX;
			}
            GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX);
        }

        /// <summary>
        /// Spend EX over time (like for supers)
        /// </summary>
        /// <param name="rate">Drain rate per second</param>
        /// <param name="bars">Minimum bars required to activate</param>
        public virtual bool SpendEXOverTime(float rate, int bars) {
			if (Mathf.FloorToInt(CurrentEX) < bars) {
				return false;
			}
			_exDrainRate = rate;
			_exDraining = true;
			return true;
		}

		/// <summary>
		/// Handles EX draining and/or increasing every frame
		/// Do nothing if game is not in progress
		/// </summary>
		protected virtual void HandleEX() {
			if (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.GameInProgress) {
				return;
			}
			if (_exDraining) {
				AddEX(_exDrainRate * Time.deltaTime);		// Drain EX by drain rate
                _exDrainRate += EXDrainRateAcceleration * Time.deltaTime;	// Increase drain rate
                if (CurrentEX <= MinEX) {
					_exDraining = false;
					_exDrainRate = 0f;
				}
			}
			// Gain EX over time. Do not gain EX if EX is draining
			if (GainEXOverTime && !EXDraining) {
				AddEX(EXGainPerSecond * Time.deltaTime);
			}
			if (_exLastFrame != CurrentEX) {
                GUIManagerIRE_PW.Instance.UpdateEXBar(CurrentEX);
            }
			_exLastFrame = CurrentEX;
		}

		/// <summary>
		/// Catch charge events, add or set EX charge
		/// EX gain is blocked while EX is draining
		/// </summary>
		/// <param name="chargeEvent"></param>
		public virtual void OnMMEvent(PaywallEXChargeEvent chargeEvent) {
			// Add EX amount
			if (chargeEvent.ChangeAmountMethod == ChangeAmountMethods.Add) {
				// If the game is paused when EX is added, that means it was added from the depot, so do not block
				if (BlockEXGainWhileDraining && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.Paused)) {
					if ((chargeEvent.ChargeAmount > 0) && EXDraining) {
						return;
					}
				}
				AddEX(chargeEvent.ChargeAmount);
            }
			// Set EX amount
            else {
				if (BlockEXGainWhileDraining && (GameManagerIRE_PW.Instance.Status != GameManagerIRE_PW.GameStatus.Paused)) {
					if ((chargeEvent.ChargeAmount > CurrentEX) && EXDraining) {
						return;
					}
				}
                CurrentEX = chargeEvent.ChargeAmount;
            }
        }

		/// <summary>
		/// When a powerup is picked, apply its effects to the player
		/// </summary>
		/// <param name="itemPickEvent"></param>
		public virtual void OnMMEvent(RunnerItemPickEvent itemPickEvent) {
			if (itemPickEvent.PickedPowerUpType == PowerUpTypes.EX) {
                AddEX(itemPickEvent.Amount);
            }
		}

		/// <summary>
		/// Gain EX when the player kills something
		/// </summary>
		/// <param name="killEvent"></param>
		public virtual void OnMMEvent(PaywallKillEvent killEvent) {
			if (killEvent.PlayerInstigated) {
				AddEX(KillEXGain);
			}
        }

        public void OnMMEvent(MMGameEvent eventType) {
            if (eventType.EventName.Equals("LifeLost")) {
				AddEX(-EXLifeLost);

                if (_flickerCoroutine != null) {
                    StopCoroutine(_flickerCoroutine);
                }
				_remainingInvincibility = -1f;
            }
        }

        protected virtual void OnEnable() {
			if (!_initialized) {
				Initialization();
			}
            this.MMEventStartListening<PaywallEXChargeEvent>();
            this.MMEventStartListening<RunnerItemPickEvent>();
            this.MMEventStartListening<PaywallKillEvent>();
            this.MMEventStartListening<MMGameEvent>();
        }

        protected override void OnDisable() {
			base.OnDisable();
			this.MMEventStopListening<PaywallEXChargeEvent>();
			this.MMEventStopListening<RunnerItemPickEvent>();
			this.MMEventStopListening<PaywallKillEvent>();
			this.MMEventStopListening<MMGameEvent>();
		}
    }
}
